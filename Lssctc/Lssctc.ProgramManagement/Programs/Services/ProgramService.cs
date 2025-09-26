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
                .Include( p => p.ProgramEntryRequirements)
                .Include(p => p.ProgramCourses)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (program == null || program.IsDeleted == true)
                return null;

            _mapper.Map(dto, program);

            // Clear the old courses in the program
            var existingProgramCourses = program.ProgramCourses.ToList();
            if (existingProgramCourses.Any())
            {
                foreach (var pc in existingProgramCourses)
                {
                    await _unitOfWork.ProgramCourseRepository.DeleteAsync(pc);
                }

                await _unitOfWork.SaveChangesAsync();

                program.ProgramCourses.Clear();
            }

            if (dto.Courses != null && dto.Courses.Any())
            {
                var courseIds = dto.Courses.Select(c => c.CourseId).ToList();

                var coursesInProgram = await _unitOfWork.CourseRepository
                    .GetAllAsQueryable()
                    .Where(c => courseIds.Contains(c.Id) && c.IsDeleted != true)
                    .ToListAsync();

                program.TotalCourses = coursesInProgram.Count;
                program.DurationHours = 0;
                foreach (var c in coursesInProgram)
                {
                    program.DurationHours += c.DurationHours ?? 0;
                }

                foreach (var courseOrderDto in dto.Courses)
                {
                    var course = coursesInProgram.FirstOrDefault(c => c.Id == courseOrderDto.CourseId);
                    if (course != null)
                    {
                        program.ProgramCourses.Add(new ProgramCourse
                        {
                            CoursesId = course.Id,
                            ProgramId = program.Id,
                            CourseOrder = courseOrderDto.Order,
                            Name = dto.Name,
                            Description = dto.Description
                        });
                    }
                }
            }
            else
            {
                program.TotalCourses = 0;
                program.DurationHours = 0;
            }
            //update prerequisites
            if (dto.Prerequisites != null)
            {
                var dtoPrereqIds = dto.Prerequisites.Where(p => p.Id > 0).Select(p => p.Id).ToList();

                // Remove prerequisites not in DTO
                var toRemove = program.ProgramEntryRequirements
                    .Where(p => !dtoPrereqIds.Contains(p.Id))
                    .ToList();
                if(toRemove != null && toRemove.Count > 0)
                {
                    foreach (var rem in toRemove)
                    {
                        await _unitOfWork.ProgramEntryRequirementRepository.DeleteAsync(rem);
                    }
                    await _unitOfWork.SaveChangesAsync();

                    program.ProgramEntryRequirements.Clear();
                }


                // Update existing or add new
                foreach (var prereqDto in dto.Prerequisites)
                {
                    if (prereqDto.Id > 0)
                    {
                        var existing = program.ProgramEntryRequirements.FirstOrDefault(p => p.Id == prereqDto.Id);
                        if (existing != null)
                        {
                            existing.Name = prereqDto.Name;
                            existing.Description = prereqDto.Description;
                        }
                    }
                    else
                    {
                        program.ProgramEntryRequirements.Add(new ProgramEntryRequirement
                        {
                            ProgramId = program.Id,
                            Name = prereqDto.Name,
                            Description = prereqDto.Description,
                        });
                    }
                }
            }
            await _unitOfWork.ProgramRepository.UpdateAsync(program);
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
