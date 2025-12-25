using ExcelDataReader;
using Lssctc.ProgramManagement.Sections.Dtos;
using Lssctc.Share.Common;
using Lssctc.Share.Entities;
using Lssctc.Share.Enums;
using Lssctc.Share.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Data;

namespace Lssctc.ProgramManagement.Sections.Services
{
    public class SectionsService : ISectionsService
    {
        private readonly IUnitOfWork _uow;
        public SectionsService(IUnitOfWork uow)
        {
            _uow = uow;
        }

        #region Sections
        public async Task<IEnumerable<SectionDto>> GetAllSectionsAsync()
        {
            var sections = await _uow.SectionRepository
                .GetAllAsQueryable()
                .Where(s => s.IsDeleted != true)
                .ToListAsync();

            return sections.Select(MapToDto);
        }

        public async Task<PagedResult<SectionDto>> GetSectionsAsync(int pageNumber, int pageSize)
        {
            if (pageNumber < 1) pageNumber = 1;
            if (pageSize < 1) pageSize = 10;

            var query = _uow.SectionRepository
                .GetAllAsQueryable()
                .Where(s => s.IsDeleted != true)
                .Select(s => MapToDto(s));

            var pagedResult = await query.ToPagedResultAsync(pageNumber, pageSize);
            return pagedResult;
        }

        public async Task<SectionDto?> GetSectionByIdAsync(int id)
        {
            var section = await _uow.SectionRepository
                .GetAllAsQueryable()
                .Where(s => s.Id == id && s.IsDeleted != true)
                .FirstOrDefaultAsync();

            return section == null ? null : MapToDto(section);
        }

        public async Task<SectionDto> CreateSectionAsync(CreateSectionDto createDto)
        {
            var section = new Section
            {
                SectionTitle = createDto.SectionTitle!.Trim(),
                SectionDescription = string.IsNullOrWhiteSpace(createDto.SectionDescription) ? null : createDto.SectionDescription.Trim(),
                EstimatedDurationMinutes = createDto.EstimatedDurationMinutes!.Value,
                IsDeleted = false
            };

            await _uow.SectionRepository.CreateAsync(section);
            await _uow.SaveChangesAsync();

            return MapToDto(section);
        }

        public async Task<SectionDto> CreateSectionForCourseAsync(int courseId, CreateSectionDto createDto)
        {
            if (await IsCourseLockedAsync(courseId))
            {
                throw new InvalidOperationException("Cannot create and add sections to this course. It is already in use by an active class.");
            }
            var course = await _uow.CourseRepository.GetByIdAsync(courseId);
            if (course == null || course.IsDeleted == true)
            {
                throw new KeyNotFoundException($"Course with ID {courseId} not found.");
            }

            // [FIX START] Check if total duration exceeds Course DurationHours
            var newSectionMinutes = createDto.EstimatedDurationMinutes ?? 0;
            if (course.DurationHours.HasValue)
            {
                var currentTotalMinutes = await _uow.CourseSectionRepository.GetAllAsQueryable()
                    .Where(cs => cs.CourseId == courseId && cs.Section.IsDeleted != true)
                    .SumAsync(cs => cs.Section.EstimatedDurationMinutes ?? 0);

                var maxMinutes = course.DurationHours.Value * 60;

                if (currentTotalMinutes + newSectionMinutes > maxMinutes)
                {
                    throw new InvalidOperationException($"Cannot add section. Total duration ({currentTotalMinutes + newSectionMinutes}m) would exceed the course limit of {maxMinutes}m ({course.DurationHours.Value}h).");
                }
            }
            // [FIX END]

            var section = new Section
            {
                SectionTitle = createDto.SectionTitle!.Trim(),
                SectionDescription = string.IsNullOrWhiteSpace(createDto.SectionDescription) ? null : createDto.SectionDescription.Trim(),
                EstimatedDurationMinutes = createDto.EstimatedDurationMinutes!.Value,
                IsDeleted = false
            };
            await _uow.SectionRepository.CreateAsync(section);
            await _uow.SaveChangesAsync();
            var maxOrder = await _uow.CourseSectionRepository
                .GetAllAsQueryable()
                .Where(cs => cs.CourseId == courseId)
                .MaxAsync(cs => (int?)cs.SectionOrder) ?? 0;
            var courseSection = new CourseSection
            {
                CourseId = courseId,
                SectionId = section.Id,
                SectionOrder = maxOrder + 1
            };

            await _uow.CourseSectionRepository.CreateAsync(courseSection);
            await _uow.SaveChangesAsync();

            return MapToDto(section);
        }


