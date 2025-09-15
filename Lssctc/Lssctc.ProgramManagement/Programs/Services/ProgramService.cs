using AutoMapper;
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
            var query = _unitOfWork.TrainingProgramRepository.GetAllAsQueryable();

            if (!string.IsNullOrWhiteSpace(parameters.SearchTerm))
            {
                query = query.Where(p =>
                    p.Name.Contains(parameters.SearchTerm) ||
                    (p.Description != null && p.Description.Contains(parameters.SearchTerm)));
            }

            if (parameters.IsActive.HasValue)
            {
                query = query.Where(p => p.IsActive == parameters.IsActive);
            }

            if (parameters.IsDeleted.HasValue)
            {
                query = query.Where(p => p.IsDeleted == parameters.IsDeleted);
            }

            if (parameters.MinDurationHours.HasValue)
            {
                query = query.Where(p => p.DurationHours >= parameters.MinDurationHours);
            }

            if (parameters.MaxDurationHours.HasValue)
            {
                query = query.Where(p => p.DurationHours <= parameters.MaxDurationHours);
            }

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
            var program = await _unitOfWork.TrainingProgramRepository.GetAllAsQueryable()
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

            if (dto.Courses != null && dto.Courses.Any())
            {
                var courseIds = dto.Courses.Select(c => c.CourseId).ToList();

                var courses = await _unitOfWork.CourseRepository
                    .GetAllAsQueryable()
                    .Where(c => courseIds.Contains(c.Id) && c.IsDeleted != true)
                    .ToListAsync();

                entity.TotalCourses = courses.Count;
                entity.DurationHours = courses.Sum(c => c.DurationHours ?? 0);

                foreach (var courseOrderDto in dto.Courses)
                {
                    var course = courses.FirstOrDefault(c => c.Id == courseOrderDto.CourseId);
                    if (course != null)
                    {
                        entity.ProgramCourses.Add(new ProgramCourse
                        {
                            CoursesId = course.Id,
                            Program = entity,
                            CourseOrder = courseOrderDto.Order,
                            Name = dto.Name,
                            Description = dto.Description
                        });
                    }
                }
            }
            else
            {
                entity.TotalCourses = 0;
                entity.DurationHours = 0;
            }

            await _unitOfWork.TrainingProgramRepository.CreateAsync(entity);
            await _unitOfWork.SaveChangesAsync();

            return _mapper.Map<ProgramDto>(entity);
        }

        public async Task<ProgramDto?> UpdateProgramAsync(int id, UpdateProgramDto dto)
        {
            var program = await _unitOfWork.TrainingProgramRepository.GetAllAsQueryable()
                .Include(p => p.ProgramCourses)
                .ThenInclude(pc => pc.Courses)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (program == null || program.IsDeleted == true)
                return null;

            _mapper.Map(dto, program);

            // Clear the old courses in the program
            program.ProgramCourses.Clear();

            if (dto.Courses != null && dto.Courses.Any())
            {
                var courseIds = dto.Courses.Select(c => c.CourseId).ToList();

                var coursesInProgram = await _unitOfWork.CourseRepository
                    .GetAllAsQueryable()
                    .Where(c => courseIds.Contains(c.Id) && c.IsDeleted != true)
                    .ToListAsync();

                program.TotalCourses = coursesInProgram.Count;
                program.DurationHours = coursesInProgram.Sum(c => c.DurationHours ?? 0);

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

            await _unitOfWork.TrainingProgramRepository.UpdateAsync(program);
            await _unitOfWork.SaveChangesAsync();

            var result = _mapper.Map<ProgramDto>(program);
            result.Courses = result.Courses.OrderBy(c => c.CourseOrder).ToList();
            return result;
        }

        public async Task<bool> DeleteProgramAsync(int id)
        {
            var program = await _unitOfWork.TrainingProgramRepository.GetByIdAsync(id);

            if (program == null || program.IsDeleted == true)
                return false;

            program.IsDeleted = true;
            await _unitOfWork.TrainingProgramRepository.UpdateAsync(program);
            await _unitOfWork.SaveChangesAsync();

            return true;
        }
    }
}
