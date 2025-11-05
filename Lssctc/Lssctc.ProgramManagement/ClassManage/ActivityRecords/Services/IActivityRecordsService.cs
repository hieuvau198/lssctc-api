using Lssctc.ProgramManagement.ClassManage.ActivityRecords.Dtos;

namespace Lssctc.ProgramManagement.ClassManage.ActivityRecords.Services
{
    public interface IActivityRecordsService
    {
        // Trainee can submit activities (material, quiz, practice) which will update their section records
        // There are 3 types of activities: Material, Quiz, Practice.
        // If it is Quiz, submit with Quiz Attempt, if it is Practice, submit with Practice Attempt
        // instructor can add feedback for a trainee's activity record

        // view activity records of a trainee by class id, section id and trainee id
        // view activity records of many trainee by class id, section id and activity id
        // submit activity record for a trainee
        // add feedback for a trainee's activity record

        Task<IEnumerable<ActivityRecordDto>> GetActivityRecordsAsync(int classId, int sectionId, int traineeId);
        Task<IEnumerable<ActivityRecordDto>> GetActivityRecordsByActivityAsync(int classId, int sectionId, int activityId);
        Task<ActivityRecordDto> SubmitActivityAsync(int traineeId, SubmitActivityRecordDto dto);
        Task<FeedbackDto> AddFeedbackAsync(int activityRecordId, int instructorId, InstructorFeedbackDto dto);
        Task<IEnumerable<FeedbackDto>> GetFeedbacksAsync(int activityRecordId);
    }
}
