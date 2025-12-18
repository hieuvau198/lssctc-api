using Lssctc.ProgramManagement.Activities.Services;
using Lssctc.ProgramManagement.Practices.Dtos;
using Lssctc.Share.Entities;
using Lssctc.Share.Enums;
using Lssctc.Share.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Lssctc.ProgramManagement.Practices.Services
{
    public class TraineePracticesService : ITraineePracticesService
    {
        private readonly IUnitOfWork _uow;
        private readonly IActivitySessionService _sessionService;

        public TraineePracticesService(IUnitOfWork uow, IActivitySessionService sessionService)
        {
            _uow = uow;
            _sessionService = sessionService;
        }

        public async Task<IEnumerable<TraineePracticeDto>> GetPracticesForTraineeAsync(int traineeId, int classId)
        {
            var learningProgress = await _uow.LearningProgressRepository
                .GetAllAsQueryable()
                .AsNoTracking()
                .Include(lp => lp.Enrollment)
                .Where(lp => lp.Enrollment.TraineeId == traineeId && lp.Enrollment.ClassId == classId)
                .Select(lp => new { lp.Id })
                .FirstOrDefaultAsync();

            if (learningProgress == null) return new List<TraineePracticeDto>();

            var practiceActivityRecords = await _uow.ActivityRecordRepository
                .GetAllAsQueryable()
                .AsNoTracking()
                .Where(ar => ar.SectionRecord.LearningProgressId == learningProgress.Id &&
                             ar.ActivityType == (int)ActivityType.Practice)
                .Select(ar => new { ar.Id, ar.ActivityId, ar.IsCompleted })
                .ToListAsync();

            if (!practiceActivityRecords.Any()) return new List<TraineePracticeDto>();

            var distinctActivityIds = practiceActivityRecords
                .Where(ar => ar.ActivityId.HasValue)
                .Select(ar => ar.ActivityId)
                .Distinct()
                .ToList();

            var currentTime = DateTime.UtcNow;

            var availableActivityIds = await _uow.ActivitySessionRepository
                .GetAllAsQueryable()
                .AsNoTracking()
                .Where(s => s.ClassId == classId &&
                            distinctActivityIds.Contains(s.ActivityId) &&
                            s.IsActive == true &&
                            (s.StartTime == null || s.StartTime <= currentTime) &&
                            (s.EndTime == null || s.EndTime >= currentTime))
                .Select(s => s.ActivityId)
                .ToListAsync();

            var availableSet = new HashSet<int>(availableActivityIds);
            practiceActivityRecords = practiceActivityRecords
                .Where(ar => ar.ActivityId.HasValue && availableSet.Contains(ar.ActivityId.Value))
                .ToList();

            if (!practiceActivityRecords.Any()) return new List<TraineePracticeDto>();

            var activityIds = practiceActivityRecords.Select(ar => ar.ActivityId).Distinct();
            var activityPracticeMap = await _uow.ActivityPracticeRepository
                .GetAllAsQueryable()
                .AsNoTracking()
                .Include(ap => ap.Practice)
                    .ThenInclude(p => p.PracticeTasks)
                        .ThenInclude(pt => pt.Task)
                .Where(ap => activityIds.Contains(ap.ActivityId) && ap.Practice.IsDeleted != true)
                .ToDictionaryAsync(ap => ap.ActivityId, ap => ap.Practice);

            var activityRecordIds = practiceActivityRecords.Select(ar => ar.Id).ToList();
            var currentAttemptsMap = await _uow.PracticeAttemptRepository
                .GetAllAsQueryable()
                .AsNoTracking()
                .Include(pa => pa.PracticeAttemptTasks)
                .Where(pa => activityRecordIds.Contains(pa.ActivityRecordId) && pa.IsCurrent)
                .ToDictionaryAsync(pa => pa.ActivityRecordId);

            var results = new List<TraineePracticeDto>();
            foreach (var ar in practiceActivityRecords)
            {
                if (ar.ActivityId.HasValue && activityPracticeMap.TryGetValue(ar.ActivityId.Value, out var practice))
                {
                    var dto = new TraineePracticeDto
                    {
                        ActivityRecordId = ar.Id,
                        ActivityId = ar.ActivityId.Value,
                        IsCompleted = ar.IsCompleted ?? false,
                        Id = practice.Id,
                        PracticeName = practice.PracticeName,
                        PracticeCode = practice.PracticeCode,
                        PracticeDescription = practice.PracticeDescription,
                        EstimatedDurationMinutes = practice.EstimatedDurationMinutes,
                        DifficultyLevel = practice.DifficultyLevel,
                        MaxAttempts = practice.MaxAttempts,
                        CreatedDate = practice.CreatedDate,
                        IsActive = practice.IsActive,
                        Tasks = new List<TraineeTaskDto>()
                    };

                    currentAttemptsMap.TryGetValue(ar.Id, out var currentAttempt);

                    var taskTemplates = practice.PracticeTasks
                        .Where(pt => pt.Task != null && pt.Task.IsDeleted != true)
                        .Select(pt => pt.Task);

                    if (currentAttempt != null)
                    {
                        var attemptTasksMap = currentAttempt.PracticeAttemptTasks
                            .Where(pat => pat.TaskId.HasValue)
                            .ToDictionary(pat => pat.TaskId!.Value);

                        foreach (var template in taskTemplates)
                        {
                            attemptTasksMap.TryGetValue(template.Id, out var attemptTask);
                            dto.Tasks.Add(MapToTaskDto(template, attemptTask));
                        }
                    }
                    else
                    {
                        foreach (var template in taskTemplates)
                        {
                            dto.Tasks.Add(MapToTaskDto(template, null));
                        }
                    }
                    results.Add(dto);
                }
            }
            return results;
        }

        public async Task<TraineePracticeResponseDto?> GetPracticeForTraineeByActivityIdAsync(int traineeId, int activityRecordId)
        {
            var activityRecord = await _uow.ActivityRecordRepository
                .GetAllAsQueryable()
                .AsNoTracking()
                .Include(ar => ar.SectionRecord.LearningProgress.Enrollment)
                .Where(ar => ar.Id == activityRecordId &&
                             ar.SectionRecord.LearningProgress.Enrollment.TraineeId == traineeId &&
                             ar.ActivityType == (int)ActivityType.Practice)
                .Select(ar => new
                {
                    ar.Id,
                    ar.ActivityId,
                    ar.IsCompleted,
                    ClassId = ar.SectionRecord.LearningProgress.Enrollment.ClassId
                })
                .FirstOrDefaultAsync();

            if (activityRecord == null) throw new KeyNotFoundException("Practice activity record not found for this trainee.");
            if (!activityRecord.ActivityId.HasValue) throw new KeyNotFoundException("Activity record is not linked to a practice template.");

            var session = await _uow.ActivitySessionRepository
                .GetAllAsQueryable()
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.ClassId == activityRecord.ClassId &&
                                          s.ActivityId == activityRecord.ActivityId.Value &&
                                          s.IsActive == true);

            var sessionStatus = new PracticeSessionStatusDto
            {
                IsOpen = true,
                Message = "Available"
            };

            if (session != null)
            {
                sessionStatus.StartTime = session.StartTime;
                sessionStatus.EndTime = session.EndTime;
                var now = DateTime.UtcNow;

                if (sessionStatus.StartTime.HasValue && now < sessionStatus.StartTime.Value)
                {
                    sessionStatus.IsOpen = false;
                    sessionStatus.Message = "Not started yet";
                }
                else if (sessionStatus.EndTime.HasValue && now > sessionStatus.EndTime.Value)
                {
                    sessionStatus.IsOpen = false;
                    sessionStatus.Message = "Expired";
                }
            }

            var practice = await _uow.ActivityPracticeRepository
                .GetAllAsQueryable()
                .AsNoTracking()
                .Include(ap => ap.Practice)
                    .ThenInclude(p => p.PracticeTasks)
                        .ThenInclude(pt => pt.Task)
                .Where(ap => ap.ActivityId == activityRecord.ActivityId.Value && ap.Practice.IsDeleted != true)
                .Select(ap => ap.Practice)
                .FirstOrDefaultAsync();

            if (practice == null) throw new KeyNotFoundException("Practice template not found for this activity.");

            var currentAttempt = await _uow.PracticeAttemptRepository
                .GetAllAsQueryable()
                .AsNoTracking()
                .Include(pa => pa.PracticeAttemptTasks)
                .Where(pa => pa.ActivityRecordId == activityRecord.Id && pa.IsCurrent)
                .FirstOrDefaultAsync();

            var dto = new TraineePracticeDto
            {
                ActivityRecordId = activityRecord.Id,
                ActivityId = activityRecord.ActivityId.Value,
                IsCompleted = activityRecord.IsCompleted ?? false,
                Id = practice.Id,
                PracticeName = practice.PracticeName,
                PracticeCode = practice.PracticeCode,
                PracticeDescription = practice.PracticeDescription,
                EstimatedDurationMinutes = practice.EstimatedDurationMinutes,
                DifficultyLevel = practice.DifficultyLevel,
                MaxAttempts = practice.MaxAttempts,
                CreatedDate = practice.CreatedDate,
                IsActive = practice.IsActive,
                Tasks = new List<TraineeTaskDto>()
            };

            var taskTemplates = practice.PracticeTasks
                .Where(pt => pt.Task != null && pt.Task.IsDeleted != true)
                .Select(pt => pt.Task);

            if (currentAttempt != null)
            {
                var attemptTasksMap = currentAttempt.PracticeAttemptTasks
                    .Where(pat => pat.TaskId.HasValue)
                    .ToDictionary(pat => pat.TaskId!.Value);

                foreach (var template in taskTemplates)
                {
                    attemptTasksMap.TryGetValue(template.Id, out var attemptTask);
                    dto.Tasks.Add(MapToTaskDto(template, attemptTask));
                }
            }
            else
            {
                foreach (var template in taskTemplates)
                {
                    dto.Tasks.Add(MapToTaskDto(template, null));
                }
            }

            return new TraineePracticeResponseDto
            {
                Practice = dto,
                SessionStatus = sessionStatus
            };
        }

        private static TraineeTaskDto MapToTaskDto(SimTask template, PracticeAttemptTask? attemptTask)
        {
            return new TraineeTaskDto
            {
                TaskId = template.Id,
                TaskName = template.TaskName,
                TaskCode = template.TaskCode,
                TaskDescription = template.TaskDescription,
                ExpectedResult = template.ExpectedResult,
                PracticeAttemptTaskId = attemptTask?.Id ?? 0,
                IsPass = attemptTask?.IsPass ?? false,
                Score = attemptTask?.Score,
                Description = attemptTask?.Description
            };
        }
    }
}