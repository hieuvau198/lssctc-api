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
                .Include(c => c.Category)
                .Include(c => c.Level)
                .Include(c => c.CourseCode) // [NEW] Include CourseCode
                .ToListAsync();

            return courses.Select(MapToDto);
        }

        public async Task<PagedResult<CourseDto>> GetCoursesAsync(int pageNumber, int pageSize, string? searchTerm = null, string? sortBy = null, string? sortDirection = null)
        {
            if (pageNumber < 1) pageNumber = 1;
            if (pageSize < 1) pageSize = 10;

            var query = _uow.CourseRepository
                .GetAllAsQueryable()
                .Where(c => c.IsDeleted != true)
                .Include(c => c.Category)
                .Include(c => c.Level)
                .Include(c => c.CourseCode) // [NEW] Include CourseCode
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                var normalizedSearchTerm = searchTerm.Trim();
                query = query.Where(c =>
                    c.Name.Contains(normalizedSearchTerm) ||
                    (c.Description != null && c.Description.Contains(normalizedSearchTerm)) ||
                    (c.Category != null && c.Category.Name.Contains(normalizedSearchTerm)) ||
                    (c.Level != null && c.Level.Name.Contains(normalizedSearchTerm)) ||
                    (c.CourseCode != null && c.CourseCode.Name.Contains(normalizedSearchTerm)) // [NEW] Search by Code
                );
            }

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
                BackgroundImageUrl = c.BackgroundImageUrl,
                CourseCode = c.CourseCode != null ? c.CourseCode.Name : null // [NEW] Map Code
            });

            if (!string.IsNullOrWhiteSpace(sortBy))
            {
                var isDescending = sortDirection?.ToLower() == "desc";

                projectedQuery = sortBy.ToLower() switch
                {
                    "price" => isDescending
                        ? projectedQuery.OrderByDescending(c => c.Price).ThenBy(c => c.Name)
                        : projectedQuery.OrderBy(c => c.Price).ThenBy(c => c.Name),
                    "duration" => isDescending
                        ? projectedQuery.OrderByDescending(c => c.DurationHours).ThenBy(c => c.Name)
                        : projectedQuery.OrderBy(c => c.DurationHours).ThenBy(c => c.Name),
                    "code" => isDescending // [NEW] Sort by Code
                        ? projectedQuery.OrderByDescending(c => c.CourseCode).ThenBy(c => c.Name)
                        : projectedQuery.OrderBy(c => c.CourseCode).ThenBy(c => c.Name),
                    _ => projectedQuery.OrderBy(c => c.Name)
                };
            }
            else if (!string.IsNullOrWhiteSpace(searchTerm))
            {
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
                projectedQuery = projectedQuery.OrderByDescending(c => c.CreatedAt);
            }

            var pagedResult = await projectedQuery.ToPagedResultAsync(pageNumber, pageSize);
            return pagedResult;
        }

        public async Task<IEnumerable<CourseDto>> GetAvailableCoursesAsync()
        {
            // Filter out invalid courses (missing certificate or sections) ===
            var courses = await _uow.CourseRepository
                .GetAllAsQueryable()
                .Where(c => c.IsDeleted != true
                         && c.IsActive == true
                         && c.CourseCertificates.Any()
                         && c.CourseSections.Any())
                .Include(c => c.Category)
                .Include(c => c.Level)
                .Include(c => c.CourseCode) // [NEW] Include CourseCode
                .ToListAsync();
            // ============================================================================

            return courses.Select(MapToDto);
        }

        public async Task<CourseDto?> GetCourseByIdAsync(int id)
        {
            var course = await _uow.CourseRepository
                .GetAllAsQueryable()
                .Where(c => c.Id == id && c.IsDeleted != true)
                .Include(c => c.Category)
                .Include(c => c.Level)
                .Include(c => c.CourseCode) // [NEW] Include CourseCode
                .FirstOrDefaultAsync();

            return course == null ? null : MapToDto(course);
        }

        public async Task<CourseDto> CreateCourseAsync(CreateCourseDto createDto)
        {
            // [NEW] Handle CourseCode creation
            string finalCourseCode;

            if (string.IsNullOrWhiteSpace(createDto.CourseCode))
            {
                // Generate unique code if null/empty
                finalCourseCode = await GenerateUniqueCourseCodeAsync();
            }
            else
            {
                // Use provided code, verify uniqueness
                finalCourseCode = createDto.CourseCode.Trim();
                var exists = await _uow.CourseCodeRepository.GetAllAsQueryable()
                    .AnyAsync(cc => cc.Name == finalCourseCode);

                if (exists)
                {
                    throw new InvalidOperationException($"Course code '{finalCourseCode}' already exists.");
                }
            }

            // Create CourseCode Entity
            var newCourseCodeEntity = new CourseCode { Name = finalCourseCode };
            await _uow.CourseCodeRepository.CreateAsync(newCourseCodeEntity);
            await _uow.SaveChangesAsync(); // Save to get ID

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
                IsDeleted = false,
                CourseCodeId = newCourseCodeEntity.Id // Link the code
            };

            await _uow.CourseRepository.CreateAsync(course);
            await _uow.SaveChangesAsync();

            var newCourse = await _uow.CourseRepository.GetAllAsQueryable()
                .Include(c => c.Category)
                .Include(c => c.Level)
                .Include(c => c.CourseCode)
                .FirstOrDefaultAsync(c => c.Id == course.Id);

            return MapToDto(newCourse!);
        }

        public async Task<CourseDto> UpdateCourseAsync(int id, UpdateCourseDto updateDto)
        {
            var course = await _uow.CourseRepository.GetAllAsQueryable()
                .Include(c => c.CourseCode)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (course == null || course.IsDeleted == true)
            {
                throw new KeyNotFoundException($"Course with ID {id} not found.");
            }

            if (updateDto.Name != null)
            {
                var normalizedName = string.Join(' ', updateDto.Name.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries));

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

            // [NEW] Update CourseCode logic
            if (!string.IsNullOrWhiteSpace(updateDto.CourseCode))
            {
                var newCode = updateDto.CourseCode.Trim();
                // Check if code is different from current
                if (course.CourseCode == null || !course.CourseCode.Name.Equals(newCode, StringComparison.OrdinalIgnoreCase))
                {
                    var exists = await _uow.CourseCodeRepository.GetAllAsQueryable()
                        .AnyAsync(cc => cc.Name == newCode);

                    if (exists)
                    {
                        throw new InvalidOperationException($"Course code '{newCode}' already exists.");
                    }

                    // Create new entity for the new code
                    var newCourseCodeEntity = new CourseCode { Name = newCode };
                    await _uow.CourseCodeRepository.CreateAsync(newCourseCodeEntity);
                    await _uow.SaveChangesAsync();

                    course.CourseCodeId = newCourseCodeEntity.Id;
                }
            }

            // [FIX START] Check total section duration against new course duration
            if (updateDto.DurationHours.HasValue)
            {
                var newDurationMinutes = updateDto.DurationHours.Value * 60;

                // Calculate total duration of existing sections (ignore deleted sections)
                var currentSectionTotalMinutes = await _uow.CourseSectionRepository.GetAllAsQueryable()
                    .Where(cs => cs.CourseId == id && cs.Section != null && cs.Section.IsDeleted != true)
                    .SumAsync(cs => cs.Section.EstimatedDurationMinutes ?? 0);

                if (newDurationMinutes < currentSectionTotalMinutes)
                {
                    throw new InvalidOperationException($"Cannot update course duration to {updateDto.DurationHours} hours ({newDurationMinutes} minutes). It is less than the total duration of existing sections ({currentSectionTotalMinutes} minutes).");
                }

                course.DurationHours = updateDto.DurationHours;
            }
            // [FIX END]

            course.Description = updateDto.Description ?? course.Description;
            course.CategoryId = updateDto.CategoryId ?? course.CategoryId;
            course.LevelId = updateDto.LevelId ?? course.LevelId;
            course.Price = updateDto.Price ?? course.Price;
            // course.DurationHours = updateDto.DurationHours ?? course.DurationHours; // Moved above to validation block
            course.ImageUrl = updateDto.ImageUrl ?? course.ImageUrl;
            course.IsActive = updateDto.IsActive ?? course.IsActive;
            course.BackgroundImageUrl = updateDto.BackgroundImageUrl ?? course.BackgroundImageUrl;

            await _uow.CourseRepository.UpdateAsync(course);
            await _uow.SaveChangesAsync();

            var updatedCourse = await _uow.CourseRepository.GetAllAsQueryable()
                .Include(c => c.Category)
                .Include(c => c.Level)
                .Include(c => c.CourseCode)
                .FirstOrDefaultAsync(c => c.Id == id);

            return MapToDto(updatedCourse!);
        }

        public async Task DeleteCourseAsync(int id)
        {
            var hasClasses = await _uow.ProgramCourseRepository
                .GetAllAsQueryable()
                .AnyAsync(pc => pc.CourseId == id && pc.Classes.Any());

            if (hasClasses)
            {
                throw new InvalidOperationException("Cannot delete course. It is currently assigned to one or more classes.");
            }

            var course = await _uow.CourseRepository
                .GetAllAsQueryable()
                .Where(c => c.Id == id)
                .Include(c => c.CourseCertificates)
                .Include(c => c.CourseSections)
                    .ThenInclude(cs => cs.Section)
                .Include(c => c.ProgramCourses)
                .FirstOrDefaultAsync();

            if (course == null || course.IsDeleted == true)
            {
                throw new KeyNotFoundException($"Course with ID {id} not found.");
            }

            if (course.CourseCertificates.Any())
            {
                foreach (var cert in course.CourseCertificates.ToList())
                {
                    await _uow.CourseCertificateRepository.DeleteAsync(cert);
                }
            }

            if (course.CourseSections.Any())
            {
                foreach (var cs in course.CourseSections.ToList())
                {
                    var sectionId = cs.SectionId;

                    await _uow.CourseSectionRepository.DeleteAsync(cs);

                    var isUsedElsewhere = await _uow.CourseSectionRepository
                        .GetAllAsQueryable()
                        .AnyAsync(x => x.SectionId == sectionId && x.CourseId != id);

                    if (!isUsedElsewhere && cs.Section != null)
                    {
                        cs.Section.IsDeleted = true;
                        await _uow.SectionRepository.UpdateAsync(cs.Section);
                    }
                }
            }

            if (course.ProgramCourses.Any())
            {
                foreach (var pc in course.ProgramCourses.ToList())
                {
                    await _uow.ProgramCourseRepository.DeleteAsync(pc);
                }
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
                .Include(pc => pc.Course)
                    .ThenInclude(c => c!.CourseCode) // [NEW] Include CourseCode
                .ToListAsync();

            var courses = programCourses
                .Where(pc => pc.Course != null && pc.Course.IsDeleted != true)
                .Select(pc => MapToDto(pc.Course!));
            return courses;
        }

        public async Task<IEnumerable<CourseDto>> GetAvailableCoursesByProgramIdAsync(int programId)
        {
            // Filter out invalid courses directly in query ===
            var programCourses = await _uow.ProgramCourseRepository
                .GetAllAsQueryable()
                .Where(pc => pc.ProgramId == programId
                          && pc.Course != null
                          && pc.Course.IsDeleted != true
                          && pc.Course.IsActive == true
                          && pc.Course.CourseCertificates.Any()
                          && pc.Course.CourseSections.Any())
                .Include(pc => pc.Course)
                    .ThenInclude(c => c!.Category)
                .Include(pc => pc.Course)
                    .ThenInclude(c => c!.Level)
                .Include(pc => pc.Course)
                    .ThenInclude(c => c!.CourseCode) // [NEW] Include CourseCode
                .ToListAsync();
            // ============================================================

            var courses = programCourses
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
                        .ThenInclude(co => co.Category)
                .Include(c => c.ProgramCourse)
                    .ThenInclude(pc => pc.Course)
                        .ThenInclude(co => co.Level)
                .Include(c => c.ProgramCourse)
                    .ThenInclude(pc => pc.Course)
                        .ThenInclude(co => co.CourseCode) // [NEW] Include CourseCode
                .AsNoTracking()
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

            return MapToDto(course);
        }
        #endregion

        #region Course Categories and Levels

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

        #region Helpers

        private async Task<string> GenerateUniqueCourseCodeAsync()
        {
            string code;
            bool exists;
            do
            {
                // Generate format C-XXXXXXXX (8 chars hex or random)
                // Using 8 chars substring of GUID for simplicity and high uniqueness
                code = "C-" + Guid.NewGuid().ToString("N").Substring(0, 8).ToUpper();
                exists = await _uow.CourseCodeRepository.GetAllAsQueryable().AnyAsync(c => c.Name == code);
            } while (exists);

            return code;
        }

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
                BackgroundImageUrl = c.BackgroundImageUrl,
                CourseCode = c.CourseCode?.Name // [NEW] Map CourseCode
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