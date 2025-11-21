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

        #region Gets
        public async Task<IEnumerable<PracticeAttemptDto>> GetPracticeAttempts(int traineeId, int activityRecordId)
        {
            var activityRecord = await _uow.ActivityRecordRepository
                .GetAllAsQueryable()
                .Include(ar => ar.SectionRecord.LearningProgress.Enrollment)
                .AsNoTracking()
                .FirstOrDefaultAsync(ar => ar.Id == activityRecordId);

            if (activityRecord == null)
                throw new KeyNotFoundException("Activity record not found.");

            if (activityRecord.SectionRecord.LearningProgress.Enrollment.TraineeId != traineeId)
                throw new KeyNotFoundException("Activity record not found for this trainee.");

            var attempts = await _uow.PracticeAttemptRepository
                .GetAllAsQueryable()
                .AsNoTracking()
                .Include(pa => pa.PracticeAttemptTasks)
                .Where(pa => pa.ActivityRecordId == activityRecordId)
                .OrderByDescending(pa => pa.AttemptDate)
                .ToListAsync();

            return await MapToDtosWithCodesAsync(attempts);
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

            if (activityRecord.SectionRecord.LearningProgress.Enrollment.TraineeId != traineeId)
                throw new KeyNotFoundException("Activity record not found for this trainee.");

            var attempt = await _uow.PracticeAttemptRepository
                .GetAllAsQueryable()
                .AsNoTracking()
                .Include(pa => pa.PracticeAttemptTasks)
                .Where(pa => pa.ActivityRecordId == activityRecordId && pa.IsCurrent)
                .FirstOrDefaultAsync();

            if (attempt == null) return null;

            var dtos = await MapToDtosWithCodesAsync(new[] { attempt });
            return dtos.FirstOrDefault();
        }

        public async Task<IEnumerable<PracticeAttemptDto>> GetPracticeAttemptsByPractice(int traineeId, int practiceId)
        {
            var activityRecordIds = await _uow.ActivityRecordRepository
                .GetAllAsQueryable()
                .Include(ar => ar.SectionRecord.LearningProgress.Enrollment)
                .Where(ar => ar.SectionRecord.LearningProgress.Enrollment.TraineeId == traineeId)
                .Select(ar => ar.Id)
                .ToListAsync();

            if (!activityRecordIds.Any())
                return Enumerable.Empty<PracticeAttemptDto>();

            var attempts = await _uow.PracticeAttemptRepository
                .GetAllAsQueryable()
                .AsNoTracking()
                .Include(pa => pa.PracticeAttemptTasks)
                .Where(pa => activityRecordIds.Contains(pa.ActivityRecordId)
                          && pa.PracticeId == practiceId
                          && (pa.IsDeleted == null || pa.IsDeleted == false))
                .OrderByDescending(pa => pa.AttemptDate)
                .ToListAsync();

            return await MapToDtosWithCodesAsync(attempts);
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

            if (attempt == null) return null;

            var dtos = await MapToDtosWithCodesAsync(new[] { attempt });
            return dtos.FirstOrDefault();
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

            if (activityRecord.SectionRecord.LearningProgress.Enrollment.TraineeId != traineeId)
                throw new KeyNotFoundException("Activity record not found for this trainee.");

            var query = _uow.PracticeAttemptRepository
                .GetAllAsQueryable()
                .AsNoTracking()
                .Include(pa => pa.PracticeAttemptTasks)
                .Where(pa => pa.ActivityRecordId == activityRecordId)
                .OrderByDescending(pa => pa.AttemptDate);

            var pagedEntities = await query.ToPagedResultAsync(pageNumber, pageSize);

            var dtos = await MapToDtosWithCodesAsync(pagedEntities.Items);

            return new PagedResult<PracticeAttemptDto>
            {
                Items = dtos,
                TotalCount = pagedEntities.TotalCount,
                Page = pagedEntities.Page,
                PageSize = pagedEntities.PageSize
            };
        }

        public async Task<PagedResult<PracticeAttemptDto>> GetPracticeAttemptsByPracticePaged(int traineeId, int practiceId, int pageNumber, int pageSize)
        {
            if (pageNumber < 1) pageNumber = 1;
            if (pageSize < 1) pageSize = 10;

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

            var pagedEntities = await query.ToPagedResultAsync(pageNumber, pageSize);
            var dtos = await MapToDtosWithCodesAsync(pagedEntities.Items);

            return new PagedResult<PracticeAttemptDto>
            {
                Items = dtos,
                TotalCount = pagedEntities.TotalCount,
                Page = pagedEntities.Page,
                PageSize = pagedEntities.PageSize
            };
        }

        #endregion

        #region Submit Practice Attempt
        public async Task<PracticeAttemptDto> CreatePracticeAttempt(int traineeId, CreatePracticeAttemptDto createDto)
        {
            if (traineeId <= 0) throw new ArgumentException("TraineeId must be greater than 0.", nameof(traineeId));
            if (createDto.ClassId <= 0) throw new ArgumentException("ClassId must be greater than 0.", nameof(createDto.ClassId));
            if (createDto.PracticeId <= 0) throw new ArgumentException("PracticeId must be greater than 0.", nameof(createDto.PracticeId));

            var traineeExists = await _uow.TraineeRepository.ExistsAsync(t => t.Id == traineeId && (t.IsDeleted == null || t.IsDeleted == false));
            if (!traineeExists) throw new KeyNotFoundException($"Trainee with ID {traineeId} not found.");

            var classEntity = await _uow.ClassRepository.GetByIdAsync(createDto.ClassId);
            if (classEntity == null) throw new KeyNotFoundException($"Class with ID {createDto.ClassId} not found.");

            var practiceTemplate = await _uow.PracticeRepository
                .GetAllAsQueryable()
                .AsNoTracking()
                .Include(p => p.PracticeTasks)
                .FirstOrDefaultAsync(p => p.Id == createDto.PracticeId && (p.IsDeleted == null || p.IsDeleted == false));

            if (practiceTemplate == null) throw new KeyNotFoundException($"Practice with ID {createDto.PracticeId} not found.");

            var templateTaskIds = practiceTemplate.PracticeTasks.Select(pt => pt.TaskId).ToHashSet();

            var enrollmentExists = await _uow.EnrollmentRepository.ExistsAsync(e =>
                e.TraineeId == traineeId && e.ClassId == createDto.ClassId && (e.IsDeleted == null || e.IsDeleted == false));
            if (!enrollmentExists) throw new KeyNotFoundException($"Trainee {traineeId} is not enrolled in Class {createDto.ClassId}.");

            bool allTasksPass = false;
            var submittedPassedTaskIds = new HashSet<int>();

            if (createDto.PracticeAttemptTasks != null && createDto.PracticeAttemptTasks.Any())
            {
                foreach (var taskDto in createDto.PracticeAttemptTasks)
                {
                    if (taskDto.TaskId.HasValue && templateTaskIds.Contains(taskDto.TaskId.Value))
                    {
                        if (taskDto.IsPass == true) submittedPassedTaskIds.Add(taskDto.TaskId.Value);
                    }
                    else if (taskDto.TaskId.HasValue)
                    {
                        throw new ArgumentException($"Submitted TaskId {taskDto.TaskId.Value} is not part of Practice {createDto.PracticeId}.");
                    }
                }
                allTasksPass = templateTaskIds.SetEquals(submittedPassedTaskIds);
            }

            createDto.Score = allTasksPass ? 10 : 0;
            createDto.IsPass = allTasksPass;
            if(createDto.PracticeAttemptTasks == null)
            {
                createDto.PracticeAttemptTasks = new List<CreatePracticeAttemptTaskDto>();
            }

            return await SavePracticeAttempt(traineeId, createDto.ClassId, createDto.PracticeId,
                createDto.Score, createDto.Description, createDto.IsPass,
                createDto.PracticeAttemptTasks.Select(t => new PracticeAttemptTask
                {
                    TaskId = t.TaskId.GetValueOrDefault(),
                    Score = t.Score,
                    Description = t.Description,
                    IsPass = t.IsPass,
                    IsDeleted = false
                }).ToList(),
                templateTaskIds);
        }

        public async Task<PracticeAttemptDto> CreatePracticeAttemptByCode(int traineeId, CreatePracticeAttemptWithCodeDto createDto)
        {
            if (traineeId <= 0) throw new ArgumentException("TraineeId must be greater than 0.", nameof(traineeId));
            if (createDto.ClassId <= 0) throw new ArgumentException("ClassId must be greater than 0.", nameof(createDto.ClassId));
            if (string.IsNullOrWhiteSpace(createDto.PracticeCode)) throw new ArgumentException("PracticeCode is required.", nameof(createDto.PracticeCode));

            var practiceTemplate = await _uow.PracticeRepository
                .GetAllAsQueryable()
                .AsNoTracking()
                .Include(p => p.PracticeTasks)
                    .ThenInclude(pt => pt.Task) // Valid: PracticeTask has SimTask Task
                .FirstOrDefaultAsync(p => p.PracticeCode == createDto.PracticeCode && (p.IsDeleted == null || p.IsDeleted == false));

            if (practiceTemplate == null)
                throw new KeyNotFoundException($"Practice with Code '{createDto.PracticeCode}' not found.");

            int practiceId = practiceTemplate.Id;

            var taskCodeToIdMap = practiceTemplate.PracticeTasks
                .Where(pt => pt.Task != null && pt.Task.TaskCode != null)
                .ToDictionary(pt => pt.Task.TaskCode!, pt => pt.TaskId);

            var templateTaskIds = practiceTemplate.PracticeTasks.Select(pt => pt.TaskId).ToHashSet();

            var traineeExists = await _uow.TraineeRepository.ExistsAsync(t => t.Id == traineeId && (t.IsDeleted == null || t.IsDeleted == false));
            if (!traineeExists) throw new KeyNotFoundException($"Trainee with ID {traineeId} not found.");

            var enrollmentExists = await _uow.EnrollmentRepository.ExistsAsync(e =>
                e.TraineeId == traineeId && e.ClassId == createDto.ClassId && (e.IsDeleted == null || e.IsDeleted == false));
            if (!enrollmentExists) throw new KeyNotFoundException($"Trainee {traineeId} is not enrolled in Class {createDto.ClassId}.");

            bool allTasksPass = false;
            var submittedPassedTaskIds = new HashSet<int>();
            var practiceAttemptTasksToSave = new List<PracticeAttemptTask>();

            if (createDto.PracticeAttemptTasks != null && createDto.PracticeAttemptTasks.Any())
            {
                foreach (var taskDto in createDto.PracticeAttemptTasks)
                {
                    if (!taskCodeToIdMap.TryGetValue(taskDto.TaskCode, out int taskId))
                    {
                        throw new ArgumentException($"Submitted TaskCode '{taskDto.TaskCode}' is not valid for Practice '{createDto.PracticeCode}'.");
                    }

                    if (taskDto.IsPass == true)
                    {
                        submittedPassedTaskIds.Add(taskId);
                    }

                    practiceAttemptTasksToSave.Add(new PracticeAttemptTask
                    {
                        TaskId = taskId,
                        Score = taskDto.Score,
                        Description = taskDto.Description,
                        IsPass = taskDto.IsPass,
                        IsDeleted = false
                    });
                }

                allTasksPass = templateTaskIds.SetEquals(submittedPassedTaskIds);
            }

            createDto.Score = allTasksPass ? 10 : 0;
            createDto.IsPass = allTasksPass;

            return await SavePracticeAttempt(traineeId, createDto.ClassId, practiceId,
                createDto.Score, createDto.Description, createDto.IsPass,
                practiceAttemptTasksToSave, templateTaskIds);
        }

        private async Task<PracticeAttemptDto> SavePracticeAttempt(
            int traineeId,
            int classId,
            int practiceId,
            decimal? score,
            string? description,
            bool? isPass,
            List<PracticeAttemptTask> tasksToSave,
            HashSet<int> templateTaskIds)
        {
            var activityRecord = await _uow.ActivityRecordRepository
                .GetAllAsQueryable()
                .Include(ar => ar.SectionRecord)
                    .ThenInclude(sr => sr.LearningProgress)
                        .ThenInclude(lp => lp.Enrollment)
                .FirstOrDefaultAsync(ar =>
                    ar.SectionRecord.LearningProgress.Enrollment.TraineeId == traineeId &&
                    ar.SectionRecord.LearningProgress.Enrollment.ClassId == classId &&
                    ar.ActivityType == (int)ActivityType.Practice &&
                    ar.ActivityId != null &&
                    ar.ActivityId == (
                        _uow.ActivityPracticeRepository.GetAllAsQueryable()
                            .Where(ap => ap.PracticeId == practiceId)
                            .Select(ap => ap.ActivityId)
                            .FirstOrDefault()
                    ));

            if (activityRecord == null)
            {
                throw new KeyNotFoundException(
                    $"Activity record not found for TraineeId={traineeId}, ClassId={classId}, PracticeId={practiceId}. " +
                    "Please ensure the practice is assigned to an activity in this class.");
            }

            var existingAttempts = await _uow.PracticeAttemptRepository
                .GetAllAsQueryable()
                .Where(pa => pa.ActivityRecordId == activityRecord.Id && pa.IsCurrent)
                .ToListAsync();

            foreach (var attempt in existingAttempts)
            {
                attempt.IsCurrent = false;
                await _uow.PracticeAttemptRepository.UpdateAsync(attempt);
            }

            var practiceAttempt = new PracticeAttempt
            {
                ActivityRecordId = activityRecord.Id,
                PracticeId = practiceId,
                Score = score,
                AttemptDate = DateTime.Now,
                AttemptStatus = (int)ActivityRecordStatusEnum.InProgress,
                Description = description,
                IsPass = isPass,
                IsCurrent = true,
                IsDeleted = false
            };

            await _uow.PracticeAttemptRepository.CreateAsync(practiceAttempt);
            await _uow.SaveChangesAsync();

            foreach (var task in tasksToSave)
            {
                if (templateTaskIds.Contains(task.TaskId.GetValueOrDefault()))
                {
                    task.PracticeAttemptId = practiceAttempt.Id;
                    await _uow.PracticeAttemptTaskRepository.CreateAsync(task);
                }
            }

            await _uow.SaveChangesAsync();

            await _progressHelper.UpdateActivityRecordProgressAsync(traineeId, activityRecord.Id);
            await _progressHelper.UpdateSectionRecordProgressAsync(traineeId, activityRecord.SectionRecordId);
            await _progressHelper.UpdateLearningProgressProgressAsync(traineeId, activityRecord.SectionRecord.LearningProgressId);

            var result = await GetPracticeAttemptById(practiceAttempt.Id);
            if (result == null)
                throw new InvalidOperationException("Failed to retrieve the created practice attempt.");

            return result;
        }

        #endregion

        #region Helper Methods
        private async Task<List<PracticeAttemptDto>> MapToDtosWithCodesAsync(IEnumerable<PracticeAttempt> attempts)
        {
            var attemptList = attempts.ToList();
            if (!attemptList.Any()) return new List<PracticeAttemptDto>();

            // 1. Collect IDs
            var practiceIds = attemptList.Where(a => a.PracticeId.HasValue)
                                         .Select(a => a.PracticeId)
                                         .Distinct()
                                         .ToList();

            var taskIds = attemptList.SelectMany(a => a.PracticeAttemptTasks)
                                     .Where(t => t.TaskId.HasValue)
                                     .Select(t => t.TaskId)
                                     .Distinct()
                                     .ToList();

            // 2. Bulk Fetch Codes
            var practiceMap = new Dictionary<int, string>();
            if (practiceIds.Any())
            {
                practiceMap = await _uow.PracticeRepository.GetAllAsQueryable()
                    .Where(p => practiceIds.Contains(p.Id))
                    .Select(p => new { p.Id, p.PracticeCode })
                    .ToDictionaryAsync(x => x.Id, x => x.PracticeCode ?? "");
            }

            var taskMap = new Dictionary<int, string>();
            if (taskIds.Any())
            {
                taskMap = await _uow.SimTaskRepository.GetAllAsQueryable()
                    .Where(t => taskIds.Contains(t.Id))
                    .Select(t => new { t.Id, t.TaskCode })
                    .ToDictionaryAsync(x => x.Id, x => x.TaskCode ?? "");
            }

            // 3. Map
            var dtos = new List<PracticeAttemptDto>();
            foreach (var pa in attemptList)
            {
                var dto = MapToDtoBasic(pa);

                // Set Practice Code
                if (pa.PracticeId.HasValue && practiceMap.TryGetValue(pa.PracticeId.Value, out var pCode))
                {
                    dto.PracticeCode = pCode;
                }

                // Set Task Codes
                foreach (var tDto in dto.PracticeAttemptTasks)
                {
                    if (tDto.TaskId.HasValue && taskMap.TryGetValue(tDto.TaskId.Value, out var tCode))
                    {
                        tDto.TaskCode = tCode;
                    }
                }

                dtos.Add(dto);
            }

            return dtos;
        }

        private static PracticeAttemptDto MapToDtoBasic(PracticeAttempt pa)
        {
            return new PracticeAttemptDto
            {
                Id = pa.Id,
                ActivityRecordId = pa.ActivityRecordId,
                PracticeId = pa.PracticeId,
                // PracticeCode is set later
                Score = pa.Score,
                AttemptDate = pa.AttemptDate,
                AttemptStatus = pa.AttemptStatus?.ToString() ?? "Unknown",
                Description = pa.Description,
                IsPass = pa.IsPass,
                IsCurrent = pa.IsCurrent,
                PracticeAttemptTasks = pa.PracticeAttemptTasks.Select(MapToTaskDtoBasic).ToList()
            };
        }

        private static PracticeAttemptTaskDto MapToTaskDtoBasic(PracticeAttemptTask pat)
        {
            return new PracticeAttemptTaskDto
            {
                Id = pat.Id,
                PracticeAttemptId = pat.PracticeAttemptId,
                TaskId = pat.TaskId,
                // TaskCode is set later
                Score = pat.Score,
                Description = pat.Description,
                IsPass = pat.IsPass
            };
        }

        #endregion
    }
}