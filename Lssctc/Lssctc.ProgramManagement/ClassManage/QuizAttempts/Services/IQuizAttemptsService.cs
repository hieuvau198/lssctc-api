using Lssctc.ProgramManagement.ClassManage.QuizAttempts.Dtos;

namespace Lssctc.ProgramManagement.ClassManage.QuizAttempts.Services
{
    public interface IQuizAttemptsService
    {
        // Logic: lasted submitted attempt is the new valid one, using IsCurrent field to mark it   
        // Logic: when submit new attempt, mark all previous attempts IsCurrent = false, and new one IsCurrent = true

        Task<IEnumerable<QuizAttemptDto>> GetQuizAttemptsAsync(int traineeId, int activityRecordId);
        // QAS1: Get all quiz attempts by trainee id, activity record id
        Task<QuizAttemptDto?> GetLatestQuizAttemptAsync(int traineeId, int activityRecordId);
        // QAS2: Get latest quiz attempt by trainee id, activity record id
        Task<IEnumerable<QuizAttemptDto>> GetLatestAttemptsForActivityAsync(int activityId);
        // QAS3: Get all quiz (one lastest for each trainee inside that class) attempts by activity record id

        Task<QuizAttemptDto> SubmitQuizAttemptAsync(int traineeId, SubmitQuizDto dto);
        // QAS-4: submit quiz attempt by trainee id, activity record id,
        // input will be a a quiz that contains list of answers
        // we will create QuizAttempt, QuizAttemptQuestion, QuizAttemptAnswer for this
        // we will call M3 to update the progress

    }
}
