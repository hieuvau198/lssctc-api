using Lssctc.ProgramManagement.Activities.Services;
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
        private readonly IActivitySessionService _sessionService;

        public PracticeAttemptsService(IUnitOfWork uow, IActivitySessionService sessionService)
        {
            _uow = uow;
            _progressHelper = new ProgressHelper(uow);
            _sessionService = sessionService;
        }

        public async Task<IEnumerable<PracticeAttemptDto>> GetPracticeAttempts(int traineeId, int activityRecordId)
        {
            var activityRecord = await _uow.ActivityRecordRepository.GetAllAsQueryable()
                .Include(ar => ar.SectionRecord.LearningProgress.Enrollment).AsNoTracking()
                .FirstOrDefaultAsync(ar => ar.Id == activityRecordId);

            if (activityRecord == null) throw new KeyNotFoundException("Activity record not found.");
            if (activityRecord.SectionRecord.LearningProgress.Enrollment.TraineeId != traineeId)
                throw new KeyNotFoundException("Activity record not found for this trainee.");

            var attempts = await _uow.PracticeAttemptRepository.GetAllAsQueryable().AsNoTracking()
                .Include(pa => pa.PracticeAttemptTasks)
                .Where(pa => pa.ActivityRecordId == activityRecordId)
                .OrderByDescending(pa => pa.AttemptDate).ToListAsync();

            return await MapToDtosWithFullTaskContextAsync(attempts);
        }

        public async Task<PracticeAttemptDto?> GetLatestPracticeAttempt(int traineeId, int activityRecordId)
        {
            var attempt = await _uow.PracticeAttemptRepository.GetAllAsQueryable().AsNoTracking()
                .Include(pa => pa.PracticeAttemptTasks)
                .Where(pa => pa.ActivityRecordId == activityRecordId && pa.IsCurrent)
                .FirstOrDefaultAsync();

            if (attempt == null) return null;
            return (await MapToDtosWithFullTaskContextAsync(new[] { attempt })).FirstOrDefault();
        }

        // REMOVED: GetPracticeAttemptsByPractice method has been removed as per requirement change.

        public async Task<PracticeAttemptDto?> GetPracticeAttemptById(int practiceAttemptId)
        {
            var attempt = await _uow.PracticeAttemptRepository.GetAllAsQueryable().AsNoTracking()
                .Include(pa => pa.PracticeAttemptTasks)
                .FirstOrDefaultAsync(pa => pa.Id == practiceAttemptId);

            if (attempt == null) return null;
            return (await MapToDtosWithFullTaskContextAsync(new[] { attempt })).FirstOrDefault();
        }

        public async Task<PagedResult<PracticeAttemptDto>> GetPracticeAttemptsPaged(int traineeId, int activityRecordId, int pageNumber, int pageSize)
        {
            var query = _uow.PracticeAttemptRepository.GetAllAsQueryable().AsNoTracking()
                .Include(pa => pa.PracticeAttemptTasks)
                .Where(pa => pa.ActivityRecordId == activityRecordId)
                .OrderByDescending(pa => pa.AttemptDate);

            var paged = await query.ToPagedResultAsync(pageNumber, pageSize);
            var dtos = await MapToDtosWithFullTaskContextAsync(paged.Items);
            return new PagedResult<PracticeAttemptDto> { Items = dtos, TotalCount = paged.TotalCount, Page = paged.Page, PageSize = paged.PageSize };
        }

        public async Task<PagedResult<PracticeAttemptDto>> GetPracticeAttemptsByPracticePaged(int traineeId, int practiceId, int pageNumber, int pageSize)
        {
            var activityRecordIds = await _uow.ActivityRecordRepository.GetAllAsQueryable()
               .Include(ar => ar.SectionRecord.LearningProgress.Enrollment)
               .Where(ar => ar.SectionRecord.LearningProgress.Enrollment.TraineeId == traineeId)
               .Select(ar => ar.Id).ToListAsync();

            if (!activityRecordIds.Any()) return new PagedResult<PracticeAttemptDto> { Items = new List<PracticeAttemptDto>(), TotalCount = 0 };

            var query = _uow.PracticeAttemptRepository.GetAllAsQueryable().AsNoTracking()
                .Include(pa => pa.PracticeAttemptTasks)
                .Where(pa => activityRecordIds.Contains(pa.ActivityRecordId) && pa.PracticeId == practiceId)
                .OrderByDescending(pa => pa.AttemptDate);

            var paged = await query.ToPagedResultAsync(pageNumber, pageSize);
            var dtos = await MapToDtosWithFullTaskContextAsync(paged.Items);
            return new PagedResult<PracticeAttemptDto> { Items = dtos, TotalCount = paged.TotalCount, Page = paged.Page, PageSize = paged.PageSize };
        }

        public async Task<PracticeAttemptDto> CreatePracticeAttempt(int traineeId, CreatePracticeAttemptDto createDto)
        {
            return await CreatePracticeAttemptByCode(traineeId, new CreatePracticeAttemptWithCodeDto
            {
                ClassId = createDto.ClassId,
                ActivityRecordId = 0
            });
        }

        public async Task<PracticeAttemptDto> CreatePracticeAttemptByCode(int traineeId, CreatePracticeAttemptWithCodeDto createDto)
        {
            if (traineeId <= 0) throw new ArgumentException("Invalid TraineeId.");
            if (createDto.ActivityRecordId <= 0) throw new ArgumentException("ActivityRecordId is required.");

            var activityRecord = await _uow.ActivityRecordRepository.GetByIdAsync(createDto.ActivityRecordId);
            if (activityRecord == null) throw new KeyNotFoundException("ActivityRecord not found.");

            if (activityRecord.ActivityId.HasValue)
            {
                await _sessionService.CheckActivityAccess(createDto.ClassId, activityRecord.ActivityId.Value);
            }

            var practiceTemplate = await _uow.PracticeRepository.GetAllAsQueryable().AsNoTracking()
                .Include(p => p.PracticeTasks).ThenInclude(pt => pt.Task)
                .FirstOrDefaultAsync(p => p.PracticeCode == createDto.PracticeCode);

            if (practiceTemplate == null) throw new KeyNotFoundException($"Practice '{createDto.PracticeCode}' not found.");

            var taskCodeMap = practiceTemplate.PracticeTasks
                .Where(pt => pt.Task?.TaskCode != null)
                .ToDictionary(pt => pt.Task!.TaskCode!, pt => pt.TaskId);

            int totalPracticeTasks = practiceTemplate.PracticeTasks.Count;

            var templateTaskIds = practiceTemplate.PracticeTasks.Select(pt => pt.TaskId).ToHashSet();
            var passedTaskIds = new HashSet<int>();
            var tasksToSave = new List<PracticeAttemptTask>();

            if (createDto.PracticeAttemptTasks != null)
            {
                foreach (var taskDto in createDto.PracticeAttemptTasks)
                {
                    if (!taskCodeMap.TryGetValue(taskDto.TaskCode, out int taskId)) continue;

                    if (taskDto.IsPass == true) passedTaskIds.Add(taskId);

                    decimal taskScore = CalculateTaskScore(totalPracticeTasks, taskDto.Mistakes ?? 0);

                    tasksToSave.Add(new PracticeAttemptTask
                    {
                        TaskId = taskId,
                        Score = taskScore,
                        Mistakes = taskDto.Mistakes,
                        Description = taskDto.Description,
                        IsPass = taskDto.IsPass,
                        IsDeleted = false
                    });
                }
            }

            bool allPass = templateTaskIds.SetEquals(passedTaskIds);
            decimal practiceScore = tasksToSave.Sum(t => t.Score ?? 0);

            var oldAttempts = await _uow.PracticeAttemptRepository.GetAllAsQueryable()
                .Where(pa => pa.ActivityRecordId == createDto.ActivityRecordId && pa.IsCurrent)
                .ToListAsync();
            foreach (var old in oldAttempts) { old.IsCurrent = false; await _uow.PracticeAttemptRepository.UpdateAsync(old); }

            var attempt = new PracticeAttempt
            {
                ActivityRecordId = createDto.ActivityRecordId,
                PracticeId = practiceTemplate.Id,
                Score = practiceScore,
                TotalMistakes = createDto.TotalMistakes,
                StartTime = createDto.StartTime?.AddHours(-7),
                EndTime = createDto.EndTime?.AddHours(-7),
                DurationSeconds = createDto.DurationSeconds,
                AttemptDate = DateTime.UtcNow.AddHours(7),
                AttemptStatus = (int)ActivityRecordStatusEnum.Completed,
                Description = createDto.Description,
                IsPass = allPass,
                IsCurrent = true,
                IsDeleted = false
            };

            await _uow.PracticeAttemptRepository.CreateAsync(attempt);
            await _uow.SaveChangesAsync();

            foreach (var task in tasksToSave)
            {
                task.PracticeAttemptId = attempt.Id;
                await _uow.PracticeAttemptTaskRepository.CreateAsync(task);
            }
            await _uow.SaveChangesAsync();

            await _progressHelper.UpdateActivityRecordProgressAsync(traineeId, activityRecord.Id);
            var fullRecord = await _uow.ActivityRecordRepository.GetByIdAsync(activityRecord.Id);
            if (fullRecord != null)
            {
                await _progressHelper.UpdateSectionRecordProgressAsync(traineeId, fullRecord.SectionRecordId);
            }

            return (await GetPracticeAttemptById(attempt.Id))!;
        }

        public async Task<PracticeAttemptDto> SubmitSinglePracticeTask(int traineeId, SubmitPracticeTaskDto submitDto)
        {
            var activityRecord = await _uow.ActivityRecordRepository.GetByIdAsync(submitDto.ActivityRecordId);
            if (activityRecord == null) throw new KeyNotFoundException("ActivityRecord not found.");

            var attempt = await _uow.PracticeAttemptRepository.GetAllAsQueryable()
                .Include(pa => pa.PracticeAttemptTasks)
                .FirstOrDefaultAsync(pa => pa.ActivityRecordId == submitDto.ActivityRecordId && pa.IsCurrent);

            if (attempt == null)
            {
                var practiceId = await _uow.ActivityPracticeRepository.GetAllAsQueryable()
                     .Where(ap => ap.ActivityId == activityRecord.ActivityId).Select(ap => ap.PracticeId).FirstOrDefaultAsync();

                if (practiceId == 0) throw new InvalidOperationException("Practice not found for this activity.");

                attempt = new PracticeAttempt
                {
                    ActivityRecordId = submitDto.ActivityRecordId,
                    PracticeId = practiceId,
                    AttemptDate = DateTime.UtcNow.AddHours(7),
                    AttemptStatus = (int)ActivityRecordStatusEnum.InProgress,
                    IsCurrent = true,
                    IsDeleted = false,
                    Score = 0
                };
                await _uow.PracticeAttemptRepository.CreateAsync(attempt);
                await _uow.SaveChangesAsync();
            }

            var practice = await _uow.PracticeRepository.GetAllAsQueryable().AsNoTracking()
                .Include(p => p.PracticeTasks).ThenInclude(pt => pt.Task)
                .FirstOrDefaultAsync(p => p.Id == attempt.PracticeId);

            var taskDef = practice?.PracticeTasks.FirstOrDefault(pt => pt.Task?.TaskCode == submitDto.TaskCode);
            if (taskDef == null) throw new ArgumentException($"Invalid TaskCode: {submitDto.TaskCode}");

            int totalPracticeTasks = practice?.PracticeTasks.Count ?? 0;
            decimal taskScore = CalculateTaskScore(totalPracticeTasks, submitDto.Mistakes ?? 0);

            var existingTask = attempt.PracticeAttemptTasks.FirstOrDefault(t => t.TaskId == taskDef.TaskId);
            if (existingTask != null)
            {
                existingTask.Score = taskScore;
                existingTask.Mistakes = submitDto.Mistakes;
                existingTask.IsPass = submitDto.IsPass;
                existingTask.Description = submitDto.Description;
                await _uow.PracticeAttemptTaskRepository.UpdateAsync(existingTask);
            }
            else
            {
                var newTask = new PracticeAttemptTask
                {
                    PracticeAttemptId = attempt.Id,
                    TaskId = taskDef.TaskId,
                    Score = taskScore,
                    Mistakes = submitDto.Mistakes,
                    IsPass = submitDto.IsPass,
                    Description = submitDto.Description,
                    IsDeleted = false
                };
                attempt.PracticeAttemptTasks.Add(newTask);
                await _uow.PracticeAttemptTaskRepository.CreateAsync(newTask);
            }

            attempt.AttemptDate = DateTime.UtcNow.AddHours(7);
            attempt.Score = attempt.PracticeAttemptTasks.Sum(t => t.Score);

            await _uow.SaveChangesAsync();
            await _progressHelper.UpdateActivityRecordProgressAsync(traineeId, activityRecord.Id);

            return (await GetPracticeAttemptById(attempt.Id))!;
        }

        private decimal CalculateTaskScore(int totalTasksCount, int mistakes)
        {
            if (totalTasksCount <= 0) return 0;
            decimal maxScorePerTask = 10m / (decimal)totalTasksCount;
            decimal penalty = mistakes * 0.5m;
            return Math.Max(0, maxScorePerTask - penalty);
        }

        private async Task<List<PracticeAttemptDto>> MapToDtosWithFullTaskContextAsync(IEnumerable<PracticeAttempt> attempts)
        {
            var attemptList = attempts.ToList();
            if (!attemptList.Any()) return new List<PracticeAttemptDto>();

            var practiceIds = attemptList.Where(a => a.PracticeId.HasValue).Select(a => a.PracticeId.Value).Distinct().ToList();
            var practiceTemplates = new Dictionary<int, Practice>();
            if (practiceIds.Any())
            {
                practiceTemplates = await _uow.PracticeRepository.GetAllAsQueryable()
                    .Where(p => practiceIds.Contains(p.Id))
                    .Include(p => p.PracticeTasks).ThenInclude(pt => pt.Task)
                    .AsNoTracking().ToDictionaryAsync(p => p.Id, p => p);
            }

            var dtos = new List<PracticeAttemptDto>();
            foreach (var pa in attemptList)
            {
                var dto = new PracticeAttemptDto
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
                    TotalMistakes = pa.TotalMistakes,
                    StartTime = pa.StartTime,
                    EndTime = pa.EndTime,
                    DurationSeconds = pa.DurationSeconds,
                    PracticeAttemptTasks = new List<PracticeAttemptTaskDto>()
                };

                if (pa.PracticeId.HasValue && practiceTemplates.TryGetValue(pa.PracticeId.Value, out var template))
                {
                    dto.PracticeCode = template.PracticeCode;
                    var existingTasksMap = pa.PracticeAttemptTasks.Where(t => t.TaskId.HasValue).ToDictionary(t => t.TaskId.Value, t => t);

                    foreach (var templateTask in template.PracticeTasks)
                    {
                        var taskId = templateTask.TaskId;
                        var taskCode = templateTask.Task?.TaskCode;

                        if (existingTasksMap.TryGetValue(taskId, out var recordedTask))
                        {
                            dto.PracticeAttemptTasks.Add(new PracticeAttemptTaskDto
                            {
                                Id = recordedTask.Id,
                                PracticeAttemptId = recordedTask.PracticeAttemptId,
                                TaskId = taskId,
                                TaskCode = taskCode,
                                Score = recordedTask.Score,
                                Mistakes = recordedTask.Mistakes,
                                Description = recordedTask.Description,
                                IsPass = recordedTask.IsPass
                            });
                        }
                        else
                        {
                            dto.PracticeAttemptTasks.Add(new PracticeAttemptTaskDto
                            {
                                Id = 0,
                                PracticeAttemptId = pa.Id,
                                TaskId = taskId,
                                TaskCode = taskCode,
                                Score = 0,
                                Description = "Not Attempted",
                                IsPass = false
                            });
                        }
                    }
                }
                else
                {
                    dto.PracticeAttemptTasks = pa.PracticeAttemptTasks.Select(pat => new PracticeAttemptTaskDto
                    {
                        Id = pat.Id,
                        PracticeAttemptId = pat.PracticeAttemptId,
                        TaskId = pat.TaskId,
                        Score = pat.Score,
                        Mistakes = pat.Mistakes,
                        Description = pat.Description,
                        IsPass = pat.IsPass
                    }).ToList();
                }
                dtos.Add(dto);
            }
            return dtos;
        }
    }
}