        public async Task<SectionDto> UpdateSectionAsync(int id, UpdateSectionDto updateDto)
        {
            // [FIX START] Include CourseSections and Course to check limits across all linked courses
            var section = await _uow.SectionRepository
                .GetAllAsQueryable()
                .Include(s => s.CourseSections)
                    .ThenInclude(cs => cs.Course)
                .Where(s => s.Id == id)
                .FirstOrDefaultAsync();
            // [FIX END]

            if (section == null || section.IsDeleted == true)
            {
                throw new KeyNotFoundException($"Section with ID {id} not found.");
            }

            // REMOVED: Lock check (IsSectionLockedAsync) to allow updates to title/description/duration 
            // even if the section is used in an active class.

            // 1. Validation: Unique Title Check
            if (updateDto.SectionTitle != null)
            {
                // Normalize title (remove leading/trailing whitespace and multiple internal spaces)
                var normalizedTitle = string.Join(' ', updateDto.SectionTitle.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries));

                // Check for duplicates only if the title has actually changed
                if (!string.Equals(section.SectionTitle, normalizedTitle, StringComparison.CurrentCultureIgnoreCase))
                {
                    var isDuplicate = await _uow.SectionRepository.GetAllAsQueryable()
                        .AnyAsync(s => s.SectionTitle == normalizedTitle && s.Id != id && s.IsDeleted != true);

                    if (isDuplicate)
                    {
                        throw new InvalidOperationException($"Section title '{normalizedTitle}' already exists.");
                    }
                    section.SectionTitle = normalizedTitle;
                }
            }

            // [FIX START] Validation: Duration Check for all associated courses
            if (updateDto.EstimatedDurationMinutes.HasValue)
            {
                var newDuration = updateDto.EstimatedDurationMinutes.Value;

                foreach (var cs in section.CourseSections)
                {
                    var course = cs.Course;
                    if (course != null && course.DurationHours.HasValue && course.IsDeleted != true)
                    {
                        var maxMinutes = course.DurationHours.Value * 60;

                        // Sum all sections in this course EXCEPT the current one
                        var otherSectionsTotal = await _uow.CourseSectionRepository.GetAllAsQueryable()
                            .Where(x => x.CourseId == course.Id && x.SectionId != id && x.Section.IsDeleted != true)
                            .SumAsync(x => x.Section.EstimatedDurationMinutes ?? 0);

                        if (otherSectionsTotal + newDuration > maxMinutes)
                        {
                            throw new InvalidOperationException($"Cannot update section. Total duration would exceed limit for course '{course.Name}' ({maxMinutes}m).");
                        }
                    }
                }

                section.EstimatedDurationMinutes = newDuration;
            }
            // [FIX END]

            // 2. Update other fields
            if (updateDto.SectionDescription != null)
            {
                section.SectionDescription = string.IsNullOrWhiteSpace(updateDto.SectionDescription)
                    ? null
                    : string.Join(' ', updateDto.SectionDescription.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries));
            }

            // Note: Duration update moved up into the validation block

            await _uow.SectionRepository.UpdateAsync(section);
            await _uow.SaveChangesAsync();

