using Lssctc.ProgramManagement.Accounts.Authens.Services;
using Lssctc.ProgramManagement.Accounts.Helpers;
using Lssctc.ProgramManagement.Activities.Services;
using Lssctc.ProgramManagement.ClassManage.Classes.Dtos;
using Lssctc.ProgramManagement.ClassManage.FinalExams.Services;
using Lssctc.ProgramManagement.ClassManage.Helpers;
using Lssctc.ProgramManagement.ClassManage.Timeslots.Services;
using Lssctc.Share.Common;
using Lssctc.Share.Entities;
using Lssctc.Share.Enums;
using Lssctc.Share.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using System.Data;

namespace Lssctc.ProgramManagement.ClassManage.Classes.Services
{
    public class ClassesService : IClassesService
    {
        private readonly IUnitOfWork _uow;
        private readonly ClassManageHandler _handler;
        private readonly IMailService _mailService;
        private readonly ITimeslotService _timeslotService;
        private readonly IActivitySessionService _activitySessionService;
        private readonly IFinalExamsService _finalExamsService;
        private static readonly Random _random = new Random();

        public ClassesService(
        IUnitOfWork uow,
        IMailService mailService,
        ITimeslotService timeslotService,
        IActivitySessionService activitySessionService,
        IFinalExamsService finalExamsService) 
        {
            _uow = uow;
            _mailService = mailService;
            _timeslotService = timeslotService;
            _activitySessionService = activitySessionService;
            _finalExamsService = finalExamsService; 
            _handler = new ClassManageHandler(uow);
        }

        #region Classes

        public async Task<IEnumerable<ClassDto>> GetAllClassesAsync()
        {
            var classes = await _uow.ClassRepository
                .GetAllAsQueryable()
                .Include(c => c.ProgramCourse)
                    .ThenInclude(pc => pc.Course)
                .Include(c => c.ClassCode)
                .ToListAsync();

            return classes.Select(MapToDto);
        }

        public async Task<PagedResult<ClassDto>> GetClassesAsync(int pageNumber, int pageSize, string? searchTerm = null, string? sortBy = null, string? sortDirection = null, string? status = null)
        {
            if (pageNumber < 1) pageNumber = 1;
            if (pageSize < 1) pageSize = 10;

            var query = _uow.ClassRepository
                .GetAllAsQueryable()
                .Include(c => c.ProgramCourse)
                    .ThenInclude(pc => pc.Course)
                .Include(c => c.ClassCode)
                .AsQueryable();

            // 1. Status Filtering (String-based)
            if (!string.IsNullOrWhiteSpace(status))
            {
                // Try to parse the status string to ClassStatusEnum (case-insensitive)
                if (Enum.TryParse<ClassStatusEnum>(status, ignoreCase: true, out var parsedStatus))
                {
                    query = query.Where(c => c.Status == (int)parsedStatus);
                }
                else
                {
                    // If parsing fails, return empty result (invalid status)
                    return new PagedResult<ClassDto>
                    {
                        Items = new List<ClassDto>(),
                        TotalCount = 0,
                        Page = pageNumber,
                        PageSize = pageSize
                    };
                }
            }

            // 2. Search Term Filtering
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                var searchLower = searchTerm.ToLower();
                query = query.Where(c =>
                    c.Name.ToLower().Contains(searchLower) ||
                    (c.ClassCode != null && c.ClassCode.Name.ToLower().Contains(searchLower)) ||
                    (c.Description != null && c.Description.ToLower().Contains(searchLower))
                );
            }

            // 3. Sorting Logic
            if (!string.IsNullOrWhiteSpace(sortBy))
            {
                // Scenario A: Explicit Sort by StartDate or EndDate
                var direction = sortDirection?.ToLower() ?? "asc";
                
                if (sortBy.ToLower() == "startdate")
                {
                    query = direction == "desc" 
                        ? query.OrderByDescending(c => c.StartDate) 
                        : query.OrderBy(c => c.StartDate);
                }
                else if (sortBy.ToLower() == "enddate")
                {
                    query = direction == "desc" 
                        ? query.OrderByDescending(c => c.EndDate) 
                        : query.OrderBy(c => c.EndDate);
                }
            }
            else if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                // Scenario B: Search Relevance Sort (when searchTerm provided but no explicit sortBy)
                var searchLower = searchTerm.ToLower();
                query = query.OrderByDescending(c =>
                    // Priority 1: Match in Name or ClassCode
                    (c.Name.ToLower().Contains(searchLower) || 
                     (c.ClassCode != null && c.ClassCode.Name.ToLower().Contains(searchLower))) ? 1 : 0
                ).ThenByDescending(c =>
                    // Priority 2: Match in Description
                    (c.Description != null && c.Description.ToLower().Contains(searchLower)) ? 1 : 0
                );
            }
            // Scenario C: Default sorting (keep existing order if no sorting specified)

            // 4. Apply Pagination
            var pagedEntities = await query.ToPagedResultAsync(pageNumber, pageSize);

            var dtoItems = pagedEntities.Items.Select(MapToDto).ToList();

