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

        public async Task<PagedResult<ProgramDto?>> GetAllProgramsAsync(ProgramQueryParameters parameters)
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

        public async Task<ProgramDto?> GetProgramByIdAsync(int id)
        {
            var program = await _unitOfWork.ProgramRepository.GetAllAsQueryable()
                .Include(p => p.ProgramEntryRequirements)
                .Include(p => p.ProgramCourses)
                .ThenInclude(pc => pc.Courses)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (program == null || program.IsDeleted == true)
                return null;

            var dto = _mapper.Map<ProgramDto>(program);
            dto.Courses = dto.Courses.OrderBy(c => c.CourseOrder).ToList();

            return dto;
        }

        public async Task<ProgramDto> CreateProgramAsync(CreateProgramDto dto)
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
        public async Task<ProgramDto?> AddCoursesToProgramAsync(int programId, List<CourseOrderDto> coursesToAdd)
        {
            var program = await _unitOfWork.ProgramRepository.GetAllAsQueryable()
                .Include(p => p.ProgramCourses)
                .ThenInclude(pc => pc.Courses)
                .FirstOrDefaultAsync(p => p.Id == programId);

            if (program == null || program.IsDeleted == true)
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
            foreach(var pc in program.ProgramCourses)
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
        public async Task<ProgramDto?> AddPrerequisitesToProgramAsync(int programId, List<EntryRequirementDto> prerequisitesToAdd)
        {
            var program = await _unitOfWork.ProgramRepository.GetAllAsQueryable()
                .Include(p => p.ProgramEntryRequirements)
                .FirstOrDefaultAsync(p => p.Id == programId);

            if (program == null || program.IsDeleted == true)
                return null;

            if (prerequisitesToAdd == null || !prerequisitesToAdd.Any())
                throw new BadRequestException("At least one prerequisite must be provided.");

            foreach (var prereqDto in prerequisitesToAdd)
            {
                program.ProgramEntryRequirements.Add(new ProgramEntryRequirement
                {
                    ProgramId = program.Id,
                    Name = prereqDto.Name,
                    Description = prereqDto.Description
                });
            }

            await _unitOfWork.ProgramRepository.UpdateAsync(program);
            await _unitOfWork.SaveChangesAsync();

            var result = _mapper.Map<ProgramDto>(program);
            result.Courses = result.Courses.OrderBy(c => c.CourseOrder).ToList();
            // Assuming ProgramDto has a collection of prerequisites too
            result.Prerequisites = result.Prerequisites.OrderBy(p => p.Name).ToList();

            return result;
        }




        public async Task<ProgramDto?> UpdateProgramAsync(int id, UpdateProgramDto dto)
        {
            var program = await _unitOfWork.ProgramRepository.GetAllAsQueryable()
                .Include(p => p.ProgramCourses)
                    .ThenInclude(pc => pc.Classes)
                .Include(p => p.ProgramEntryRequirements)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (program == null || program.IsDeleted == true)
                return null;

            // --- Update scalar fields manually (avoid overwriting navigation collections) ---
            program.Name = dto.Name;
            program.Description = dto.Description;
            program.ImageUrl = dto.ImageUrl;
            program.IsActive = dto.IsActive ?? program.IsActive;

            // --- Current ProgramCourses ---
            var oldCourseIds = program.ProgramCourses.Select(pc => pc.CoursesId).ToList();
            var newCourseIds = dto.Courses.Select(c => c.CourseId).ToList();

            // --- Find removed courses ---
            var removedCourseIds = oldCourseIds.Except(newCourseIds).ToList();

            // --- Check if any removed course has classes ---
            var blocked = program.ProgramCourses
                .FirstOrDefault(pc => removedCourseIds.Contains(pc.CoursesId) && pc.Classes.Any());

            if (blocked != null)
            {
                throw new BadRequestException(
                    $"Cannot remove course '{blocked.Name}' because it still has associated classes."
                );
            }

            // --- Remove ProgramCourses only if they are not in new list ---
            program.ProgramCourses = program.ProgramCourses
                .Where(pc => !removedCourseIds.Contains(pc.CoursesId))
                .ToList();

            // --- Clear prerequisites ---
            program.ProgramEntryRequirements.Clear();

            await _unitOfWork.SaveChangesAsync(); // flush deletes

            // --- Add/update new ProgramCourses ---
            if (dto.Courses != null && dto.Courses.Any())
            {
                var coursesInProgram = await _unitOfWork.CourseRepository
                    .GetAllAsQueryable()
                    .Where(c => newCourseIds.Contains(c.Id) && c.IsDeleted != true)
                    .ToListAsync();

                program.TotalCourses = coursesInProgram.Count;
                program.DurationHours = coursesInProgram.Sum(c => c.DurationHours ?? 0);

                foreach (var courseOrderDto in dto.Courses)
                {
                    var existing = program.ProgramCourses.FirstOrDefault(pc => pc.CoursesId == courseOrderDto.CourseId);
                    if (existing != null)
                    {
                        existing.CourseOrder = courseOrderDto.Order; // update order
                    }
                    else
                    {
                        var course = coursesInProgram.FirstOrDefault(c => c.Id == courseOrderDto.CourseId);
                        if (course != null)
                        {
                            program.ProgramCourses.Add(new ProgramCourse
                            {
                                ProgramId = program.Id,
                                CoursesId = course.Id,
                                CourseOrder = courseOrderDto.Order,
                                Name = course.Name,
                                Description = course.Description
                            });
                        }
                    }
                }
            }
            else
            {
                program.TotalCourses = 0;
                program.DurationHours = 0;
            }

            // --- Add prerequisites ---
            if (dto.Prerequisites != null && dto.Prerequisites.Any())
            {
                foreach (var prereqDto in dto.Prerequisites)
                {
                    program.ProgramEntryRequirements.Add(new ProgramEntryRequirement
                    {
                        ProgramId = program.Id,
                        Name = prereqDto.Name,
                        Description = prereqDto.Description,
                        DocumentUrl = prereqDto.DocumentUrl
                    });
                }
            }

            await _unitOfWork.SaveChangesAsync();

            var result = _mapper.Map<ProgramDto>(program);
            result.Courses = result.Courses.OrderBy(c => c.CourseOrder).ToList();
            return result;
        }







        public async Task<bool> DeleteProgramAsync(int id)
        {
            var program = await _unitOfWork.ProgramRepository.GetByIdAsync(id);

            if (program == null || program.IsDeleted == true)
                return false;

            program.IsDeleted = true;
            await _unitOfWork.ProgramRepository.UpdateAsync(program);
            await _unitOfWork.SaveChangesAsync();

            return true;
        }
    }
}
