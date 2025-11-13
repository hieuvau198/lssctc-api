using Lssctc.ProgramManagement.ClassManage.PracticeAttempts.Dtos;
using Lssctc.Share.Common;

namespace Lssctc.ProgramManagement.ClassManage.PracticeAttempts.Services
{
    public interface IPracticeAttemptsService
    {
        // Get all practice attempts by trainee id and activity record id
        Task<IEnumerable<PracticeAttemptDto>> GetPracticeAttempts(int traineeId, int activityRecordId);
        
        // Get latest practice attempt by trainee id and activity record id
        Task<PracticeAttemptDto?> GetLatestPracticeAttempt(int traineeId, int activityRecordId);
        
        // Get practice attempts by trainee id and practice id
        Task<IEnumerable<PracticeAttemptDto>> GetPracticeAttemptsByPractice(int traineeId, int practiceId);
        
        // Get practice attempt by practice attempt id
        Task<PracticeAttemptDto?> GetPracticeAttemptById(int practiceAttemptId);

        // Get practice attempts with paging by trainee id and activity record id
        Task<PagedResult<PracticeAttemptDto>> GetPracticeAttemptsPaged(int traineeId, int activityRecordId, int pageNumber, int pageSize);

        // Get practice attempts with paging by trainee id and practice id
        Task<PagedResult<PracticeAttemptDto>> GetPracticeAttemptsByPracticePaged(int traineeId, int practiceId, int pageNumber, int pageSize);

        // Create a new practice attempt with tasks
        Task<PracticeAttemptDto> CreatePracticeAttempt(int traineeId, CreatePracticeAttemptDto createDto);
    }
}
