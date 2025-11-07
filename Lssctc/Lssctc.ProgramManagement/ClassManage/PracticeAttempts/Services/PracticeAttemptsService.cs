using Lssctc.Share.Interfaces;
using Lssctc.ProgramManagement.ClassManage.PracticeAttempts.Dtos;
using Lssctc.Share.Common;
using Microsoft.EntityFrameworkCore;
using Lssctc.Share.Entities;

namespace Lssctc.ProgramManagement.ClassManage.PracticeAttempts.Services
{
    public class PracticeAttemptsService : IPracticeAttemptsService
    {
        private readonly IUnitOfWork _uow;
        
        public PracticeAttemptsService(IUnitOfWork uow)
        {
            _uow = uow;
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

        public async Task<PracticeAttemptDto> CreatePracticeAttempt(CreatePracticeAttemptDto createDto)
        {
            // 0. Validate input IDs
            if (createDto.TraineeId <= 0)
                throw new ArgumentException("TraineeId must be greater than 0.", nameof(createDto.TraineeId));

            if (createDto.ClassId <= 0)
                throw new ArgumentException("ClassId must be greater than 0.", nameof(createDto.ClassId));

            if (createDto.PracticeId <= 0)
                throw new ArgumentException("PracticeId must be greater than 0.", nameof(createDto.PracticeId));

            // Validate that trainee exists
            var traineeExists = await _uow.TraineeRepository.ExistsAsync(t => t.Id == createDto.TraineeId && (t.IsDeleted == null || t.IsDeleted == false));
            if (!traineeExists)
                throw new KeyNotFoundException($"Trainee with ID {createDto.TraineeId} not found.");

            // Validate that class exists
            var classEntity = await _uow.ClassRepository.GetByIdAsync(createDto.ClassId);
            if (classEntity == null)
                throw new KeyNotFoundException($"Class with ID {createDto.ClassId} not found.");

            // Validate that practice exists
            var practiceExists = await _uow.PracticeRepository.ExistsAsync(p => p.Id == createDto.PracticeId && (p.IsDeleted == null || p.IsDeleted == false));
            if (!practiceExists)
                throw new KeyNotFoundException($"Practice with ID {createDto.PracticeId} not found.");

            // Validate enrollment exists
            var enrollmentExists = await _uow.EnrollmentRepository.ExistsAsync(e => 
                e.TraineeId == createDto.TraineeId && 
                e.ClassId == createDto.ClassId && 
                (e.IsDeleted == null || e.IsDeleted == false));
            if (!enrollmentExists)
                throw new KeyNotFoundException($"Trainee {createDto.TraineeId} is not enrolled in Class {createDto.ClassId}.");

            // Validate TaskIds if provided and calculate score based on pass status
            bool allTasksPass = false;
            if (createDto.PracticeAttemptTasks != null && createDto.PracticeAttemptTasks.Any())
            {
                foreach (var taskDto in createDto.PracticeAttemptTasks)
                {
                    if (taskDto.TaskId.HasValue && taskDto.TaskId.Value > 0)
                    {
                        var taskExists = await _uow.SimTaskRepository.ExistsAsync(t => 
                            t.Id == taskDto.TaskId.Value && 
                            (t.IsDeleted == null || t.IsDeleted == false));
                        if (!taskExists)
                            throw new KeyNotFoundException($"Task with ID {taskDto.TaskId.Value} not found.");
                    }
                }

                // Check if all tasks have IsPass = true
                allTasksPass = createDto.PracticeAttemptTasks.All(t => t.IsPass == true);

                // Calculate score: 10 if all tasks pass, 0 if any task fails
                createDto.Score = allTasksPass ? 10 : 0;
                
                // Set IsPass based on all tasks passing
                createDto.IsPass = allTasksPass;
            }
            else
            {
                // If no tasks provided, set default values
                createDto.Score = 0;
                createDto.IsPass = false;
            }

            // 1. Tìm ActivityRecord dựa trên TraineeId, ClassId, và PracticeId
            var activityRecord = await _uow.ActivityRecordRepository
                .GetAllAsQueryable()
                .Include(ar => ar.SectionRecord)
                    .ThenInclude(sr => sr.LearningProgress)
                        .ThenInclude(lp => lp.Enrollment)
                .FirstOrDefaultAsync(ar => 
                    ar.SectionRecord.LearningProgress.Enrollment.TraineeId == createDto.TraineeId &&
                    ar.SectionRecord.LearningProgress.Enrollment.ClassId == createDto.ClassId &&
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
                    $"Activity record not found for TraineeId={createDto.TraineeId}, ClassId={createDto.ClassId}, PracticeId={createDto.PracticeId}. " +
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
                Score = createDto.Score,
                AttemptDate = DateTime.Now,
                AttemptStatus = 1, // Default status
                Description = createDto.Description,
                IsPass = createDto.IsPass,
                IsCurrent = true,
                IsDeleted = false
            };

            await _uow.PracticeAttemptRepository.CreateAsync(practiceAttempt);
            await _uow.SaveChangesAsync();

            // 4. Create PracticeAttemptTasks
            var practiceAttemptTasks = new List<PracticeAttemptTask>();
            if (createDto.PracticeAttemptTasks != null && createDto.PracticeAttemptTasks.Any())
            {
                foreach (var taskDto in createDto.PracticeAttemptTasks)
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

            await _uow.SaveChangesAsync();

            // 5. Update ActivityRecord if needed
            activityRecord.Score = createDto.Score;
            activityRecord.IsCompleted = createDto.IsPass ?? false;
            if (createDto.IsPass == true)
            {
                activityRecord.CompletedDate = DateTime.Now;
            }
            await _uow.ActivityRecordRepository.UpdateAsync(activityRecord);
            await _uow.SaveChangesAsync();

            // 6. Return DTO
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
