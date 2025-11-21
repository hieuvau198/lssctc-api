using Lssctc.ProgramManagement.ClassManage.PracticeAttempts.Dtos;
using Lssctc.Share.Common;

namespace Lssctc.ProgramManagement.ClassManage.PracticeAttempts.Services
{
    public interface IPracticeAttemptsService
    {
        Task<IEnumerable<PracticeAttemptDto>> GetPracticeAttempts(int traineeId, int activityRecordId);
        Task<PracticeAttemptDto?> GetLatestPracticeAttempt(int traineeId, int activityRecordId);
        Task<IEnumerable<PracticeAttemptDto>> GetPracticeAttemptsByPractice(int traineeId, int practiceId);
        Task<PracticeAttemptDto?> GetPracticeAttemptById(int practiceAttemptId);
        Task<PagedResult<PracticeAttemptDto>> GetPracticeAttemptsPaged(int traineeId, int activityRecordId, int pageNumber, int pageSize);
        Task<PagedResult<PracticeAttemptDto>> GetPracticeAttemptsByPracticePaged(int traineeId, int practiceId, int pageNumber, int pageSize);
        Task<PracticeAttemptDto> CreatePracticeAttempt(int traineeId, CreatePracticeAttemptDto createDto);
        Task<PracticeAttemptDto> CreatePracticeAttemptByCode(int traineeId, CreatePracticeAttemptWithCodeDto createDto);

    }
}
