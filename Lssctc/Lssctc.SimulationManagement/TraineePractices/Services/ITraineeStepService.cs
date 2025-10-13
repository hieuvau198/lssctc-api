using Lssctc.SimulationManagement.TraineePractices.Dtos;

namespace Lssctc.SimulationManagement.TraineePractices.Services
{
    public interface ITraineeStepService
    {
        // get steps by practice id and trainee id
        Task<List<TraineeStepDto>> GetTraineeStepsByPracticeIdAndTraineeId(int practiceId, int traineeId);
        // get step by step id and trainee id
        Task<TraineeStepDto?> GetTraineeStepByIdAndTraineeId(int stepId, int traineeId);
        // submit trainee step attempt
        Task<bool> SubmitTraineeStepAttempt(int attemptId, int traineeId, UpdateTraineeStepAttemptDto input);
    }
}
