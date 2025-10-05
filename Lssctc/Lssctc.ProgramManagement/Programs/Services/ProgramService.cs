using AutoMapper;
using Lssctc.ProgramManagement.HttpCustomResponse;
using Lssctc.ProgramManagement.Programs.DTOs;
using Lssctc.Share.Common;
using Lssctc.Share.Entities;
using Lssctc.Share.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Lssctc.ProgramManagement.Programs.Services
{
    public class ProgramService : IProgramService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public ProgramService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<List<ProgramDto>> GetAllPrograms()
        {
            var programs = await _unitOfWork.ProgramRepository.GetAllAsQueryable()
                .Include(p => p.ProgramEntryRequirements)
                .Include(p => p.ProgramCourses)
                    .ThenInclude(pc => pc.Courses)
                .Where(p => p.IsDeleted != true)
                .ToListAsync();
            var dtoList = _mapper.Map<List<ProgramDto>>(programs);
            foreach (var dto in dtoList)
            {
                dto.Courses = dto.Courses.OrderBy(c => c.CourseOrder).ToList();
            }
            return dtoList;
        }

        public async Task<PagedResult<ProgramDto?>> GetPrograms(ProgramQueryParameters parameters)
        {
            var query = _unitOfWork.ProgramRepository.GetAllAsQueryable();

            if (!string.IsNullOrWhiteSpace(parameters.SearchTerm))
            {
                query = query.Where(p =>
                    p.Name.Contains(parameters.SearchTerm) ||
                    (p.Description ?? "").Contains(parameters.SearchTerm));
            }

            query = query
                .Include(p => p.ProgramEntryRequirements)
                .Include(p => p.ProgramCourses)
                    .ThenInclude(pc => pc.Courses);





            var totalCount = await query.CountAsync();

            var programs = await query
                .Skip((parameters.PageNumber - 1) * parameters.PageSize)
                .Take(parameters.PageSize)
                .ToListAsync();

            var dtoList = _mapper.Map<List<ProgramDto>>(programs);

            // Order Course in the program by CourseOrder
            foreach (var dto in dtoList)
            {
                dto.Courses = dto.Courses.OrderBy(c => c.CourseOrder).ToList();
            }

            return new PagedResult<ProgramDto?>
            {
                Items = dtoList,
                TotalCount = totalCount,
                Page = parameters.PageNumber,
                PageSize = parameters.PageSize
            };
        }

        public async Task<ProgramDto?> GetProgramById(int id)
        {
            var program = await _unitOfWork.ProgramRepository.GetAllAsQueryable()
                .Include(p => p.ProgramEntryRequirements)
                .Include(p => p.ProgramCourses)
                .ThenInclude(pc => pc.Courses)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (program == null )
                return null;

            var dto = _mapper.Map<ProgramDto>(program);
            dto.Courses = dto.Courses.OrderBy(c => c.CourseOrder).ToList();

            return dto;
        }

        public async Task<ProgramDto> CreateProgram(CreateProgramDto dto)
        {
            var entity = _mapper.Map<TrainingProgram>(dto);
            entity.IsActive = true;
            entity.IsDeleted = false;
            entity.TotalCourses = 0;
            entity.DurationHours = 0;

            await _unitOfWork.ProgramRepository.CreateAsync(entity);
            await _unitOfWork.SaveChangesAsync();

            return _mapper.Map<ProgramDto>(entity);
        }
        public async Task<ProgramDto?> AddCourseToProgram(int programId, int courseId)
        {
            var program = await _unitOfWork.ProgramRepository.GetAllAsQueryable()
                .Include(p => p.ProgramCourses)
                .ThenInclude(pc => pc.Courses)
                .FirstOrDefaultAsync(p => p.Id == programId);

            if (program == null)
                return null;

            var courseToAdd = await _unitOfWork.CourseRepository
                .GetAllAsQueryable()
                .FirstOrDefaultAsync(c => c.Id == courseId && c.IsDeleted != true);

            if (courseToAdd == null)
                throw new BadRequestException($"Course with ID {courseId} is invalid or deleted.");

            if (program.ProgramCourses.Any(pc => pc.CoursesId == courseId))
                throw new BadRequestException($"Course with ID {courseId} is already part of the program.");

            var nextOrder = program.ProgramCourses.Any()
                ? program.ProgramCourses.Max(pc => pc.CourseOrder) + 1
                : 1;

            var newProgramCourse = new ProgramCourse
            {
                CoursesId = courseToAdd.Id,
                ProgramId = program.Id,
                CourseOrder = nextOrder,
                Name = program.Name,
                Description = program.Description
            };

            program.ProgramCourses.Add(newProgramCourse);

            program.TotalCourses = program.ProgramCourses.Count;
            program.DurationHours += courseToAdd.DurationHours ?? 0; // Add the new course's duration

            await _unitOfWork.ProgramRepository.UpdateAsync(program);
            await _unitOfWork.SaveChangesAsync();

            var result = _mapper.Map<ProgramDto>(program);
            result.Courses = result.Courses.OrderBy(c => c.CourseOrder).ToList();

            return result;
        }
        public async Task<ProgramDto?> AddCoursesToProgram(int programId, List<CourseOrderDto> coursesToAdd)
        {
            var program = await _unitOfWork.ProgramRepository.GetAllAsQueryable()
                .Include(p => p.ProgramCourses)
                .ThenInclude(pc => pc.Courses)
                .FirstOrDefaultAsync(p => p.Id == programId);

            if (program == null )
                return null;

            if (coursesToAdd == null || !coursesToAdd.Any())
                throw new BadRequestException("At least one course must be provided.");

            var courseIds = coursesToAdd.Select(c => c.CourseId).ToList();

            var validCourses = await _unitOfWork.CourseRepository
                .GetAllAsQueryable()
                .Where(c => courseIds.Contains(c.Id) && c.IsDeleted != true)
                .ToListAsync();

            if (validCourses.Count != coursesToAdd.Count)
                throw new BadRequestException("Some courses are invalid or deleted.");

            foreach (var courseOrderDto in coursesToAdd)
            {
                var course = validCourses.FirstOrDefault(c => c.Id == courseOrderDto.CourseId);
                if (course != null)
                {
                    program.ProgramCourses.Add(new ProgramCourse
                    {
                        CoursesId = course.Id,
                        ProgramId = program.Id,
                        CourseOrder = courseOrderDto.Order,
                        Name = program.Name,
                        Description = program.Description
                    });
                }
            }

            program.TotalCourses = program.ProgramCourses.Count;
            program.DurationHours = 0;
            foreach (var pc in program.ProgramCourses)
            {
                var course = await _unitOfWork.CourseRepository.GetByIdAsync(pc.CoursesId);
                if (course != null)
                {
                    program.DurationHours += course.DurationHours ?? 0;
                }
            }

            await _unitOfWork.ProgramRepository.UpdateAsync(program);
            await _unitOfWork.SaveChangesAsync();

            var result = _mapper.Map<ProgramDto>(program);
            result.Courses = result.Courses.OrderBy(c => c.CourseOrder).ToList();
            return result;
        }
        
        public async Task<ProgramDto?> AddPrerequisitesToProgram(int programId, List<EntryRequirementDto> prerequisitesToAdd)
        {
            var program = await _unitOfWork.ProgramRepository.GetAllAsQueryable()
                .Include(p => p.ProgramEntryRequirements)
                .FirstOrDefaultAsync(p => p.Id == programId);

            if (program == null )
                return null;

            if (prerequisitesToAdd == null || !prerequisitesToAdd.Any())
                throw new BadRequestException("At least one prerequisite must be provided.");

            foreach (var prereqDto in prerequisitesToAdd)
            {
                program.ProgramEntryRequirements.Add(new ProgramEntryRequirement
                {
                    ProgramId = program.Id,
                    Name = prereqDto.Name,
                    Description = prereqDto.Description,
                    DocumentUrl = prereqDto.DocumentUrl
                });
            }

            await _unitOfWork.ProgramRepository.UpdateAsync(program);
            await _unitOfWork.SaveChangesAsync();

            var result = _mapper.Map<ProgramDto>(program);
            result.Courses = result.Courses.OrderBy(c => c.CourseOrder).ToList();
            // Assuming ProgramDto has a collection of prerequisites too
            result.EntryRequirements = result.EntryRequirements.OrderBy(p => p.Name).ToList();

            return result;
        }




        public async Task<ProgramDto?> UpdateProgram(int id, UpdateProgramInfoDto dto)
        {
            var program = await _unitOfWork.ProgramRepository.GetByIdAsync(id);
            if (program == null ) return null;

            program.Name = dto.Name;
            program.Description = dto.Description;
            program.ImageUrl = dto.ImageUrl;
            program.IsActive = dto.IsActive ?? program.IsActive;

            await _unitOfWork.SaveChangesAsync();
            return _mapper.Map<ProgramDto>(program);
        }

        public async Task<ProgramDto?> UpdateProgramCourses(int id, ICollection<ProgramCourseOrderDto> courses)
        {
            var program = await _unitOfWork.ProgramRepository.GetAllAsQueryable()
                .Include(p => p.ProgramCourses)
                    .ThenInclude(pc => pc.Classes)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (program == null ) return null;

            var oldCourseIds = program.ProgramCourses.Select(pc => pc.CoursesId).ToList();
            var newCourseIds = courses.Select(c => c.CourseId).ToList();
            var removedCourseIds = oldCourseIds.Except(newCourseIds).ToList();

            // Block if course has classes
            var blocked = program.ProgramCourses
                .FirstOrDefault(pc => removedCourseIds.Contains(pc.CoursesId) && pc.Classes.Any());
            if (blocked != null)
                throw new BadRequestException($"Cannot remove course '{blocked.Name}' because it still has associated classes.");

            // Remove old ProgramCourses (explicitly via repository)
            foreach (var toRemove in program.ProgramCourses.Where(pc => removedCourseIds.Contains(pc.CoursesId)).ToList())
            {
                await _unitOfWork.ProgramCourseRepository.DeleteAsync(toRemove);
            }

            // Fetch available courses from DB
            var coursesInProgram = await _unitOfWork.CourseRepository
                .GetAllAsQueryable()
                .Where(c => newCourseIds.Contains(c.Id) && c.IsDeleted != true)
                .ToListAsync();

            program.TotalCourses = coursesInProgram.Count;
            program.DurationHours = coursesInProgram.Sum(c => c.DurationHours ?? 0);

            // Update existing or add new ProgramCourses
            foreach (var courseOrderDto in courses)
            {
                var existing = program.ProgramCourses.FirstOrDefault(pc => pc.CoursesId == courseOrderDto.CourseId);
                if (existing != null)
                {
                    existing.CourseOrder = courseOrderDto.Order;
                    // no need to re-add, EF is already tracking it
                    await _unitOfWork.ProgramCourseRepository.UpdateAsync(existing);
                }
                else
                {
                    var course = coursesInProgram.FirstOrDefault(c => c.Id == courseOrderDto.CourseId);
                    if (course != null)
                    {
                        var newPc = new ProgramCourse
                        {
                            ProgramId = program.Id,
                            CoursesId = course.Id,
                            CourseOrder = courseOrderDto.Order,
                            Name = course.Name,
                            Description = course.Description
                        };

                        await _unitOfWork.ProgramCourseRepository.CreateAsync(newPc);
                    }
                }
            }

            await _unitOfWork.SaveChangesAsync();

            var result = _mapper.Map<ProgramDto>(program);
            result.Courses = result.Courses.OrderBy(c => c.CourseOrder).ToList();
            return result;
        }

        public async Task<EntryRequirementDto?> UpdateProgramEntryRequirement(int id, UpdateEntryRequirementDto entryRequirement)
        {
            var existingReq = await _unitOfWork.ProgramEntryRequirementRepository.GetByIdAsync(id);
            if (existingReq == null ) return null;
            if(entryRequirement.Name != null)
                existingReq.Name = entryRequirement.Name;
            if(entryRequirement.Description != null)
                existingReq.Description = entryRequirement.Description;
            if(entryRequirement.DocumentUrl != null)
                existingReq.DocumentUrl = entryRequirement.DocumentUrl;
            await _unitOfWork.ProgramEntryRequirementRepository.UpdateAsync(existingReq);
            await _unitOfWork.SaveChangesAsync();
            return _mapper.Map<EntryRequirementDto>(existingReq);
        }

        public async Task<ProgramDto?> UpdateProgramEntryRequirements(int id, ICollection<UpdateEntryRequirementDto> entryRequirements)
        {
            var program = await _unitOfWork.ProgramRepository.GetAllAsQueryable()
                .Include(p => p.ProgramEntryRequirements)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (program == null ) return null;

            // Explicitly delete all existing entry requirements
            foreach (var oldReq in program.ProgramEntryRequirements.ToList())
            {
                await _unitOfWork.ProgramEntryRequirementRepository.DeleteAsync(oldReq);
            }

            // Add new ones if provided
            if (entryRequirements != null && entryRequirements.Any())
            {
                foreach (var req in entryRequirements)
                {
                    var entity = new ProgramEntryRequirement
                    {
                        ProgramId = program.Id,
                        Name = req.Name,
                        Description = req.Description,
                        DocumentUrl = req.DocumentUrl
                    };

                    await _unitOfWork.ProgramEntryRequirementRepository.CreateAsync(entity);
                }
            }

            await _unitOfWork.SaveChangesAsync();

            // Reload to include updated requirements
            var result = await _unitOfWork.ProgramRepository.GetAllAsQueryable()
                .Include(p => p.ProgramEntryRequirements)
                .FirstOrDefaultAsync(p => p.Id == id);

            return _mapper.Map<ProgramDto>(result);
        }









        public async Task<bool> DeleteProgram(int id)
        {
            var program = await _unitOfWork.ProgramRepository.GetByIdAsync(id);

            if (program == null )
                return false;

            program.IsDeleted = true;
            program.IsActive = false;
            await _unitOfWork.ProgramRepository.UpdateAsync(program);
            await _unitOfWork.SaveChangesAsync();

            return true;
        }
    }
}
