using ExcelDataReader;
using Lssctc.ProgramManagement.Accounts.Authens.Services;
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
        private readonly IMailService _mailService;

        public SectionsService(IUnitOfWork uow, IMailService mailService)
        {
            _uow = uow;
            _mailService = mailService;
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


        public async Task<SectionDto> UpdateSectionAsync(int id, UpdateSectionDto updateDto)
        {
            var section = await _uow.SectionRepository.GetByIdAsync(id);
            if (section == null || section.IsDeleted == true)
            {
                throw new KeyNotFoundException($"Section with ID {id} not found.");
            }

            // Block update ONLY if the Section belongs to a Completed class
            if (await IsSectionFrozenAsync(id))
            {
                throw new InvalidOperationException("Cannot update section details. It is part of a course that is already completed.");
            }

            section.SectionTitle = updateDto.SectionTitle!.Trim();
            section.SectionDescription = string.IsNullOrWhiteSpace(updateDto.SectionDescription) ? null : updateDto.SectionDescription.Trim();
            section.EstimatedDurationMinutes = updateDto.EstimatedDurationMinutes!.Value;

            await _uow.SectionRepository.UpdateAsync(section);
            await _uow.SaveChangesAsync();

            // Post-Update: Send email notifications to trainees in Inprogress classes (background)
            var sectionTitle = section.SectionTitle;
            _ = Task.Run(async () =>
            {
                try
                {
                    await SendSectionUpdateNotificationsAsync(id, sectionTitle);
                }
                catch
                {
                    // Suppress any exceptions to avoid affecting the API response
                }
            });

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

            await _uow.CourseSectionRepository.DeleteAsync(courseSection);
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

            // A course is locked if it's part of a ProgramCourse
            // that is used by any Class with a locked status.
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

        /// <summary>
        /// Checks if a section is "frozen" (part of a Completed class).
        /// Frozen sections cannot be updated at all.
        /// </summary>
        private async Task<bool> IsSectionFrozenAsync(int sectionId)
        {
            // Find all CourseIDs this Section is linked to
            var courseIds = await _uow.CourseSectionRepository
                .GetAllAsQueryable()
                .Where(cs => cs.SectionId == sectionId)
                .Select(cs => cs.CourseId)
                .Distinct()
                .ToListAsync();

            if (!courseIds.Any())
            {
                return false; // Section isn't used by any course, so it's not frozen.
            }

            // Check if ANY of those courses are linked to a Completed class
            bool isFrozen = await _uow.ClassRepository
                .GetAllAsQueryable()
                .AnyAsync(c => courseIds.Contains(c.ProgramCourse.CourseId) &&
                               c.Status.HasValue &&
                               c.Status.Value == (int)ClassStatusEnum.Completed);
            return isFrozen;
        }

        /// <summary>
        /// Sends email notifications to all trainees enrolled in Inprogress classes 
        /// that use the updated section.
        /// </summary>
        private async Task SendSectionUpdateNotificationsAsync(int sectionId, string sectionTitle)
        {
            // Find all CourseIDs this Section is linked to
            var courseIds = await _uow.CourseSectionRepository
                .GetAllAsQueryable()
                .Where(cs => cs.SectionId == sectionId)
                .Select(cs => cs.CourseId)
                .Distinct()
                .ToListAsync();

            if (!courseIds.Any())
            {
                return; // No courses use this section
            }

            // Find all Inprogress classes that use these courses
            var inprogressClassIds = await _uow.ClassRepository
                .GetAllAsQueryable()
                .Where(c => courseIds.Contains(c.ProgramCourse.CourseId) &&
                            c.Status.HasValue &&
                            c.Status.Value == (int)ClassStatusEnum.Inprogress)
                .Select(c => c.Id)
                .ToListAsync();

            if (!inprogressClassIds.Any())
            {
                return; // No active classes use this section
            }

            // Find all trainees enrolled in these Inprogress classes with Inprogress enrollment status
            var traineeInfos = await _uow.EnrollmentRepository
                .GetAllAsQueryable()
                .Where(e => inprogressClassIds.Contains(e.ClassId) &&
                            e.Status == (int)EnrollmentStatusEnum.Inprogress)
                .Include(e => e.Trainee)
                    .ThenInclude(t => t.IdNavigation)
                .Include(e => e.Class)
                .Select(e => new
                {
                    TraineeFullname = e.Trainee.IdNavigation.Fullname ?? "Student",
                    TraineeEmail = e.Trainee.IdNavigation.Email,
                    ClassName = e.Class.Name
                })
                .Distinct()
                .ToListAsync();

            // Send emails to each trainee
            foreach (var trainee in traineeInfos)
            {
                if (string.IsNullOrWhiteSpace(trainee.TraineeEmail))
                {
                    continue; // Skip if no email
                }

                try
                {
                    string emailSubject = $"📚 Course Content Updated - {sectionTitle}";
                    string emailBody = GenerateSectionUpdateEmailBody(
                        trainee.TraineeFullname,
                        sectionTitle,
                        trainee.ClassName);

                    await _mailService.SendEmailAsync(trainee.TraineeEmail, emailSubject, emailBody);
                }
                catch
                {
                    // Silently ignore individual email failures
                }
            }
        }

        /// <summary>
        /// Generates the HTML email body for section update notifications.
        /// </summary>
        private static string GenerateSectionUpdateEmailBody(string traineeFullname, string sectionTitle, string className)
        {
            return $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='UTF-8'>
    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
    <style>
        body {{ 
            font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif; 
            line-height: 1.6; 
            color: #333; 
            margin: 0; 
            padding: 0; 
            background-color: #f4f4f4; 
        }}
        .email-container {{ 
            max-width: 600px; 
            margin: 20px auto; 
            background-color: #ffffff; 
            border-radius: 10px; 
            box-shadow: 0 2px 10px rgba(0,0,0,0.1); 
            overflow: hidden; 
        }}
        .email-header {{ 
            background: linear-gradient(135deg, #4CAF50 0%, #45a049 100%); 
            color: #ffffff; 
            padding: 30px 20px; 
            text-align: center; 
        }}
        .email-header h1 {{ 
            margin: 0; 
            font-size: 24px; 
            font-weight: 600; 
        }}
        .email-header .icon {{ 
            font-size: 48px; 
            margin-bottom: 10px; 
        }}
        .email-body {{ 
            padding: 30px; 
        }}
        .greeting {{ 
            font-size: 18px; 
            font-weight: 500; 
            color: #333; 
            margin-bottom: 20px; 
        }}
        .message {{ 
            font-size: 16px; 
            color: #555; 
            margin-bottom: 25px; 
            line-height: 1.8; 
        }}
        .update-info {{ 
            background-color: #f8f9fa; 
            border-left: 4px solid #4CAF50; 
            padding: 20px; 
            margin: 20px 0; 
            border-radius: 5px; 
        }}
        .update-info-title {{ 
            font-size: 18px; 
            font-weight: 600; 
            color: #4CAF50; 
            margin-bottom: 15px; 
        }}
        .info-row {{ 
            padding: 8px 0; 
            border-bottom: 1px solid #e0e0e0; 
        }}
        .info-row:last-child {{ 
            border-bottom: none; 
        }}
        .info-label {{ 
            font-weight: 600; 
            color: #666; 
        }}
        .info-value {{ 
            color: #333; 
            font-weight: 500; 
        }}
        .cta-button {{ 
            display: inline-block; 
            background: linear-gradient(135deg, #4CAF50 0%, #45a049 100%); 
            color: #ffffff; 
            padding: 14px 30px; 
            text-decoration: none; 
            border-radius: 5px; 
            font-weight: 600; 
            font-size: 16px; 
            margin: 20px 0; 
            text-align: center; 
        }}
        .footer {{ 
            background-color: #f8f9fa; 
            padding: 20px; 
            text-align: center; 
            font-size: 14px; 
            color: #666; 
        }}
        .footer-note {{ 
            margin-top: 15px; 
            font-size: 12px; 
            color: #999; 
        }}
        .highlight {{ 
            color: #4CAF50; 
            font-weight: 600; 
        }}
    </style>
</head>
<body>
    <div class='email-container'>
        <div class='email-header'>
            <div class='icon'>📚</div>
            <h1>Course Content Updated</h1>
        </div>
        
        <div class='email-body'>
            <div class='greeting'>
                Dear <span class='highlight'>{traineeFullname}</span>,
            </div>
            
            <div class='message'>
                We would like to inform you that the course content you are currently studying has been updated. Please review the changes to stay up-to-date with the latest learning materials.
            </div>
            
            <div class='update-info'>
                <div class='update-info-title'>📋 Update Details</div>
                <div class='info-row'>
                    <span class='info-label'>Updated Section:</span>
                    <span class='info-value'>{sectionTitle}</span>
                </div>
                <div class='info-row'>
                    <span class='info-label'>Class:</span>
                    <span class='info-value'>{className}</span>
                </div>
                <div class='info-row'>
                    <span class='info-label'>Update Time:</span>
                    <span class='info-value'>{DateTime.UtcNow:dd/MM/yyyy HH:mm} UTC</span>
                </div>
            </div>
            
            <div style='text-align: center;'>
                <a href='#' class='cta-button'>View Updated Content</a>
            </div>
            
            <div class='message' style='margin-top: 20px;'>
                If you have any questions about the updated content, please contact your instructor.
            </div>
            
            <div class='message' style='margin-top: 20px;'>
                Best regards,<br>
                <strong>LSSCTC Training Management Team</strong>
            </div>
        </div>
        
        <div class='footer'>
            <p>© 2024 LSSCTC Training Center. All rights reserved.</p>
            <p class='footer-note'>
                This is an automated message. Please do not reply directly to this email.
            </p>
        </div>
    </div>
</body>
</html>";
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