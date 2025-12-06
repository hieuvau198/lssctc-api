using Lssctc.ProgramManagement.Activities.Services;
using Lssctc.ProgramManagement.ClassManage.ActivityRecords.Dtos;
using Lssctc.Share.Entities;
using Lssctc.Share.Enums;
using Lssctc.Share.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Lssctc.ProgramManagement.ClassManage.ActivityRecords.Services
{
    public class ActivityRecordsService : IActivityRecordsService
    {
        private readonly IUnitOfWork _uow;
        private readonly IActivitySessionService _sessionService;
        public ActivityRecordsService(IUnitOfWork uow, IActivitySessionService activitySessionService)
        {
            _uow = uow;
            _sessionService = activitySessionService;
        }

        public async Task<IEnumerable<ActivityRecordDto>> GetActivityRecordsAsync(int classId, int sectionId, int traineeId)
        {
            // MODIFIED: Join with Activity to get ActivityTitle
            var records = await GetActivityRecordQuery()
                .Where(ar => ar.SectionRecord.LearningProgress.Enrollment.ClassId == classId &&
                             ar.SectionRecord.SectionId == sectionId &&
                             ar.SectionRecord.LearningProgress.Enrollment.TraineeId == traineeId)
                .Join(
                    _uow.ActivityRepository.GetAllAsQueryable(),
                    ar => ar.ActivityId,
                    a => a.Id,
                    (ar, a) => new { ActivityRecord = ar, ActivityTitle = a.ActivityTitle }
                )
                .ToListAsync();

            // MODIFIED: Call new MapToDto overload
            return records.Select(r => MapToDto(r.ActivityRecord, r.ActivityTitle));
        }

        public async Task<IEnumerable<ActivityRecordDto>> GetActivityRecordsByActivityAsync(int classId, int sectionId, int activityId)
        {
            // MODIFIED: Join with Activity to get ActivityTitle
            var records = await GetActivityRecordQuery()
                .Where(ar => ar.SectionRecord.LearningProgress.Enrollment.ClassId == classId &&
                             ar.SectionRecord.SectionId == sectionId &&
                             ar.ActivityId == activityId)
                .Join(
                    _uow.ActivityRepository.GetAllAsQueryable(),
                    ar => ar.ActivityId,
                    a => a.Id,
                    (ar, a) => new { ActivityRecord = ar, ActivityTitle = a.ActivityTitle }
                )
                .ToListAsync();

            // MODIFIED: Call new MapToDto overload
            return records.Select(r => MapToDto(r.ActivityRecord, r.ActivityTitle));
        }

        public async Task<ActivityRecordDto> SubmitActivityAsync(int traineeId, SubmitActivityRecordDto dto)
        {
            var activityRecord = await _uow.ActivityRecordRepository
                .GetAllAsQueryable()
                .Include(ar => ar.SectionRecord.LearningProgress.Enrollment)
                .FirstOrDefaultAsync(ar => ar.Id == dto.ActivityRecordId);

            if (activityRecord == null)
                throw new KeyNotFoundException("Activity record not found.");

            if (activityRecord.SectionRecord.LearningProgress.Enrollment.TraineeId != traineeId)
                throw new UnauthorizedAccessException("You are not authorized to submit this activity.");
            await _sessionService.CheckActivityAccess(
                activityRecord.SectionRecord.LearningProgress.Enrollment.ClassId,
                activityRecord.ActivityId.Value);

            if (activityRecord.Status == (int)ActivityRecordStatusEnum.Completed)
                throw new InvalidOperationException("This activity has already been completed.");

            if (activityRecord.ActivityType != (int)ActivityType.Material)
            {
                throw new InvalidOperationException("This endpoint is only for submitting 'Material' activities. Quiz and Practice must be submitted via their respective services.");
            }

            activityRecord.Status = (int)ActivityRecordStatusEnum.Completed;
            activityRecord.IsCompleted = true;
            activityRecord.CompletedDate = DateTime.UtcNow;

            await _uow.ActivityRecordRepository.UpdateAsync(activityRecord);
            await _uow.SaveChangesAsync();

            // MODIFIED: Fetch the name to pass to MapToDto
            var updatedRecord = await GetActivityRecordQuery().FirstAsync(ar => ar.Id == activityRecord.Id);
            string activityName = "N/A";
            if (updatedRecord.ActivityId.HasValue)
            {
                var activity = await _uow.ActivityRepository.GetByIdAsync(updatedRecord.ActivityId.Value);
                if (activity != null)
                    activityName = activity.ActivityTitle;
            }

            return MapToDto(updatedRecord, activityName);
        }

        public async Task<FeedbackDto> AddFeedbackAsync(int activityRecordId, int instructorId, InstructorFeedbackDto dto)
        {
            var activityRecordExists = await _uow.ActivityRecordRepository
                .GetAllAsQueryable()
                .AnyAsync(ar => ar.Id == activityRecordId);

            if (!activityRecordExists)
                throw new KeyNotFoundException("Activity record not found.");

            if (string.IsNullOrWhiteSpace(dto.FeedbackText))
                throw new ArgumentException("Feedback text cannot be empty.");

            var newFeedback = new InstructorFeedback
            {
                ActivityRecordId = activityRecordId,
                InstructorId = instructorId,
                FeedbackText = dto.FeedbackText,
                CreatedDate = DateTime.UtcNow
            };

            await _uow.InstructorFeedbackRepository.CreateAsync(newFeedback);
            await _uow.SaveChangesAsync();

            return MapToFeedbackDto(newFeedback);
        }

        public async Task<IEnumerable<FeedbackDto>> GetFeedbacksAsync(int activityRecordId)
        {
            var activityRecordExists = await _uow.ActivityRecordRepository
                .GetAllAsQueryable()
                .AnyAsync(ar => ar.Id == activityRecordId);

            if (!activityRecordExists)
                throw new KeyNotFoundException("Activity record not found.");

            var feedbacks = await _uow.InstructorFeedbackRepository
                .GetAllAsQueryable()
                .AsNoTracking()
                .Where(f => f.ActivityRecordId == activityRecordId)
                .OrderByDescending(f => f.CreatedDate)
                .ToListAsync();

            return feedbacks.Select(MapToFeedbackDto);
        }


        private IQueryable<ActivityRecord> GetActivityRecordQuery()
        {
            return _uow.ActivityRecordRepository
                .GetAllAsQueryable()
                .AsNoTracking()
                .Include(ar => ar.SectionRecord)
                    .ThenInclude(sr => sr.LearningProgress)
                        .ThenInclude(lp => lp.Enrollment)
                            .ThenInclude(e => e.Trainee)
                                .ThenInclude(t => t.IdNavigation);
        }

        private static ActivityRecordDto MapToDto(ActivityRecord ar, string activityName)
        {
            string status = ar.Status.HasValue
                ? Enum.GetName(typeof(ActivityRecordStatusEnum), ar.Status.Value) ?? "NotStarted"
                : "NotStarted";

            string type = ar.ActivityType.HasValue
                ? Enum.GetName(typeof(ActivityType), ar.ActivityType.Value) ?? "Material"
                : "Material";

            return new ActivityRecordDto
            {
                Id = ar.Id,
                SectionRecordId = ar.SectionRecordId,
                ActivityId = ar.ActivityId,
                ActivityName = activityName, // <-- ADDED
                Status = status,
                Score = ar.Score,
                IsCompleted = ar.IsCompleted,
                CompletedDate = ar.CompletedDate,
                ActivityType = type,
                LearningProgressId = ar.SectionRecord.LearningProgressId,
                SectionId = ar.SectionRecord.SectionId,
                TraineeId = ar.SectionRecord.LearningProgress.Enrollment.TraineeId,
                TraineeName = ar.SectionRecord.LearningProgress.Enrollment.Trainee.IdNavigation.Fullname ?? "N/A",
                ClassId = ar.SectionRecord.LearningProgress.Enrollment.ClassId
            };
        }
        private static FeedbackDto MapToFeedbackDto(InstructorFeedback f)
        {
            return new FeedbackDto
            {
                Id = f.Id,
                ActivityRecordId = f.ActivityRecordId,
                InstructorId = f.InstructorId,
                FeedbackText = f.FeedbackText,
                CreatedDate = f.CreatedDate
            };
        }
    }
}
