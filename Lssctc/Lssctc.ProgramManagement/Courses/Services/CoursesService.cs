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
                // ADDED: Include navigation properties
                .Include(c => c.Category)
                .Include(c => c.Level)
                .ToListAsync();

            return courses.Select(MapToDto);
        }

        public async Task<PagedResult<CourseDto>> GetCoursesAsync(int pageNumber, int pageSize)
        {
            if (pageNumber < 1) pageNumber = 1;
            if (pageSize < 1) pageSize = 10;

            // CHANGED: We now project directly to CourseDto in the query.
            // This is more efficient for paging as it doesn't use MapToDto
            // and lets the database do the work.
            var query = _uow.CourseRepository
                .GetAllAsQueryable()
                .Where(c => c.IsDeleted != true)
                .Select(c => new CourseDto
                {
                    Id = c.Id,
                    Name = c.Name,
                    Description = c.Description,
                    Category = c.Category != null ? c.Category.Name : null, // Map name
                    Level = c.Level != null ? c.Level.Name : null,         // Map name
                    Price = c.Price,
                    DurationHours = c.DurationHours,
                    ImageUrl = c.ImageUrl,
                    IsActive = c.IsActive ?? false
                });

            var pagedResult = await query.ToPagedResultAsync(pageNumber, pageSize);
            return pagedResult;
        }

        public async Task<CourseDto?> GetCourseByIdAsync(int id)
        {
            var course = await _uow.CourseRepository
                .GetAllAsQueryable()
                .Where(c => c.Id == id && c.IsDeleted != true)
                // ADDED: Include navigation properties
                .Include(c => c.Category)
                .Include(c => c.Level)
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

            // ADDED: Reload the new course with its navigation properties
            // to ensure Category.Name and Level.Name are available for MapToDto
            var newCourse = await _uow.CourseRepository.GetAllAsQueryable()
                .Include(c => c.Category)
                .Include(c => c.Level)
                .FirstOrDefaultAsync(c => c.Id == course.Id);

            return MapToDto(newCourse!); // Map the reloaded entity
        }

        public async Task<CourseDto> UpdateCourseAsync(int id, UpdateCourseDto updateDto)
        {
            // Fetch the course to update (no includes needed yet)
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

            // ADDED: Reload the entity *with* navigation properties to return the correct DTO
            var updatedCourse = await _uow.CourseRepository.GetAllAsQueryable()
                .Include(c => c.Category)
                .Include(c => c.Level)
                .FirstOrDefaultAsync(c => c.Id == id);

            return MapToDto(updatedCourse!); // Map the reloaded entity
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
                // ADDED: ThenInclude to get the Course's Category and Level
                .Include(pc => pc.Course)
                    .ThenInclude(c => c!.Category)
                .Include(pc => pc.Course)
                    .ThenInclude(c => c!.Level)
                .ToListAsync();

            var courses = programCourses
                .Where(pc => pc.Course != null && pc.Course.IsDeleted != true)
                .Select(pc => MapToDto(pc.Course!));
            return courses;
        }

        #endregion

        #region Course Categories and Levels

        // ... (No changes needed in this region) ...

        public async Task<IEnumerable<CourseCategoryDto>> GetAllCourseCategoriesAsync()
        {
            var categories = await _uow.CourseCategoryRepository
                .GetAllAsQueryable()
                .ToListAsync();
            return categories.Select(MapToDto);
        }

        public async Task<CourseCategoryDto> CreateCourseCategoryAsync(CreateCourseCategoryDto dto)
        {
            var existing = await _uow.CourseCategoryRepository
                .GetAllAsQueryable()
                .FirstOrDefaultAsync(c => c.Name.ToLower() == dto.Name.ToLower());

            if (existing != null)
                throw new InvalidOperationException($"Category name '{dto.Name}' already exists.");

            var category = new CourseCategory
            {
                Name = dto.Name,
                Description = dto.Description
            };

            await _uow.CourseCategoryRepository.CreateAsync(category);
            await _uow.SaveChangesAsync();
            return MapToDto(category);
        }

        public async Task<CourseCategoryDto> UpdateCourseCategoryAsync(int id, UpdateCourseCategoryDto dto)
        {
            var category = await _uow.CourseCategoryRepository.GetByIdAsync(id);
            if (category == null)
                throw new KeyNotFoundException($"Category with ID {id} not found.");

            if (dto.Name != null && dto.Name.ToLower() != category.Name.ToLower())
            {
                var existing = await _uow.CourseCategoryRepository
                    .GetAllAsQueryable()
                    .FirstOrDefaultAsync(c => c.Name.ToLower() == dto.Name.ToLower() && c.Id != id);

                if (existing != null)
                    throw new InvalidOperationException($"Category name '{dto.Name}' already exists.");

                category.Name = dto.Name;
            }

            category.Description = dto.Description ?? category.Description;

            await _uow.CourseCategoryRepository.UpdateAsync(category);
            await _uow.SaveChangesAsync();
            return MapToDto(category);
        }

        public async Task<IEnumerable<CourseLevelDto>> GetAllCourseLevelsAsync()
        {
            var levels = await _uow.CourseLevelRepository
                .GetAllAsQueryable()
                .ToListAsync();
            return levels.Select(MapToDto);
        }

        public async Task<CourseLevelDto> CreateCourseLevelAsync(CreateCourseLevelDto dto)
        {
            var existing = await _uow.CourseLevelRepository
                .GetAllAsQueryable()
                .FirstOrDefaultAsync(l => l.Name.ToLower() == dto.Name.ToLower());

            if (existing != null)
                throw new InvalidOperationException($"Level name '{dto.Name}' already exists.");

            var level = new CourseLevel
            {
                Name = dto.Name,
                Description = dto.Description
            };

            await _uow.CourseLevelRepository.CreateAsync(level);
            await _uow.SaveChangesAsync();
            return MapToDto(level);
        }

        public async Task<CourseLevelDto> UpdateCourseLevelAsync(int id, UpdateCourseLevelDto dto)
        {
            var level = await _uow.CourseLevelRepository.GetByIdAsync(id);
            if (level == null)
                throw new KeyNotFoundException($"Level with ID {id} not found.");

            if (dto.Name != null && dto.Name.ToLower() != level.Name.ToLower())
            {
                var existing = await _uow.CourseLevelRepository
                    .GetAllAsQueryable()
                    .FirstOrDefaultAsync(l => l.Name.ToLower() == dto.Name.ToLower() && l.Id != id);

                if (existing != null)
                    throw new InvalidOperationException($"Level name '{dto.Name}' already exists.");

                level.Name = dto.Name;
            }

            level.Description = dto.Description ?? level.Description;

            await _uow.CourseLevelRepository.UpdateAsync(level);
            await _uow.SaveChangesAsync();
            return MapToDto(level);
        }

        #endregion

        #region Mapping

        // --- CHANGED ---
        private static CourseDto MapToDto(Course c)
        {
            return new CourseDto
            {
                Id = c.Id,
                Name = c.Name,
                Description = c.Description,
                // Use the navigation property's Name, with a null check
                Category = c.Category?.Name,
                Level = c.Level?.Name,
                Price = c.Price,
                DurationHours = c.DurationHours,
                ImageUrl = c.ImageUrl,
                IsActive = c.IsActive ?? false
            };
        }
        // --- END CHANGE ---

        private static CourseCategoryDto MapToDto(CourseCategory c)
        {
            return new CourseCategoryDto
            {
                Id = c.Id,
                Name = c.Name,
                Description = c.Description
            };
        }

        private static CourseLevelDto MapToDto(CourseLevel l)
        {
            return new CourseLevelDto
            {
                Id = l.Id,
                Name = l.Name,
                Description = l.Description
            };
        }
        #endregion
    }
}