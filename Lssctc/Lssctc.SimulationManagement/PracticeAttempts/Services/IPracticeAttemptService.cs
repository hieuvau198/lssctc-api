using Lssctc.SimulationManagement.PracticeAttempts.Dtos;

namespace Lssctc.SimulationManagement.PracticeAttempts.Services
{
    public interface IPracticeAttemptService
    {
        // get practice attempts by practice id and trainee id
        Task<List<PracticeAttemptDto>> GetPracticeAttemptsByPracticeIdAndTraineeId(int sectionPracticeId, int traineeId);
        // get practice attempt by attempt id
        Task<PracticeAttemptDto?> GetPracticeAttemptById(int attemptId);
        // create practice attempt by practice id and trainee id
        Task<PracticeAttemptDto> CreatePracticeAttempt(int sectionPracticeId, int traineeId);
        // delete practice attempt
        Task<bool> DeletePracticeAttempt(int attemptId);
        // confirm practice attempt is complete
        Task<PracticeAttemptDto> ConfirmPracticeAttemptComplete(int attemptId);
    }
}