            return MapToDto(section);
        }

        public async Task DeleteSectionAsync(int id)
        {
            var section = await _uow.SectionRepository
                .GetAllAsQueryable()
                .Where(s => s.Id == id)
                .Include(s => s.CourseSections)
                .FirstOrDefaultAsync();

            if (section == null || section.IsDeleted == true)
            {
                throw new KeyNotFoundException($"Section with ID {id} not found.");
            }

            // Check if section is associated with any courses
            if (section.CourseSections != null && section.CourseSections.Any())
            {
                // --- MODIFIED LOGIC (BR 2) ---
                // Also check if any of those courses are locked
                if (await IsSectionLockedAsync(id))
                {
                    throw new InvalidOperationException("Cannot delete section. It is part of a course that is already in progress or completed.");
                }
                // --- END MODIFIED LOGIC ---

                throw new InvalidOperationException("Cannot delete section associated with courses. Please remove it from all courses first.");
            }

            section.IsDeleted = true;
            await _uow.SectionRepository.UpdateAsync(section);
            await _uow.SaveChangesAsync();
        }
        #endregion

        #region Course Sections

        public async Task<IEnumerable<SectionDto>> GetSectionsByCourseIdAsync(int courseId)
        {
            var courseSections = await _uow.CourseSectionRepository
                .GetAllAsQueryable()
                .Where(cs => cs.CourseId == courseId)
                .OrderBy(cs => cs.SectionOrder)
                .Include(cs => cs.Section)
                .ToListAsync();

            var sections = courseSections
                .Where(cs => cs.Section != null && cs.Section.IsDeleted != true)
                .Select(cs => MapToDto(cs.Section!));

            return sections;
        }

        public async Task AddSectionToCourseAsync(int courseId, int sectionId)
        {
            // --- ADDED LOGIC (BR 1) ---
            if (await IsCourseLockedAsync(courseId))
            {
                throw new InvalidOperationException("Cannot add sections to this course. It is already in use by an active class.");
            }
            // --- END ADDED LOGIC ---

            // Verify course exists
            var course = await _uow.CourseRepository.GetByIdAsync(courseId);
            if (course == null || course.IsDeleted == true)
            {
                throw new KeyNotFoundException($"Course with ID {courseId} not found.");
            }

            // Verify section exists
            var section = await _uow.SectionRepository.GetByIdAsync(sectionId);
            if (section == null || section.IsDeleted == true)
            {
                throw new KeyNotFoundException($"Section with ID {sectionId} not found.");
            }

            // Check if the section is already added to the course
            var existingCourseSection = await _uow.CourseSectionRepository
                .GetAllAsQueryable()
                .Where(cs => cs.CourseId == courseId && cs.SectionId == sectionId)
                .FirstOrDefaultAsync();

            if (existingCourseSection != null)
            {
                throw new InvalidOperationException($"Section with ID {sectionId} is already added to course {courseId}.");
            }

            // Get the max order for this course
            var maxOrder = await _uow.CourseSectionRepository
                .GetAllAsQueryable()
                .Where(cs => cs.CourseId == courseId)
                .MaxAsync(cs => (int?)cs.SectionOrder) ?? 0;

            var courseSection = new CourseSection
            {
                CourseId = courseId,
                SectionId = sectionId,
                SectionOrder = maxOrder + 1
            };

            await _uow.CourseSectionRepository.CreateAsync(courseSection);
            await _uow.SaveChangesAsync();
        }

        public async Task RemoveSectionFromCourseAsync(int courseId, int sectionId)
        {
            // --- ADDED LOGIC (BR 1) ---
            if (await IsCourseLockedAsync(courseId))
            {
                throw new InvalidOperationException("Cannot remove sections from this course. It is already in use by an active class.");
            }
            // --- END ADDED LOGIC ---

            var courseSection = await _uow.CourseSectionRepository
                .GetAllAsQueryable()
                .Where(cs => cs.CourseId == courseId && cs.SectionId == sectionId)
                .FirstOrDefaultAsync();

            if (courseSection == null)
            {
                throw new KeyNotFoundException($"Section with ID {sectionId} not found in course {courseId}.");
            }

            // 1. Delete the link (Remove section from THIS course)
            await _uow.CourseSectionRepository.DeleteAsync(courseSection);

            // 2. Orphan Check: Check if this section is used by any *other* course
            var isUsedElsewhere = await _uow.CourseSectionRepository
                .GetAllAsQueryable()
                .AnyAsync(cs => cs.SectionId == sectionId && cs.CourseId != courseId);

            // 3. If not used elsewhere, Soft Delete the Section origin
            if (!isUsedElsewhere)
            {
                var section = await _uow.SectionRepository.GetByIdAsync(sectionId);
                if (section != null && section.IsDeleted != true)
                {
                    section.IsDeleted = true;
                    await _uow.SectionRepository.UpdateAsync(section);
                }
            }

            await _uow.SaveChangesAsync();

            // Reorder remaining sections
            await ReorderCourseSectionsAsync(courseId);
        }

        public async Task UpdateCourseSectionOrderAsync(int courseId, int sectionId, int newOrder)
        {
            // --- ADDED LOGIC (BR 1) ---
            if (await IsCourseLockedAsync(courseId))
            {
                throw new InvalidOperationException("Cannot re-order sections for this course. It is already in use by an active class.");
            }
            // --- END ADDED LOGIC ---

            if (newOrder < 1)
            {
                throw new ArgumentException("Section order must be greater than 0.", nameof(newOrder));
            }

            var courseSection = await _uow.CourseSectionRepository
                .GetAllAsQueryable()
                .Where(cs => cs.CourseId == courseId && cs.SectionId == sectionId)
                .FirstOrDefaultAsync();

            if (courseSection == null)
            {
                throw new KeyNotFoundException($"Section with ID {sectionId} not found in course {courseId}.");
            }

            var allCourseSections = await _uow.CourseSectionRepository
                .GetAllAsQueryable()
                .Where(cs => cs.CourseId == courseId)
                .OrderBy(cs => cs.SectionOrder)
                .ToListAsync();

            var oldOrder = courseSection.SectionOrder;

            if (oldOrder == newOrder)
            {
                return; // No change needed
            }

            // Remove from current position
            allCourseSections.Remove(courseSection);

            // Insert at new position (newOrder - 1 because list is 0-indexed)
            var insertIndex = Math.Min(newOrder - 1, allCourseSections.Count);
            allCourseSections.Insert(insertIndex, courseSection);

            // Update all orders
            for (int i = 0; i < allCourseSections.Count; i++)
            {
                allCourseSections[i].SectionOrder = i + 1;
                await _uow.CourseSectionRepository.UpdateAsync(allCourseSections[i]);
            }

            await _uow.SaveChangesAsync();
        }

        private async Task ReorderCourseSectionsAsync(int courseId)
        {
            var courseSections = await _uow.CourseSectionRepository
                .GetAllAsQueryable()
                .Where(cs => cs.CourseId == courseId)
                .OrderBy(cs => cs.SectionOrder)
                .ToListAsync();

            for (int i = 0; i < courseSections.Count; i++)
            {
                courseSections[i].SectionOrder = i + 1;
                await _uow.CourseSectionRepository.UpdateAsync(courseSections[i]);
            }

            await _uow.SaveChangesAsync();
        }

        #endregion

        #region Import Sections
        public async Task<IEnumerable<SectionDto>> ImportSectionsFromExcelAsync(int courseId, IFormFile file)
        {
            // 1. Validation: Check if course is locked (BR 1)
            if (await IsCourseLockedAsync(courseId))
            {
                throw new InvalidOperationException("Cannot import sections. The course is currently in use by an active class.");
            }

            // 2. Validation: Verify course exists
            var course = await _uow.CourseRepository.GetByIdAsync(courseId);
            if (course == null || course.IsDeleted == true)
            {
                throw new KeyNotFoundException($"Course with ID {courseId} not found.");
            }

            // 3. Configure Encoding for ExcelDataReader (Required for .NET Core)
            System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);

            var newSections = new List<Section>();

            // 4. Read Excel File
            using (var stream = file.OpenReadStream())
            using (var reader = ExcelReaderFactory.CreateReader(stream))
            {
                // Read content as a DataSet
                var result = reader.AsDataSet(new ExcelDataSetConfiguration()
                {
                    ConfigureDataTable = (_) => new ExcelDataTableConfiguration()
                    {
                        UseHeaderRow = true // Assumes the first row is "Title", "Description", etc.
                    }
                });

                var dataTable = result.Tables[0]; // Get the first sheet

                if (dataTable.Rows.Count == 0)
                    throw new ArgumentException("The uploaded Excel file is empty.");

                foreach (DataRow row in dataTable.Rows)
                {
                    // Safely extract values (Adjust column indexes 0, 1, 2 based on your Excel template)
                    string title = row[0]?.ToString()?.Trim() ?? "";
                    string description = row[1]?.ToString()?.Trim() ?? "";

                    // Parse Duration (Default to 0 or skip if invalid)
                    if (!int.TryParse(row[2]?.ToString(), out int duration))
                    {
                        duration = 60; // Default value if parsing fails
                    }

                    if (string.IsNullOrEmpty(title)) continue; // Skip rows without titles

                    var section = new Section
                    {
                        SectionTitle = title,
                        SectionDescription = string.IsNullOrEmpty(description) ? null : description,
                        EstimatedDurationMinutes = duration,
                        IsDeleted = false
                    };

                    newSections.Add(section);

                    // Add to repository context
                    await _uow.SectionRepository.CreateAsync(section);
                }
            }

            if (!newSections.Any())
            {
                throw new ArgumentException("No valid sections found in the file.");
            }

            // 5. Save Sections First (To generate IDs)
            await _uow.SaveChangesAsync();

            // 6. Link to Course (Create CourseSection entities)

            // Get current max order to append to the end
            var currentMaxOrder = await _uow.CourseSectionRepository
                .GetAllAsQueryable()
                .Where(cs => cs.CourseId == courseId)
                .MaxAsync(cs => (int?)cs.SectionOrder) ?? 0;

            int orderCounter = 1;
            foreach (var section in newSections)
            {
                var courseSection = new CourseSection
                {
                    CourseId = courseId,
                    SectionId = section.Id, // This Id is now available after the first SaveChanges
                    SectionOrder = currentMaxOrder + orderCounter
                };

                await _uow.CourseSectionRepository.CreateAsync(courseSection);
                orderCounter++;
            }

            // 7. Save Links
            await _uow.SaveChangesAsync();

            // 8. Return Result
            return newSections.Select(MapToDto);
        }

        public async Task<IEnumerable<SectionDto>> ImportSectionsWithActivitiesFromExcelAsync(int courseId, IFormFile file)
        {
            // 1. Validation
            if (await IsCourseLockedAsync(courseId))
            {
                throw new InvalidOperationException("Cannot import to a locked course.");
            }

            var course = await _uow.CourseRepository.GetByIdAsync(courseId);
            if (course == null || course.IsDeleted == true)
                throw new KeyNotFoundException($"Course with ID {courseId} not found.");

            System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);

            var createdSections = new List<Section>();
            Section? currentSection = null;

            // Get current max order for the course to append new sections at the end
            var currentSectionOrder = await _uow.CourseSectionRepository
                .GetAllAsQueryable()
                .Where(cs => cs.CourseId == courseId)
                .MaxAsync(cs => (int?)cs.SectionOrder) ?? 0;

            using (var stream = file.OpenReadStream())
            using (var reader = ExcelReaderFactory.CreateReader(stream))
            {
                var result = reader.AsDataSet(new ExcelDataSetConfiguration()
                {
                    ConfigureDataTable = (_) => new ExcelDataTableConfiguration() { UseHeaderRow = true }
                });

                var dataTable = result.Tables[0];
                if (dataTable.Rows.Count == 0) throw new ArgumentException("Excel file is empty.");

                foreach (DataRow row in dataTable.Rows)
                {
                    // --- Column Mapping ---
                    // 0: Section Title
                    // 1: Section Description
                    // 2: Activity Type (Material, Quiz, Practice)
                    // 3: Activity Name
                    // 4: Activity Description / Link
                    // 5: Duration (Minutes)

                    string secTitle = row[0]?.ToString()?.Trim() ?? "";
                    string secDesc = row[1]?.ToString()?.Trim() ?? "";

                    string actTypeStr = row[2]?.ToString()?.Trim() ?? "";
                    string actName = row[3]?.ToString()?.Trim() ?? "";
                    string actDesc = row[4]?.ToString()?.Trim() ?? "";

                    // Parse Duration
                    int.TryParse(row[5]?.ToString(), out int duration);
                    if (duration <= 0) duration = 0;

                    // === LOGIC: NEW SECTION ===
                    if (!string.IsNullOrEmpty(secTitle))
                    {
                        // Create and Save Section
                        var newSection = new Section
                        {
                            SectionTitle = secTitle,
                            SectionDescription = string.IsNullOrEmpty(secDesc) ? null : secDesc,
                            EstimatedDurationMinutes = duration > 0 ? duration : 0, // Default to 0 if not set, or sum activities later
                            IsDeleted = false
                        };

                        await _uow.SectionRepository.CreateAsync(newSection);
                        await _uow.SaveChangesAsync(); // Save to get Id

                        // Link to Course
                        currentSectionOrder++;
                        var courseSection = new CourseSection
                        {
                            CourseId = courseId,
                            SectionId = newSection.Id,
                            SectionOrder = currentSectionOrder
                        };
                        await _uow.CourseSectionRepository.CreateAsync(courseSection);
                        await _uow.SaveChangesAsync();

                        currentSection = newSection;
                        createdSections.Add(newSection);
                    }

                    // === LOGIC: NEW ACTIVITY (Attached to Current Section) ===
                    // Valid if we have a current section AND an Activity Type is specified
                    if (currentSection != null && !string.IsNullOrEmpty(actTypeStr) && !string.IsNullOrEmpty(actName))
                    {
                        if (Enum.TryParse<ActivityType>(actTypeStr, true, out var activityTypeEnum))
                        {
                            // 1. Create Generic Activity
                            var newActivity = new Activity
                            {
                                ActivityTitle = actName,
                                ActivityDescription = string.IsNullOrEmpty(actDesc) ? null : actDesc,
                                ActivityType = (int)activityTypeEnum,
                                EstimatedDurationMinutes = duration,
                                IsDeleted = false
                            };

                            await _uow.ActivityRepository.CreateAsync(newActivity);
                            await _uow.SaveChangesAsync();

                            // 2. Link to Section
                            // Get next order for this specific section
                            var actOrder = await _uow.SectionActivityRepository.GetAllAsQueryable()
                                .Where(sa => sa.SectionId == currentSection.Id)
                                .CountAsync() + 1;

                            var sectionActivity = new SectionActivity
                            {
                                SectionId = currentSection.Id,
                                ActivityId = newActivity.Id,
                                ActivityOrder = actOrder
                            };
                            await _uow.SectionActivityRepository.CreateAsync(sectionActivity);
                            await _uow.SaveChangesAsync();
                        }
                    }
                }
            }

            return createdSections.Select(MapToDto);
        }

        #endregion

        #region --- ADDED HELPER METHODS ---

        /// <summary>
        /// Checks if a course is "locked" (i.e., tied to a class that is InProgress, Completed, or Cancelled).
        /// </summary>
        private async Task<bool> IsCourseLockedAsync(int courseId)
        {
            var lockedStatuses = new[] {
                (int)ClassStatusEnum.Inprogress,
                (int)ClassStatusEnum.Completed,
                (int)ClassStatusEnum.Cancelled
            };

            bool isLocked = await _uow.ClassRepository
                .GetAllAsQueryable()
                .AnyAsync(c => c.ProgramCourse.CourseId == courseId &&
                               c.Status.HasValue &&
                               lockedStatuses.Contains(c.Status.Value));
            return isLocked;
        }

        /// <summary>
        /// Checks if a specific section is "locked" by being part of any locked course.
        /// </summary>
        private async Task<bool> IsSectionLockedAsync(int sectionId)
        {
            var lockedStatuses = new[] {
                (int)ClassStatusEnum.Inprogress,
                (int)ClassStatusEnum.Completed,
                (int)ClassStatusEnum.Cancelled
            };

            // Find all CourseIDs this Section is linked to
            var courseIds = await _uow.CourseSectionRepository
                .GetAllAsQueryable()
                .Where(cs => cs.SectionId == sectionId)
                .Select(cs => cs.CourseId)
                .Distinct()
                .ToListAsync();

            if (!courseIds.Any())
            {
                return false; // Section isn't used by any course, so it's not locked.
            }

            // Check if ANY of those courses are linked to an active class
            bool isLocked = await _uow.ClassRepository
                .GetAllAsQueryable()
                .AnyAsync(c => courseIds.Contains(c.ProgramCourse.CourseId) &&
                               c.Status.HasValue &&
                               lockedStatuses.Contains(c.Status.Value));
            return isLocked;
        }

        #endregion

        #region Mapping
        private static SectionDto MapToDto(Section s)
        {
            return new SectionDto
            {
                Id = s.Id,
                SectionTitle = s.SectionTitle,
                SectionDescription = s.SectionDescription,
                EstimatedDurationMinutes = s.EstimatedDurationMinutes
            };
        }
        #endregion
    }
}