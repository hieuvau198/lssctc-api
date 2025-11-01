using Lssctc.ProgramManagement.Courses.Dtos;
using Lssctc.Share.Common;
using Lssctc.Share.Entities;
using Lssctc.Share.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Lssctc.ProgramManagement.Courses.Services
{
    public class CoursesService : ICoursesService
    {
        private readonly IUnitOfWork _uow;
        public CoursesService(IUnitOfWork uow)
        {
            _uow = uow;
        }

        #region Courses
        public async Task<IEnumerable<CourseDto>> GetAllCoursesAsync()
        {
            var courses = await _uow.CourseRepository
                .GetAllAsQueryable()
                .Where(c => c.IsDeleted != true)
                .ToListAsync();

            return courses.Select(MapToDto);
        }

        public async Task<PagedResult<CourseDto>> GetCoursesAsync(int pageNumber, int pageSize)
        {
            if (pageNumber < 1) pageNumber = 1;
            if (pageSize < 1) pageSize = 10;

            var query = _uow.CourseRepository
                .GetAllAsQueryable()
                .Where(c => c.IsDeleted != true)
                .Select(c => MapToDto(c));

            var pagedResult = await query.ToPagedResultAsync(pageNumber, pageSize);
            return pagedResult;
        }

        public async Task<CourseDto?> GetCourseByIdAsync(int id)
        {
            var course = await _uow.CourseRepository
                .GetAllAsQueryable()
                .Where(c => c.Id == id && c.IsDeleted != true)
                .FirstOrDefaultAsync();

            return course == null ? null : MapToDto(course);
        }

        public async Task<CourseDto> CreateCourseAsync(CreateCourseDto createDto)
        {
            var course = new Course
            {
                Name = createDto.Name,
                Description = createDto.Description,
                CategoryId = createDto.CategoryId,
                LevelId = createDto.LevelId,
                Price = createDto.Price,
                DurationHours = createDto.DurationHours,
                ImageUrl = createDto.ImageUrl,
                IsActive = true,
                IsDeleted = false
            };

            await _uow.CourseRepository.CreateAsync(course);
            await _uow.SaveChangesAsync();

            return MapToDto(course);
        }

        public async Task<CourseDto> UpdateCourseAsync(int id, UpdateCourseDto updateDto)
        {
            var course = await _uow.CourseRepository.GetByIdAsync(id);
            if (course == null || course.IsDeleted == true)
            {
                throw new KeyNotFoundException($"Course with ID {id} not found.");
            }

            course.Name = updateDto.Name ?? course.Name;
            course.Description = updateDto.Description ?? course.Description;
            course.CategoryId = updateDto.CategoryId ?? course.CategoryId;
            course.LevelId = updateDto.LevelId ?? course.LevelId;
            course.Price = updateDto.Price ?? course.Price;
            course.DurationHours = updateDto.DurationHours ?? course.DurationHours;
            course.ImageUrl = updateDto.ImageUrl ?? course.ImageUrl;
            course.IsActive = updateDto.IsActive ?? course.IsActive;

            await _uow.CourseRepository.UpdateAsync(course);
            await _uow.SaveChangesAsync();

            return MapToDto(course);
        }

        public async Task DeleteCourseAsync(int id)
        {
            var course = await _uow.CourseRepository
                .GetAllAsQueryable()
                .Where(c => c.Id == id)
                .Include(c => c.ProgramCourses)
                .FirstOrDefaultAsync()
                ;
            if (course == null || course.IsDeleted == true)
            {
                throw new KeyNotFoundException($"Course with ID {id} not found.");
            }
            // check if course is associated with any program courses
            if (course.ProgramCourses != null && course.ProgramCourses.Any())
            {
                throw new InvalidOperationException("Cannot delete course associated with programs.");
            }

            course.IsDeleted = true;
            await _uow.CourseRepository.UpdateAsync(course);
            await _uow.SaveChangesAsync();
        }
        #endregion

        #region Program Courses

        public async Task<IEnumerable<CourseDto>> GetCoursesByProgramIdAsync(int programId)
        {
            var programCourses = await _uow.ProgramCourseRepository
                .GetAllAsQueryable()
                .Where(pc => pc.ProgramId == programId)
                .Include(pc => pc.Course)
                .ToListAsync();
            var courses = programCourses
                .Where(pc => pc.Course != null && pc.Course.IsDeleted != true)
                .Select(pc => MapToDto(pc.Course!));
            return courses;
        }

        #endregion

        #region Mapping
        private static CourseDto MapToDto(Course c)
        {
            return new CourseDto
            {
                Id = c.Id,
                Name = c.Name,
                Description = c.Description,
                CategoryId = c.CategoryId,
                LevelId = c.LevelId,
                Price = c.Price,
                DurationHours = c.DurationHours,
                ImageUrl = c.ImageUrl,
                IsActive = c.IsActive ?? false
            };
        }
        #endregion
    }
}
