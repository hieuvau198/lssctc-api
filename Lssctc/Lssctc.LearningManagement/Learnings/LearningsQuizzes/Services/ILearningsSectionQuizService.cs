using Lssctc.LearningManagement.Learnings.LearningsQuizzes.Dtos;

namespace Lssctc.LearningManagement.Learnings.LearningsQuizzes.Services
{
    public interface ILearningsSectionQuizService
    {
        // get LearningsSectionQuizDto by partitionId and traineeId
        Task<LearningsSectionQuizDto> GetSectionQuizByPartitionIdAndTraineeId(int partitionId, int traineeId);
        // submit quiz answers by partitionId, traineeId and list of answers
        Task<LearningsSectionQuizDto> SubmitSectionQuizAttempt(int partitionId, int traineeId, CreateLearningsSectionQuizAttemptDto input);
    }
}
