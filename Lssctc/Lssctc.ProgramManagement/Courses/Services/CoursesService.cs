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

        public async Task<PagedResult<CourseDto>> GetCoursesAsync(int pageNumber, int pageSize, string? searchTerm = null, string? sortBy = null, string? sortDirection = null)
        {
            if (pageNumber < 1) pageNumber = 1;
            if (pageSize < 1) pageSize = 10;

            // Start with base query including navigation properties
            var query = _uow.CourseRepository
                .GetAllAsQueryable()
                .Where(c => c.IsDeleted != true)
                .Include(c => c.Category)
                .Include(c => c.Level)
                .AsQueryable();

            // Apply search filter if searchTerm is provided
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                var normalizedSearchTerm = searchTerm.Trim();
                query = query.Where(c =>
                    c.Name.Contains(normalizedSearchTerm) ||
                    (c.Description != null && c.Description.Contains(normalizedSearchTerm)) ||
                    (c.Category != null && c.Category.Name.Contains(normalizedSearchTerm)) ||
                    (c.Level != null && c.Level.Name.Contains(normalizedSearchTerm))
                );
            }

            // Project to DTO
            var projectedQuery = query.Select(c => new CourseDto
            {
                Id = c.Id,
                Name = c.Name,
                Description = c.Description,
                Category = c.Category != null ? c.Category.Name : null,
                Level = c.Level != null ? c.Level.Name : null,
                Price = c.Price,
                DurationHours = c.DurationHours,
                ImageUrl = c.ImageUrl,
                IsActive = c.IsActive ?? false,
                CreatedAt = c.CreatedAt,
                UpdatedAt = c.UpdatedAt,
                BackgroundImageUrl = c.BackgroundImageUrl
            });

            // Apply sorting logic
            if (!string.IsNullOrWhiteSpace(sortBy))
            {
                // Scenario A: User selected specific sort
                var isDescending = sortDirection?.ToLower() == "desc";

                projectedQuery = sortBy.ToLower() switch
                {
                    "price" => isDescending
                        ? projectedQuery.OrderByDescending(c => c.Price).ThenBy(c => c.Name)
                        : projectedQuery.OrderBy(c => c.Price).ThenBy(c => c.Name),
                    "duration" => isDescending
                        ? projectedQuery.OrderByDescending(c => c.DurationHours).ThenBy(c => c.Name)
                        : projectedQuery.OrderBy(c => c.DurationHours).ThenBy(c => c.Name),
                    _ => projectedQuery.OrderBy(c => c.Name) // Default if invalid sortBy
                };
            }
            else if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                // Scenario B: Search relevance sorting (no specific sort selected)
                var normalizedSearchTerm = searchTerm.Trim();

                projectedQuery = projectedQuery
                    .OrderByDescending(c => c.Name!.Contains(normalizedSearchTerm))
                    .ThenByDescending(c =>
                        (c.Category != null && c.Category.Contains(normalizedSearchTerm)) ||
                        (c.Level != null && c.Level.Contains(normalizedSearchTerm))
                    )
                    .ThenByDescending(c => c.Description != null && c.Description.Contains(normalizedSearchTerm))
                    .ThenBy(c => c.Name);
            }
            else
            {
                // Scenario C: No search term and no sort - default ordering
                projectedQuery = projectedQuery.OrderByDescending(c => c.CreatedAt);
            }

            var pagedResult = await projectedQuery.ToPagedResultAsync(pageNumber, pageSize);
            return pagedResult;
        }

        public async Task<IEnumerable<CourseDto>> GetAvailableCoursesAsync()
        {
            var courses = await _uow.CourseRepository
                .GetAllAsQueryable()
                .Where(c => c.IsDeleted != true && c.IsActive == true)
                .Include(c => c.Category)
                .Include(c => c.Level)
                .ToListAsync();

            return courses.Select(MapToDto);
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
                BackgroundImageUrl = createDto.BackgroundImageUrl ?? "https://templates.framework-y.com/lightwire/images/wide-1.jpg",
                IsActive = true,
                IsDeleted = false
            };

            await _uow.CourseRepository.CreateAsync(course);
            await _uow.SaveChangesAsync();

            var newCourse = await _uow.CourseRepository.GetAllAsQueryable()
                .Include(c => c.Category)
                .Include(c => c.Level)
                .FirstOrDefaultAsync(c => c.Id == course.Id);

            return MapToDto(newCourse!);
        }

        public async Task<CourseDto> UpdateCourseAsync(int id, UpdateCourseDto updateDto)
        {
            // Fetch the course to update (no includes needed yet)
            var course = await _uow.CourseRepository.GetByIdAsync(id);
            if (course == null || course.IsDeleted == true)
            {
                throw new KeyNotFoundException($"Course with ID {id} not found.");
            }

            // 1. Validation: Unique Name Check
            if (updateDto.Name != null)
            {
                // Normalize name
                var normalizedName = string.Join(' ', updateDto.Name.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries));

                // Only check if the name is changing
                if (!string.Equals(course.Name, normalizedName, StringComparison.CurrentCultureIgnoreCase))
                {
                    var isDuplicate = await _uow.CourseRepository.GetAllAsQueryable()
                        .AnyAsync(c => c.Name == normalizedName && c.Id != id && c.IsDeleted != true);

                    if (isDuplicate)
                    {
                        throw new InvalidOperationException($"Course name '{normalizedName}' already exists.");
                    }
                    course.Name = normalizedName;
                }
            }

            // 2. Update fields (Allow updates regardless of usage in Programs/Classes)
            course.Description = updateDto.Description ?? course.Description;
            course.CategoryId = updateDto.CategoryId ?? course.CategoryId;
            course.LevelId = updateDto.LevelId ?? course.LevelId;
            course.Price = updateDto.Price ?? course.Price;
            course.DurationHours = updateDto.DurationHours ?? course.DurationHours;
            course.ImageUrl = updateDto.ImageUrl ?? course.ImageUrl;
            course.IsActive = updateDto.IsActive ?? course.IsActive;
            course.BackgroundImageUrl = updateDto.BackgroundImageUrl ?? course.BackgroundImageUrl;

            await _uow.CourseRepository.UpdateAsync(course);
            await _uow.SaveChangesAsync();

            // Reload the entity *with* navigation properties to return the correct DTO
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

        public async Task<IEnumerable<CourseDto>> GetAvailableCoursesByProgramIdAsync(int programId)
        {
            var programCourses = await _uow.ProgramCourseRepository
                .GetAllAsQueryable()
                .Where(pc => pc.ProgramId == programId)
                .Include(pc => pc.Course)
                    .ThenInclude(c => c!.Category)
                .Include(pc => pc.Course)
                    .ThenInclude(c => c!.Level)
                .ToListAsync();

            var courses = programCourses
                .Where(pc => pc.Course != null
                             && pc.Course.IsDeleted != true
                             && pc.Course.IsActive == true)
                .Select(pc => MapToDto(pc.Course!));

            return courses;
        }

        #endregion

        #region Class Courses
        public async Task<CourseDto?> GetCourseByClassIdAsync(int classId)
        {
            var targetClass = await _uow.ClassRepository
                .GetAllAsQueryable()
                .Where(c => c.Id == classId)
                .Include(c => c.ProgramCourse)
                    .ThenInclude(pc => pc.Course)
                        .ThenInclude(co => co.Category) // Include Category
                .Include(c => c.ProgramCourse)
                    .ThenInclude(pc => pc.Course)
                        .ThenInclude(co => co.Level) // Include Level
                .AsNoTracking() // Read-only operation
                .FirstOrDefaultAsync();

            if (targetClass == null || targetClass.ProgramCourse == null || targetClass.ProgramCourse.Course == null)
            {
                return null;
            }

            var course = targetClass.ProgramCourse.Course;

            if (course.IsDeleted == true)
            {
                return null;
            }

            return MapToDto(course); // Use existing mapper
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

        private static CourseDto MapToDto(Course c)
        {
            return new CourseDto
            {
                Id = c.Id,
                Name = c.Name,
                Description = c.Description,
                Category = c.Category?.Name,
                Level = c.Level?.Name,
                Price = c.Price,
                DurationHours = c.DurationHours,
                ImageUrl = c.ImageUrl,
                IsActive = c.IsActive ?? false,
                CreatedAt = c.CreatedAt,
                UpdatedAt = c.UpdatedAt,
                BackgroundImageUrl = c.BackgroundImageUrl
            };
        }

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