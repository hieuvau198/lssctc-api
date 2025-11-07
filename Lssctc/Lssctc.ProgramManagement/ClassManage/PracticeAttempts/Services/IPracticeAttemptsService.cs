using Lssctc.ProgramManagement.ClassManage.PracticeAttempts.Dtos;

namespace Lssctc.ProgramManagement.ClassManage.PracticeAttempts.Services
{
    public interface IPracticeAttemptsService
    {
        // Get all practice attempts by trainee id and activity record id
        Task<IEnumerable<PracticeAttemptDto>> GetPracticeAttempts(int traineeId, int activityRecordId);
        
        // Get latest practice attempt by trainee id and activity record id
        Task<PracticeAttemptDto?> GetLatestPracticeAttempt(int traineeId, int activityRecordId);
        
        // Get practice attempts by trainee id and practice id
        // Get practice attempts by practice attempt id
    }
}
