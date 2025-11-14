using Lssctc.ProgramManagement.ClassManage.Helpers;
using Lssctc.ProgramManagement.ClassManage.PracticeAttempts.Dtos;
using Lssctc.Share.Common;
using Lssctc.Share.Entities;
using Lssctc.Share.Enums;
using Lssctc.Share.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Lssctc.ProgramManagement.ClassManage.PracticeAttempts.Services
{
    public class PracticeAttemptsService : IPracticeAttemptsService
    {
        private readonly IUnitOfWork _uow;
        private readonly ProgressHelper _progressHelper;

        public PracticeAttemptsService(IUnitOfWork uow)
        {
            _uow = uow;
            _progressHelper = new ProgressHelper(uow);
        }

        public async Task<IEnumerable<PracticeAttemptDto>> GetPracticeAttempts(int traineeId, int activityRecordId)
        {
            var activityRecord = await _uow.ActivityRecordRepository
                .GetAllAsQueryable()
                .Include(ar => ar.SectionRecord.LearningProgress.Enrollment)
                .AsNoTracking()
                .FirstOrDefaultAsync(ar => ar.Id == activityRecordId);

            if (activityRecord == null)
                throw new KeyNotFoundException("Activity record not found.");

            // Verify trainee ownership
            if (activityRecord.SectionRecord.LearningProgress.Enrollment.TraineeId != traineeId)
                throw new KeyNotFoundException("Activity record not found for this trainee.");

            var attempts = await _uow.PracticeAttemptRepository
                .GetAllAsQueryable()
                .AsNoTracking()
                .Include(pa => pa.PracticeAttemptTasks)
                .Where(pa => pa.ActivityRecordId == activityRecordId)
                .OrderByDescending(pa => pa.AttemptDate)
                .ToListAsync();

            return attempts.Select(MapToDto);
        }

        public async Task<PracticeAttemptDto?> GetLatestPracticeAttempt(int traineeId, int activityRecordId)
        {
            var activityRecord = await _uow.ActivityRecordRepository
                .GetAllAsQueryable()
                .Include(ar => ar.SectionRecord.LearningProgress.Enrollment)
                .AsNoTracking()
                .FirstOrDefaultAsync(ar => ar.Id == activityRecordId);

            if (activityRecord == null)
                throw new KeyNotFoundException("Activity record not found.");

            // Verify trainee ownership
            if (activityRecord.SectionRecord.LearningProgress.Enrollment.TraineeId != traineeId)
                throw new KeyNotFoundException("Activity record not found for this trainee.");

            var attempt = await _uow.PracticeAttemptRepository
                .GetAllAsQueryable()
                .AsNoTracking()
                .Include(pa => pa.PracticeAttemptTasks)
                .Where(pa => pa.ActivityRecordId == activityRecordId && pa.IsCurrent)
                .FirstOrDefaultAsync();

            return attempt == null ? null : MapToDto(attempt);
        }

        public async Task<IEnumerable<PracticeAttemptDto>> GetPracticeAttemptsByPractice(int traineeId, int practiceId)
        {
            // Get all activity records for this trainee
            var activityRecordIds = await _uow.ActivityRecordRepository
                .GetAllAsQueryable()
                .Include(ar => ar.SectionRecord.LearningProgress.Enrollment)
                .Where(ar => ar.SectionRecord.LearningProgress.Enrollment.TraineeId == traineeId)
                .Select(ar => ar.Id)
                .ToListAsync();

            if (!activityRecordIds.Any())
                return Enumerable.Empty<PracticeAttemptDto>();

            // Get all practice attempts for this practice from trainee's activity records
            var attempts = await _uow.PracticeAttemptRepository
                .GetAllAsQueryable()
                .AsNoTracking()
                .Include(pa => pa.PracticeAttemptTasks)
                .Where(pa => activityRecordIds.Contains(pa.ActivityRecordId) 
                          && pa.PracticeId == practiceId
                          && (pa.IsDeleted == null || pa.IsDeleted == false))
                .OrderByDescending(pa => pa.AttemptDate)
                .ToListAsync();

            return attempts.Select(MapToDto);
        }

        public async Task<PracticeAttemptDto?> GetPracticeAttemptById(int practiceAttemptId)
        {
            var attempt = await _uow.PracticeAttemptRepository
                .GetAllAsQueryable()
                .AsNoTracking()
                .Include(pa => pa.PracticeAttemptTasks)
                .Include(pa => pa.ActivityRecord)
                    .ThenInclude(ar => ar.SectionRecord)
                        .ThenInclude(sr => sr.LearningProgress)
                            .ThenInclude(lp => lp.Enrollment)
                .Where(pa => pa.Id == practiceAttemptId 
                          && (pa.IsDeleted == null || pa.IsDeleted == false))
                .FirstOrDefaultAsync();

            return attempt == null ? null : MapToDto(attempt);
        }

        public async Task<PagedResult<PracticeAttemptDto>> GetPracticeAttemptsPaged(int traineeId, int activityRecordId, int pageNumber, int pageSize)
        {
            if (pageNumber < 1) pageNumber = 1;
            if (pageSize < 1) pageSize = 10;

            var activityRecord = await _uow.ActivityRecordRepository
                .GetAllAsQueryable()
                .Include(ar => ar.SectionRecord.LearningProgress.Enrollment)
                .AsNoTracking()
                .FirstOrDefaultAsync(ar => ar.Id == activityRecordId);

            if (activityRecord == null)
                throw new KeyNotFoundException("Activity record not found.");

            // Verify trainee ownership
            if (activityRecord.SectionRecord.LearningProgress.Enrollment.TraineeId != traineeId)
                throw new KeyNotFoundException("Activity record not found for this trainee.");

            var query = _uow.PracticeAttemptRepository
                .GetAllAsQueryable()
                .AsNoTracking()
                .Include(pa => pa.PracticeAttemptTasks)
                .Where(pa => pa.ActivityRecordId == activityRecordId)
                .OrderByDescending(pa => pa.AttemptDate);

            return await query.Select(pa => MapToDto(pa)).ToPagedResultAsync(pageNumber, pageSize);
        }

        public async Task<PagedResult<PracticeAttemptDto>> GetPracticeAttemptsByPracticePaged(int traineeId, int practiceId, int pageNumber, int pageSize)
        {
            if (pageNumber < 1) pageNumber = 1;
            if (pageSize < 1) pageSize = 10;

            // Get all activity records for this trainee
            var activityRecordIds = await _uow.ActivityRecordRepository
                .GetAllAsQueryable()
                .Include(ar => ar.SectionRecord.LearningProgress.Enrollment)
                .Where(ar => ar.SectionRecord.LearningProgress.Enrollment.TraineeId == traineeId)
                .Select(ar => ar.Id)
                .ToListAsync();

            if (!activityRecordIds.Any())
            {
                return new PagedResult<PracticeAttemptDto>
                {
                    Items = Enumerable.Empty<PracticeAttemptDto>(),
                    TotalCount = 0,
                    Page = pageNumber,
                    PageSize = pageSize
                };
            }

            var query = _uow.PracticeAttemptRepository
                .GetAllAsQueryable()
                .AsNoTracking()
                .Include(pa => pa.PracticeAttemptTasks)
                .Where(pa => activityRecordIds.Contains(pa.ActivityRecordId) 
                          && pa.PracticeId == practiceId
                          && (pa.IsDeleted == null || pa.IsDeleted == false))
                .OrderByDescending(pa => pa.AttemptDate);

            return await query.Select(pa => MapToDto(pa)).ToPagedResultAsync(pageNumber, pageSize);
        }

        public async Task<PracticeAttemptDto> CreatePracticeAttempt(int traineeId, CreatePracticeAttemptDto createDto)
        {
            // 0. Validate input IDs
            if (traineeId <= 0)
                throw new ArgumentException("TraineeId must be greater than 0.", nameof(traineeId));

            if (createDto.ClassId <= 0)
                throw new ArgumentException("ClassId must be greater than 0.", nameof(createDto.ClassId));

            if (createDto.PracticeId <= 0)
                throw new ArgumentException("PracticeId must be greater than 0.", nameof(createDto.PracticeId));

            // Validate that trainee exists
            var traineeExists = await _uow.TraineeRepository.ExistsAsync(t => t.Id == traineeId && (t.IsDeleted == null || t.IsDeleted == false));
            if (!traineeExists)
                throw new KeyNotFoundException($"Trainee with ID {traineeId} not found.");

            // Validate that class exists
            var classEntity = await _uow.ClassRepository.GetByIdAsync(createDto.ClassId);
            if (classEntity == null)
                throw new KeyNotFoundException($"Class with ID {createDto.ClassId} not found.");

            // -----------------------------------------------------------------
            // MODIFICATION 1: Get Practice Template Tasks
            // -----------------------------------------------------------------
            var practiceTemplate = await _uow.PracticeRepository
                .GetAllAsQueryable()
                .AsNoTracking()
                .Include(p => p.PracticeTasks)
                .FirstOrDefaultAsync(p => p.Id == createDto.PracticeId && (p.IsDeleted == null || p.IsDeleted == false));

            if (practiceTemplate == null)
                throw new KeyNotFoundException($"Practice with ID {createDto.PracticeId} not found.");

            // Get the set of *required* Task IDs from the template
            var templateTaskIds = practiceTemplate.PracticeTasks
                .Select(pt => pt.TaskId)
                .ToHashSet();

            // Validate enrollment exists
            var enrollmentExists = await _uow.EnrollmentRepository.ExistsAsync(e =>
                e.TraineeId == traineeId &&
                e.ClassId == createDto.ClassId &&
                (e.IsDeleted == null || e.IsDeleted == false));
            if (!enrollmentExists)
                throw new KeyNotFoundException($"Trainee {traineeId} is not enrolled in Class {createDto.ClassId}.");

            // -----------------------------------------------------------------
            // MODIFICATION 2: Validate submitted tasks and re-calculate pass/fail
            // -----------------------------------------------------------------
            bool allTasksPass = false;
            var submittedPassedTaskIds = new HashSet<int>();

            if (createDto.PracticeAttemptTasks != null && createDto.PracticeAttemptTasks.Any())
            {
                foreach (var taskDto in createDto.PracticeAttemptTasks)
                {
                    // Check 1: Does the submitted TaskId even exist in the template?
                    if (taskDto.TaskId.HasValue && templateTaskIds.Contains(taskDto.TaskId.Value))
                    {
                        // Check 2: Did the client say this task passed?
                        if (taskDto.IsPass == true)
                        {
                            submittedPassedTaskIds.Add(taskDto.TaskId.Value);
                        }
                    }
                    else if (taskDto.TaskId.HasValue)
                    {
                        // Submitted a task that isn't part of this practice's template
                        throw new ArgumentException($"Submitted TaskId {taskDto.TaskId.Value} is not part of Practice {createDto.PracticeId}.");
                    }
                }

                // Check 3: Does the set of passed tasks *exactly match* the set of template tasks?
                // This ensures all required tasks were submitted and all were marked as passed.
                allTasksPass = templateTaskIds.SetEquals(submittedPassedTaskIds);
            }

            // Overwrite client-sent score and pass status with server-validated values
            createDto.Score = allTasksPass ? 10 : 0;
            createDto.IsPass = allTasksPass;

            // 1. Find ActivityRecord based on TraineeId, ClassId, and PracticeId
            var activityRecord = await _uow.ActivityRecordRepository
                .GetAllAsQueryable()
                // We need to track this entity to update it
                .Include(ar => ar.SectionRecord)
                    .ThenInclude(sr => sr.LearningProgress)
                        .ThenInclude(lp => lp.Enrollment)
                .FirstOrDefaultAsync(ar =>
                    ar.SectionRecord.LearningProgress.Enrollment.TraineeId == traineeId &&
                    ar.SectionRecord.LearningProgress.Enrollment.ClassId == createDto.ClassId &&
                    ar.ActivityType == (int)ActivityType.Practice && // <-- Ensure it's a practice AR
                    ar.ActivityId != null &&
                    ar.ActivityId == (
                        _uow.ActivityPracticeRepository.GetAllAsQueryable()
                            .Where(ap => ap.PracticeId == createDto.PracticeId)
                            .Select(ap => ap.ActivityId)
                            .FirstOrDefault()
                    ));

            if (activityRecord == null)
            {
                throw new KeyNotFoundException(
                    $"Activity record not found for TraineeId={traineeId}, ClassId={createDto.ClassId}, PracticeId={createDto.PracticeId}. " +
                    "Please ensure the practice is assigned to an activity in this class.");
            }

            // 2. Set IsCurrent = false for all existing attempts for this activity record
            var existingAttempts = await _uow.PracticeAttemptRepository
                .GetAllAsQueryable()
                .Where(pa => pa.ActivityRecordId == activityRecord.Id && pa.IsCurrent)
                .ToListAsync();

            foreach (var attempt in existingAttempts)
            {
                attempt.IsCurrent = false;
                await _uow.PracticeAttemptRepository.UpdateAsync(attempt);
            }

            // 3. Create new PracticeAttempt
            var practiceAttempt = new PracticeAttempt
            {
                ActivityRecordId = activityRecord.Id,
                PracticeId = createDto.PracticeId,
                Score = createDto.Score, // Use server-validated score
                AttemptDate = DateTime.Now,
                AttemptStatus = (int)ActivityRecordStatusEnum.InProgress,
                Description = createDto.Description,
                IsPass = createDto.IsPass, // Use server-validated pass status
                IsCurrent = true,
                IsDeleted = false
            };

            await _uow.PracticeAttemptRepository.CreateAsync(practiceAttempt);
            await _uow.SaveChangesAsync(); // Save to get practiceAttempt.Id

            // 4. Create PracticeAttemptTasks
            var practiceAttemptTasks = new List<PracticeAttemptTask>();
            if (createDto.PracticeAttemptTasks != null && createDto.PracticeAttemptTasks.Any())
            {
                foreach (var taskDto in createDto.PracticeAttemptTasks)
                {
                    // Only save tasks that are part of the template
                    if (taskDto.TaskId.HasValue && templateTaskIds.Contains(taskDto.TaskId.Value))
                    {
                        var practiceAttemptTask = new PracticeAttemptTask
                        {
                            PracticeAttemptId = practiceAttempt.Id,
                            TaskId = taskDto.TaskId,
                            Score = taskDto.Score,
                            Description = taskDto.Description,
                            IsPass = taskDto.IsPass,
                            IsDeleted = false
                        };

                        practiceAttemptTasks.Add(practiceAttemptTask);
                        await _uow.PracticeAttemptTaskRepository.CreateAsync(practiceAttemptTask);
                    }
                }
            }

            // Save the tasks
            await _uow.SaveChangesAsync();

            // -----------------------------------------------------------------
            // MODIFICATION 3: Call ProgressHelper to update all statuses
            // -----------------------------------------------------------------

            // 5. Update ActivityRecord explicitly using the helper
            // This will set its status to Completed or InProgress based on the new attempt
            await _progressHelper.UpdateActivityRecordProgressAsync(traineeId, activityRecord.Id);

            // 6. Propagate changes up to SectionRecord
            await _progressHelper.UpdateSectionRecordProgressAsync(traineeId, activityRecord.SectionRecordId);

            // 7. Propagate changes up to LearningProgress
            await _progressHelper.UpdateLearningProgressProgressAsync(traineeId, activityRecord.SectionRecord.LearningProgressId);

            // 8. Return DTO
            practiceAttempt.PracticeAttemptTasks = practiceAttemptTasks;
            return MapToDto(practiceAttempt);
        }
        #region Mapping Methods

        private static PracticeAttemptDto MapToDto(PracticeAttempt pa)
        {
            return new PracticeAttemptDto
            {
                Id = pa.Id,
                ActivityRecordId = pa.ActivityRecordId,
                PracticeId = pa.PracticeId,
                Score = pa.Score,
                AttemptDate = pa.AttemptDate,
                AttemptStatus = pa.AttemptStatus?.ToString() ?? "Unknown",
                Description = pa.Description,
                IsPass = pa.IsPass,
                IsCurrent = pa.IsCurrent,
                PracticeAttemptTasks = pa.PracticeAttemptTasks.Select(MapToTaskDto).ToList()
            };
        }

        private static PracticeAttemptTaskDto MapToTaskDto(PracticeAttemptTask pat)
        {
            return new PracticeAttemptTaskDto
            {
                Id = pat.Id,
                PracticeAttemptId = pat.PracticeAttemptId,
                TaskId = pat.TaskId,
                Score = pat.Score,
                Description = pat.Description,
                IsPass = pat.IsPass
            };
        }

        #endregion
    }
}
