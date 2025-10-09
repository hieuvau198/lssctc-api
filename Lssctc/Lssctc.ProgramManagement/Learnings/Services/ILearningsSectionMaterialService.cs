using Lssctc.ProgramManagement.Learnings.Dtos;

namespace Lssctc.ProgramManagement.Learnings.Services
{
    public interface ILearningsSectionMaterialService
    {
        // get LearningsSectionMaterialDto by partitionId and traineeId
        Task<LearningsSectionMaterialDto> GetSectionMaterialByPartitionIdAndTraineeId(int partitionId, int traineeId);
        // update record partition as completed by sectionMaterialId and traineeId
        Task<bool> UpdateLearningMaterialAsCompleted(int sectionMaterialId, int traineeId);
        // update record partition as not completed by sectionMaterialId and traineeId
        Task<bool> UpdateLearningMaterialAsNotCompleted(int sectionMaterialId, int traineeId);
    }
}