            return new PagedResult<ClassDto>
            {
                Items = dtoItems,
                TotalCount = pagedEntities.TotalCount,
                Page = pagedEntities.Page,
                PageSize = pagedEntities.PageSize
            };
        }

        public async Task<ClassDto?> GetClassByIdAsync(int id)
        {
            var c = await _uow.ClassRepository
                .GetAllAsQueryable()
                .Include(c => c.ProgramCourse)
                    .ThenInclude(pc => pc.Course)
                .Include(c => c.ClassCode)
                .FirstOrDefaultAsync(c => c.Id == id);

            return c == null ? null : MapToDto(c);
        }

        public async Task<ClassDto> CreateClassAsync(CreateClassDto dto)
        {
            if (dto.StartDate < DateTime.UtcNow)
                throw new InvalidOperationException("Start date cannot be in the past.");

            if (!dto.EndDate.HasValue || dto.EndDate <= dto.StartDate.AddDays(2))
                throw new InvalidOperationException("End date must be at least 3 days after the start date.");

            if (string.IsNullOrWhiteSpace(dto.ClassCode))
                throw new ArgumentException("Class code is required.");

            // Check for existing class code (case-insensitive)
            var existingClassCode = await _uow.ClassCodeRepository
                .GetAllAsQueryable()
                .FirstOrDefaultAsync(cc => cc.Name.ToLower() == dto.ClassCode.Trim().ToLower());

            if (existingClassCode != null)
                throw new InvalidOperationException($"Class code '{existingClassCode.Name}' already exists.");

            // Find the matching ProgramCourse by ProgramId and CourseId
            var programCourse = await _uow.ProgramCourseRepository
                .GetAllAsQueryable()
                .FirstOrDefaultAsync(pc => pc.ProgramId == dto.ProgramId && pc.CourseId == dto.CourseId);

            if (programCourse == null)
                throw new KeyNotFoundException("No matching ProgramCourse found for the given ProgramId and CourseId.");

            // Create the ClassCode
            var classCodeEntity = new ClassCode
            {
                Name = dto.ClassCode.Trim()
            };
            await _uow.ClassCodeRepository.CreateAsync(classCodeEntity);
            await _uow.SaveChangesAsync();

            // Create the new Class
            var newClass = new Class
            {
                Name = dto.Name.Trim(),
                Capacity = dto.Capacity,
                ProgramCourseId = programCourse.Id,
                ClassCodeId = classCodeEntity.Id,
                Description = dto.Description?.Trim() ?? string.Empty,
                StartDate = dto.StartDate,
                EndDate = dto.EndDate,
                Status = (int)ClassStatusEnum.Draft
            };

            await _uow.ClassRepository.CreateAsync(newClass);
            await _uow.SaveChangesAsync();

            return MapToDto(newClass);
        }

        public async Task<ClassDto> UpdateClassAsync(int id, UpdateClassDto dto)
        {
            var existing = await _uow.ClassRepository
                .GetAllAsQueryable()
                .Where(c => c.Id == id)
                .Include(c => c.ProgramCourse)
                    .ThenInclude(pc => pc.Course)
                .Include(c => c.ClassCode)
                .FirstOrDefaultAsync()
                ;
            if (existing == null)
                throw new KeyNotFoundException($"Class with ID {id} not found.");

            if (existing.Status != (int)ClassStatusEnum.Draft)
                throw new InvalidOperationException("Only classes in 'Draft' status can be updated.");

            if (dto.StartDate < DateTime.UtcNow)
                throw new InvalidOperationException("Start date cannot be in the past.");

            if (!dto.EndDate.HasValue || dto.EndDate <= dto.StartDate.AddDays(2))
                throw new InvalidOperationException("End date must be at least 3 days after the start date.");

            existing.Name = dto.Name.Trim();
            existing.Capacity = dto.Capacity;
            existing.Description = dto.Description?.Trim() ?? existing.Description;
            existing.StartDate = dto.StartDate;
            existing.EndDate = dto.EndDate;

            await _uow.ClassRepository.UpdateAsync(existing);
            await _uow.SaveChangesAsync();

            return MapToDto(existing);
        }

        public async Task OpenClassAsync(int id)
        {
            var existing = await _uow.ClassRepository.GetByIdAsync(id);
            if (existing == null)
                throw new KeyNotFoundException($"Class with ID {id} not found.");

            if (existing.Status != (int)ClassStatusEnum.Draft)
                throw new InvalidOperationException("Only 'Draft' classes can be opened.");

            existing.Status = (int)ClassStatusEnum.Open;

            await _uow.ClassRepository.UpdateAsync(existing);
            await _uow.SaveChangesAsync();
        }

        public async Task StartClassAsync(int id)
        {
            var existing = await _uow.ClassRepository
                .GetAllAsQueryable()
                .Include(c => c.ClassInstructors)
                .Include(c => c.Enrollments)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (existing == null)
                throw new KeyNotFoundException($"Class with ID {id} not found.");

            if (existing.Status != (int)ClassStatusEnum.Draft &&
                existing.Status != (int)ClassStatusEnum.Open)
                throw new InvalidOperationException("Only 'Draft' or 'Open' classes can be started.");

            if (existing.EndDate <= existing.StartDate.AddDays(2))
                throw new InvalidOperationException("Invalid start or end date.");

            if (existing.ClassInstructors == null || !existing.ClassInstructors.Any())
                throw new InvalidOperationException("Cannot start class without instructors.");

            // Check for at least one *enrolled* student, not just *any* enrollment
            if (existing.Enrollments == null || !existing.Enrollments.Any(e => e.Status == (int)EnrollmentStatusEnum.Enrolled))
                throw new InvalidOperationException("Cannot start class without at least one enrolled student.");

            // Update enrollment statuses
            foreach (var enrollment in existing.Enrollments)
            {
                if (enrollment.Status == (int)EnrollmentStatusEnum.Enrolled)
                {
                    // Move enrolled students to Inprogress
                    enrollment.Status = (int)EnrollmentStatusEnum.Inprogress;
                    await _uow.EnrollmentRepository.UpdateAsync(enrollment);
                }
                else if (enrollment.Status == (int)EnrollmentStatusEnum.Pending)
                {
                    // Auto-reject pending applications as the class is starting
                    enrollment.Status = (int)EnrollmentStatusEnum.Rejected;
                    await _uow.EnrollmentRepository.UpdateAsync(enrollment);
                }
            }

            existing.Status = (int)ClassStatusEnum.Inprogress;
            await _uow.ClassRepository.UpdateAsync(existing);
            await _uow.SaveChangesAsync();

            await _handler.EnsureProgressScaffoldingForClassAsync(id);
            // Create Attendance Records for all Timeslots 
            await _timeslotService.CreateAttendanceForClassAsync(id);
            await _activitySessionService.CreateSessionsOnClassStartAsync(id);
            await _finalExamsService.AutoCreateFinalExamsForClassAsync(id);
            //// Find all 'NotStarted' progresses for this class and set them to 'InProgress'
            //var progressesToStart = await _uow.LearningProgressRepository
            //    .GetAllAsQueryable()
            //    .Where(lp => lp.Enrollment.ClassId == id && lp.Status == (int)LearningProgressStatusEnum.NotStarted)
            //    .ToListAsync();

            //foreach (var progress in progressesToStart)
            //{
            //    progress.Status = (int)LearningProgressStatusEnum.InProgress;
            //    progress.LastUpdated = DateTime.UtcNow;
            //    await _uow.LearningProgressRepository.UpdateAsync(progress);
            //}

        }

        public async Task CompleteClassAsync(int id)
        {
            var existing = await _uow.ClassRepository.GetByIdAsync(id);
            if (existing == null)
                throw new KeyNotFoundException($"Class with ID {id} not found.");

            if (existing.Status != (int)ClassStatusEnum.Inprogress)
                throw new InvalidOperationException("Only 'Inprogress' classes can be completed.");

            existing.Status = (int)ClassStatusEnum.Completed;

            await _uow.ClassRepository.UpdateAsync(existing);
            await _uow.SaveChangesAsync();
        }

        public async Task CancelClassAsync(int id)
        {
            var existing = await _uow.ClassRepository
                .GetAllAsQueryable()
                .Include(c => c.Enrollments)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (existing == null)
                throw new KeyNotFoundException($"Class with ID {id} not found.");

            if (existing.Enrollments != null && existing.Enrollments.Any())
                throw new InvalidOperationException("Cannot cancel a class with enrolled students.");

            if (existing.Status == (int)ClassStatusEnum.Inprogress ||
                existing.Status == (int)ClassStatusEnum.Completed ||
                existing.Status == (int)ClassStatusEnum.Cancelled)
                throw new InvalidOperationException("Cannot cancel a class that is in progress, completed, or already cancelled.");

            existing.Status = (int)ClassStatusEnum.Cancelled;

            await _uow.ClassRepository.UpdateAsync(existing);
            await _uow.SaveChangesAsync();
        }

        public async Task DeleteClassDataRecursiveAsync(int classId)
        {
            // Begin database transaction to ensure atomicity
            var dbContext = _uow.GetDbContext();
            await using var transaction = await dbContext.Database.BeginTransactionAsync();

            try
            {
                // 1. Check if class exists
                var classEntity = await _uow.ClassRepository
                    .GetAllAsQueryable()
                    .FirstOrDefaultAsync(c => c.Id == classId);

                if (classEntity == null)
                    throw new KeyNotFoundException($"Class with ID {classId} not found.");

                // 2. Get all enrollments for this class
                var enrollments = await _uow.EnrollmentRepository
                    .GetAllAsQueryable()
                    .Where(e => e.ClassId == classId)
                    .ToListAsync();

                var enrollmentIds = enrollments.Select(e => e.Id).ToList();

                // 3. Delete child data in proper order to avoid FK constraints

                // 3.1 Delete QuizAttemptAnswers (child of QuizAttemptQuestions)
                if (enrollmentIds.Any())
                {
                    var quizAttempts = await _uow.QuizAttemptRepository
                        .GetAllAsQueryable()
                        .Where(qa => enrollmentIds.Contains(qa.ActivityRecord.SectionRecord.LearningProgress.EnrollmentId))
                        .Include(qa => qa.QuizAttemptQuestions)
                        .ThenInclude(qaq => qaq.QuizAttemptAnswers)
                        .ToListAsync();

                    foreach (var quizAttempt in quizAttempts)
                    {
                        foreach (var question in quizAttempt.QuizAttemptQuestions)
                        {
                            foreach (var answer in question.QuizAttemptAnswers)
                            {
                                await _uow.QuizAttemptAnswerRepository.DeleteAsync(answer);
                            }
                        }
                    }
                }

                // 3.2 Delete QuizAttemptQuestions (child of QuizAttempts)
                if (enrollmentIds.Any())
                {
                    var quizAttemptQuestions = await _uow.QuizAttemptQuestionRepository
                        .GetAllAsQueryable()
                        .Where(qaq => enrollmentIds.Contains(qaq.QuizAttempt.ActivityRecord.SectionRecord.LearningProgress.EnrollmentId))
                        .ToListAsync();

                    foreach (var question in quizAttemptQuestions)
                    {
                        await _uow.QuizAttemptQuestionRepository.DeleteAsync(question);
                    }
                }

                // 3.3 Delete QuizAttempts (child of ActivityRecords)
                if (enrollmentIds.Any())
                {
                    var quizAttempts = await _uow.QuizAttemptRepository
                        .GetAllAsQueryable()
                        .Where(qa => enrollmentIds.Contains(qa.ActivityRecord.SectionRecord.LearningProgress.EnrollmentId))
                        .ToListAsync();

                    foreach (var quizAttempt in quizAttempts)
                    {
                        await _uow.QuizAttemptRepository.DeleteAsync(quizAttempt);
                    }
                }

                // 3.4 Delete PracticeAttemptTasks (child of PracticeAttempts)
                if (enrollmentIds.Any())
                {
                    var practiceAttemptTasks = await _uow.PracticeAttemptTaskRepository
                        .GetAllAsQueryable()
                        .Where(pat => enrollmentIds.Contains(pat.PracticeAttempt.ActivityRecord.SectionRecord.LearningProgress.EnrollmentId))
                        .ToListAsync();

                    foreach (var task in practiceAttemptTasks)
                    {
                        await _uow.PracticeAttemptTaskRepository.DeleteAsync(task);
                    }
                }

                // 3.5 Delete PracticeAttempts (child of ActivityRecords)
                if (enrollmentIds.Any())
                {
                    var practiceAttempts = await _uow.PracticeAttemptRepository
                        .GetAllAsQueryable()
                        .Where(pa => enrollmentIds.Contains(pa.ActivityRecord.SectionRecord.LearningProgress.EnrollmentId))
                        .ToListAsync();

                    foreach (var practiceAttempt in practiceAttempts)
                    {
                        await _uow.PracticeAttemptRepository.DeleteAsync(practiceAttempt);
                    }
                }

                // 3.6 Delete InstructorFeedbacks (child of ActivityRecords)
                if (enrollmentIds.Any())
                {
                    var instructorFeedbacks = await _uow.InstructorFeedbackRepository
                        .GetAllAsQueryable()
                        .Where(f => enrollmentIds.Contains(f.ActivityRecord.SectionRecord.LearningProgress.EnrollmentId))
                        .ToListAsync();

                    foreach (var feedback in instructorFeedbacks)
                    {
                        await _uow.InstructorFeedbackRepository.DeleteAsync(feedback);
                    }
                }

                // 3.7 Delete ActivityRecords (child of SectionRecords)
                if (enrollmentIds.Any())
                {
                    var activityRecords = await _uow.ActivityRecordRepository
                        .GetAllAsQueryable()
                        .Where(ar => enrollmentIds.Contains(ar.SectionRecord.LearningProgress.EnrollmentId))
                        .ToListAsync();

                    foreach (var activityRecord in activityRecords)
                    {
                        await _uow.ActivityRecordRepository.DeleteAsync(activityRecord);
                    }
                }

                // 3.8 Delete SectionRecords (child of LearningProgress)
                if (enrollmentIds.Any())
                {
                    var sectionRecords = await _uow.SectionRecordRepository
                        .GetAllAsQueryable()
                        .Where(sr => enrollmentIds.Contains(sr.LearningProgress.EnrollmentId))
                        .ToListAsync();

                    foreach (var sectionRecord in sectionRecords)
                    {
                        await _uow.SectionRecordRepository.DeleteAsync(sectionRecord);
                    }
                }

                // 3.9 Delete TraineeCertificates (child of Enrollments)
                if (enrollmentIds.Any())
                {
                    var traineeCertificates = await _uow.TraineeCertificateRepository
                        .GetAllAsQueryable()
                        .Where(tc => enrollmentIds.Contains(tc.EnrollmentId))
                        .ToListAsync();

                    foreach (var certificate in traineeCertificates)
                    {
                        await _uow.TraineeCertificateRepository.DeleteAsync(certificate);
                    }
                }

                // 3.10 Delete LearningProgress (child of Enrollments)
                if (enrollmentIds.Any())
                {
                    var learningProgresses = await _uow.LearningProgressRepository
                        .GetAllAsQueryable()
                        .Where(lp => enrollmentIds.Contains(lp.EnrollmentId))
                        .ToListAsync();

                    foreach (var progress in learningProgresses)
                    {
                        await _uow.LearningProgressRepository.DeleteAsync(progress);
                    }
                }

                // 3.11 Delete Enrollments
                foreach (var enrollment in enrollments)
                {
                    await _uow.EnrollmentRepository.DeleteAsync(enrollment);
                }

                // 3.12 Delete ClassInstructors
                var classInstructors = await _uow.ClassInstructorRepository
                    .GetAllAsQueryable()
                    .Where(ci => ci.ClassId == classId)
                    .ToListAsync();

                foreach (var instructor in classInstructors)
                {
                    await _uow.ClassInstructorRepository.DeleteAsync(instructor);
                }

                // 4. Finally, delete the Class itself
                await _uow.ClassRepository.DeleteAsync(classEntity);

                // 5. Save all changes
                await _uow.SaveChangesAsync();

                // 6. Commit transaction
                await transaction.CommitAsync();
            }
            catch (Exception)
            {
                // Rollback transaction if any error occurs
                await transaction.RollbackAsync();
                throw;
            }
        }
        #endregion

        #region Classes By other Filters

        public async Task<IEnumerable<ClassDto>> GetClassesByProgramAndCourseAsync(int programId, int courseId)
        {
            var classes = await _uow.ClassRepository
                .GetAllAsQueryable()
                .Include(c => c.ProgramCourse)
                .Include(c => c.ClassCode)
                .Where(c => c.ProgramCourse.ProgramId == programId && c.ProgramCourse.CourseId == courseId)
                .ToListAsync();

            return classes.Select(MapToDto);
        }

        public async Task<IEnumerable<ClassDto>> GetClassesByCourseAsync(int courseId)
        {
            var classes = await _uow.ClassRepository
                .GetAllAsQueryable()
                .Include(c => c.ProgramCourse)
                .Include(c => c.ClassCode)
                .Where(c => c.ProgramCourse.CourseId == courseId)
                .ToListAsync();

            return classes.Select(MapToDto);
        }

        public async Task<IEnumerable<ClassDto>> GetClassesByCourseIdForTrainee(int courseId)
        {
            var classes = await _uow.ClassRepository
                .GetAllAsQueryable()
                .Include(c => c.ProgramCourse)
                .Include(c => c.ClassCode)
                .Where(c => c.ProgramCourse.CourseId == courseId &&
                           (c.Status == (int)ClassStatusEnum.Open || c.Status == (int)ClassStatusEnum.Inprogress))
                .ToListAsync();

            return classes.Select(MapToDto);
        }

        public async Task<IEnumerable<ClassDto>> GetClassesByInstructorAsync(int instructorId)
        {
            var classes = await _uow.ClassInstructorRepository
                .GetAllAsQueryable()
                .Where(ci => ci.InstructorId == instructorId)
                .Include(ci => ci.Class)
                    .ThenInclude(c => c.ProgramCourse)
                .Include(ci => ci.Class)
                    .ThenInclude(c => c.ClassCode)
                .Select(ci => ci.Class)
                .ToListAsync();

            return classes.Select(MapToDto);
        }


        #endregion

        #region Classes By Trainee

        public async Task<IEnumerable<ClassDto>> GetAllClassesByTraineeAsync(int traineeId)
        {
            var classes = await _uow.EnrollmentRepository
                .GetAllAsQueryable()
                .Where(e => e.TraineeId == traineeId &&
                            (e.Status == (int)EnrollmentStatusEnum.Enrolled ||
                             e.Status == (int)EnrollmentStatusEnum.Inprogress ||
                             e.Status == (int)EnrollmentStatusEnum.Completed))
                .Include(e => e.Class)
                    .ThenInclude(c => c.ProgramCourse)
                        .ThenInclude(pc => pc.Course)
                .Include(e => e.Class)
                    .ThenInclude(c => c.ClassCode)
                .Select(e => e.Class)
                .Distinct()
                .ToListAsync();

            return classes.Select(MapToDto);
        }

        public async Task<PagedResult<ClassDto>> GetPagedClassesByTraineeAsync(int traineeId, int pageNumber, int pageSize)
        {
            if (pageNumber < 1) pageNumber = 1;
            if (pageSize < 1) pageSize = 10;

            var query = _uow.EnrollmentRepository
                .GetAllAsQueryable()
                .Where(e => e.TraineeId == traineeId &&
                            (e.Status == (int)EnrollmentStatusEnum.Enrolled ||
                             e.Status == (int)EnrollmentStatusEnum.Inprogress ||
                             e.Status == (int)EnrollmentStatusEnum.Completed))
                .Include(e => e.Class)
                    .ThenInclude(c => c.ProgramCourse)
                        .ThenInclude(pc => pc.Course)
                .Include(e => e.Class)
                    .ThenInclude(c => c.ClassCode)
                .Select(e => e.Class)
                .Distinct();

            var pagedEntities = await query.ToPagedResultAsync(pageNumber, pageSize);

            var dtoItems = pagedEntities.Items.Select(MapToDto).ToList();

            return new PagedResult<ClassDto>
            {
                Items = dtoItems,
                TotalCount = pagedEntities.TotalCount,
                Page = pagedEntities.Page,
                PageSize = pagedEntities.PageSize
            };
        }

        public async Task<ClassDto?> GetClassByIdAndTraineeAsync(int classId, int traineeId)
        {
            var enrollment = await _uow.EnrollmentRepository
                .GetAllAsQueryable()
                .Where(e => e.ClassId == classId &&
                            e.TraineeId == traineeId &&
                            (e.Status == (int)EnrollmentStatusEnum.Enrolled ||
                             e.Status == (int)EnrollmentStatusEnum.Inprogress ||
                             e.Status == (int)EnrollmentStatusEnum.Completed))
                .Include(e => e.Class)
                    .ThenInclude(c => c.ProgramCourse)
                        .ThenInclude(pc => pc.Course)
                .Include(e => e.Class)
                    .ThenInclude(c => c.ClassCode)
                .FirstOrDefaultAsync();
            return enrollment == null ? null : MapToDto(enrollment.Class);
        }

        #endregion

        #region Mapping

        private static ClassDto MapToDto(Class c)
        {
            string classStatus = c.Status.HasValue
                ? Enum.GetName(typeof(ClassStatusEnum), c.Status.Value) ?? "Cancelled"
                : "Cancelled";

            return new ClassDto
            {
                Id = c.Id,
                Name = c.Name,
                Capacity = c.Capacity,
                ClassCode = c.ClassCode?.Name ?? "CLS099",
                ProgramId = c.ProgramCourse.ProgramId,
                CourseId = c.ProgramCourse.CourseId,
                Description = c.Description,
                StartDate = c.StartDate,
                EndDate = c.EndDate,
                Status = classStatus,
                DurationHours = c.ProgramCourse.Course?.DurationHours
            };
        }

        #endregion

        #region Import Trainees

        public async Task<string> ImportTraineesToClassAsync(int classId, IFormFile file)
        {
            if (file == null || file.Length == 0)
                throw new Exception("File is empty or null.");

            if (!file.FileName.EndsWith(".xlsx", StringComparison.OrdinalIgnoreCase))
                throw new Exception("Invalid file format. Please upload an Excel file (.xlsx).");

            // Verify that the class exists and get class details for email
            var targetClass = await _uow.ClassRepository
                .GetAllAsQueryable()
                .Include(c => c.Enrollments)
                .Include(c => c.ClassCode)
                .FirstOrDefaultAsync(c => c.Id == classId);

            if (targetClass == null)
                throw new KeyNotFoundException($"Class with ID {classId} not found.");

            int createdUsersCount = 0;
            int enrolledCount = 0;
            int skippedCount = 0;
            int totalRows = 0;
            var errors = new List<string>();

            try
            {
                ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

                using (var stream = new MemoryStream())
                {
                    await file.CopyToAsync(stream);
                    stream.Position = 0;

                    using (var package = new ExcelPackage(stream))
                    {
                        var worksheet = package.Workbook.Worksheets.FirstOrDefault();
                        if (worksheet == null)
                            throw new Exception("Excel file does not contain any worksheets.");

                        int rowCount = worksheet.Dimension?.Rows ?? 0;
                        totalRows = rowCount - 1; // Exclude header row
                        
                        if (rowCount < 2)
                            throw new Exception("Excel file must contain at least one data row (plus header row).");

                        // Start a transaction for batch import
                        var dbContext = _uow.GetDbContext();
                        using (var transaction = await dbContext.Database.BeginTransactionAsync(IsolationLevel.ReadCommitted))
                        {
                            try
                            {
                                // Process each row (skip header row 1)
                                for (int row = 2; row <= rowCount; row++)
                                {
                                    try
                                    {
                                        // Read values from Excel
                                        string? username = worksheet.Cells[row, 1].Value?.ToString()?.Trim();
                                        string? email = worksheet.Cells[row, 2].Value?.ToString()?.Trim();
                                        string? fullname = worksheet.Cells[row, 3].Value?.ToString()?.Trim();
                                        string? password = worksheet.Cells[row, 4].Value?.ToString()?.Trim();
                                        string? phoneNumber = worksheet.Cells[row, 5].Value?.ToString()?.Trim();
                                        string? avatarUrl = worksheet.Cells[row, 6].Value?.ToString()?.Trim();

                                        // Skip completely empty rows
                                        if (string.IsNullOrWhiteSpace(username) && 
                                            string.IsNullOrWhiteSpace(email) && 
                                            string.IsNullOrWhiteSpace(fullname))
                                        {
                                            continue;
                                        }

                                        // Validate required fields
                                        if (string.IsNullOrWhiteSpace(username) || 
                                            string.IsNullOrWhiteSpace(email) || 
                                            string.IsNullOrWhiteSpace(fullname) || 
                                            string.IsNullOrWhiteSpace(password))
                                        {
                                            errors.Add($"Row {row}: Missing required fields (Username, Email, Fullname, or Password).");
                                            skippedCount++;
                                            continue;
                                        }

                                        // Step 1: Find or Create User
                                        var existingUser = await _uow.UserRepository
                                            .GetAllAsQueryable()
                                            .FirstOrDefaultAsync(u => 
                                                (u.Username == username || u.Email.ToLower() == email.ToLower()) 
                                                && !u.IsDeleted);

                                        int userId;
                                        bool userCreated = false;
                                        string traineeFullname = fullname;
                                        string traineeEmail = email;

                                        if (existingUser != null)
                                        {
                                            // Use existing user
                                            userId = existingUser.Id;
                                            traineeFullname = existingUser.Fullname;
                                            traineeEmail = existingUser.Email;
                                        }
                                        else
                                        {
                                            // Create new trainee user
                                            string hashedPassword = PasswordHashHandler.HashPassword(password);
                                            string traineeCode = await GenerateUniqueTraineeCode();

                                            var traineeProfile = new TraineeProfile { };

                                            var trainee = new Trainee
                                            {
                                                TraineeCode = traineeCode,
                                                IsActive = true,
                                                IsDeleted = false,
                                                TraineeProfile = traineeProfile
                                            };

                                            var user = new User
                                            {
                                                Username = username,
                                                Password = hashedPassword,
                                                Email = email,
                                                Fullname = fullname,
                                                PhoneNumber = string.IsNullOrWhiteSpace(phoneNumber) ? null : phoneNumber,
                                                AvatarUrl = string.IsNullOrWhiteSpace(avatarUrl) ? null : avatarUrl,
                                                Role = (int)UserRoleEnum.Trainee,
                                                IsActive = true,
                                                IsDeleted = false,
                                                Trainee = trainee
                                            };

                                            await _uow.UserRepository.CreateAsync(user);
                                            await _uow.SaveChangesAsync(); // Save to get the UserId

                                            userId = user.Id;
                                            createdUsersCount++;
                                            userCreated = true;
                                        }

                                        // Step 2: Enroll in Class (if not already enrolled)
                                        // Check if enrollment already exists for this user and class
                                        var existingEnrollment = await _uow.EnrollmentRepository
                                            .GetAllAsQueryable()
                                            .FirstOrDefaultAsync(e => e.ClassId == classId && e.TraineeId == userId);

                                        if (existingEnrollment != null)
                                        {
                                            // User is already enrolled in this class - SKIP silently
                                            skippedCount++;
                                            if (userCreated)
                                            {
                                                // Note: User was created but already enrolled in another session or this is a duplicate in the file
                                                errors.Add($"Row {row}: User '{username}' was created but is already enrolled in this class.");
                                            }
                                            continue;
                                        }

                                        // Check class capacity
                                        if (targetClass.Capacity.HasValue)
                                        {
                                            int currentEnrolled = await _uow.EnrollmentRepository
                                                .GetAllAsQueryable()
                                                .CountAsync(e => e.ClassId == classId && 
                                                    (e.Status == (int)EnrollmentStatusEnum.Enrolled || 
                                                     e.Status == (int)EnrollmentStatusEnum.Inprogress ||
                                                     e.Status == (int)EnrollmentStatusEnum.Pending));

                                            if (currentEnrolled >= targetClass.Capacity.Value)
                                            {
                                                errors.Add($"Row {row}: Class is full. Cannot enroll user '{username}'.");
                                                skippedCount++;
                                                continue;
                                            }
                                        }

                                        // Create new enrollment with Status = Enrolled
                                        var newEnrollment = new Enrollment
                                        {
                                            ClassId = classId,
                                            TraineeId = userId,
                                            EnrollDate = DateTime.UtcNow,
                                            Status = (int)EnrollmentStatusEnum.Enrolled,
                                            IsActive = true,
                                            IsDeleted = false
                                        };

                                        await _uow.EnrollmentRepository.CreateAsync(newEnrollment);
                                        enrolledCount++;

                                        // Step 3: Send Email Notification
                                        try
                                        {
                                            string classCode = targetClass.ClassCode?.Name ?? "N/A";
                                            string startDate = targetClass.StartDate.ToString("dd/MM/yyyy");
                                            string endDate = targetClass.EndDate.HasValue 
                                                ? targetClass.EndDate.Value.ToString("dd/MM/yyyy") 
                                                : "TBD";

                                            string emailSubject = $"🎓 Enrollment Confirmation - {targetClass.Name}";
                                            
                                            string emailBody = $@"
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
            background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); 
            color: #ffffff; 
            padding: 30px 20px; 
            text-align: center; 
        }}
        .email-header h1 {{ 
            margin: 0; 
            font-size: 28px; 
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
        .class-info {{ 
            background-color: #f8f9fa; 
            border-left: 4px solid #667eea; 
            padding: 20px; 
            margin: 20px 0; 
            border-radius: 5px; 
        }}
        .class-info-title {{ 
            font-size: 20px; 
            font-weight: 600; 
            color: #667eea; 
            margin-bottom: 15px; 
        }}
        .info-row {{ 
            display: flex; 
            padding: 8px 0; 
            border-bottom: 1px solid #e0e0e0; 
        }}
        .info-row:last-child {{ 
            border-bottom: none; 
        }}
        .info-label {{ 
            font-weight: 600; 
            color: #666; 
            min-width: 120px; 
        }}
        .info-value {{ 
            color: #333; 
            font-weight: 500; 
        }}
        .cta-button {{ 
            display: inline-block; 
            background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); 
            color: #ffffff; 
            padding: 14px 30px; 
            text-decoration: none; 
            border-radius: 5px; 
            font-weight: 600; 
            font-size: 16px; 
            margin: 20px 0; 
            text-align: center; 
        }}
        .cta-button:hover {{ 
            opacity: 0.9; 
        }}
        .divider {{ 
            height: 1px; 
            background-color: #e0e0e0; 
            margin: 25px 0; 
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
            color: #667eea; 
            font-weight: 600; 
        }}
        @media only screen and (max-width: 600px) {{ 
            .email-container {{ 
                margin: 10px; 
                border-radius: 5px; 
            }}
            .email-header {{ 
                padding: 20px 15px; 
            }}
            .email-header h1 {{ 
                font-size: 22px; 
            }}
            .email-body {{ 
                padding: 20px 15px; 
            }}
            .info-row {{ 
                flex-direction: column; 
            }}
            .info-label {{ 
                margin-bottom: 5px; 
            }}
        }}
    </style>
</head>
<body>
    <div class='email-container'>
        <div class='email-header'>
            <div class='icon'>🎓</div>
            <h1>Enrollment Confirmed!</h1>
        </div>
        
        <div class='email-body'>
            <div class='greeting'>
                Dear <span class='highlight'>{traineeFullname}</span>,
            </div>
            
            <div class='message'>
                Congratulations! We are delighted to inform you that you have been <strong>successfully enrolled</strong> in the training class. We look forward to supporting you on your learning journey.
            </div>
            
            <div class='class-info'>
                <div class='class-info-title'>📋 Class Information</div>
                <div class='info-row'>
                    <span class='info-label'>Class Name:</span>
                    <span class='info-value'>{targetClass.Name}</span>
                </div>
                <div class='info-row'>
                    <span class='info-label'>Class Code:</span>
                    <span class='info-value'>{classCode}</span>
                </div>
                <div class='info-row'>
                    <span class='info-label'>Start Date:</span>
                    <span class='info-value'>{startDate}</span>
                </div>
                <div class='info-row'>
                    <span class='info-label'>End Date:</span>
                    <span class='info-value'>{endDate}</span>
                </div>
            </div>
            
            <div style='text-align: center;'>
                <a href='#' class='cta-button'>View My Schedule</a>
            </div>
            
            <div class='divider'></div>
            
            <div class='message'>
                <strong>Next Steps:</strong>
                <ul style='margin-top: 10px; padding-left: 20px;'>
                    <li>Log in to the training management system</li>
                    <li>Review your class schedule and materials</li>
                    <li>Prepare any required documents or prerequisites</li>
                    <li>Contact your instructor if you have any questions</li>
                </ul>
            </div>
            
            <div class='message' style='margin-top: 20px;'>
                If you have any questions or concerns, please don't hesitate to reach out to our support team.
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

                                            await _mailService.SendEmailAsync(traineeEmail, emailSubject, emailBody);
                                        }
                                        catch (Exception emailEx)
                                        {
                                            // Log the email error but don't stop the import process
                                            errors.Add($"Row {row}: User '{username}' enrolled successfully, but failed to send email notification: {emailEx.Message}");
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        errors.Add($"Row {row}: {ex.Message}");
                                        skippedCount++;
                                    }
                                }

                                // Save all changes within transaction
                                await _uow.SaveChangesAsync();
                                await transaction.CommitAsync();
                            }
                            catch (Exception)
                            {
                                await transaction.RollbackAsync();
                                throw;
                            }
                        }
                    }
                }

                // Build result message
                var resultMessage = $"Import completed successfully. Processed: {totalRows} rows. Created Users: {createdUsersCount}. Enrolled in Class: {enrolledCount}. Skipped (Already Enrolled or Errors): {skippedCount}.";
                
                if (errors.Any())
                {
                    resultMessage += $" Errors: {string.Join("; ", errors.Take(10))}";
                    if (errors.Count > 10)
                    {
                        resultMessage += $" (and {errors.Count - 10} more errors...)";
                    }
                }

                return resultMessage;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error processing Excel file: {ex.Message}");
            }
        }

        private async Task<string> GenerateUniqueTraineeCode()
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            string traineeCode;
            bool isUnique;
            do
            {
                var randomPart = new string(Enumerable.Repeat(chars, 6)
                    .Select(s => s[_random.Next(s.Length)]).ToArray());

                traineeCode = "CS" + randomPart;

                isUnique = !await _uow.TraineeRepository
                    .GetAllAsQueryable()
                    .AnyAsync(t => t.TraineeCode == traineeCode);

            } while (!isUnique);
            return traineeCode;
        }

        #endregion
    }
}
