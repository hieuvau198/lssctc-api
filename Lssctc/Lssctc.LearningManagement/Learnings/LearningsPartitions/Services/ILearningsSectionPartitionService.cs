using Lssctc.LearningManagement.Learnings.LearningsPartitions.Dtos;
using Lssctc.Share.Common;

namespace Lssctc.LearningManagement.Learnings.LearningsPartitions.Services
{
    public interface ILearningsSectionPartitionService
    {
        // get list all of LearningsSectionPartitionDto by sectionId and traineeId
        Task<List<LearningsSectionPartitionDto>> GetAllSectionPartitionsBySectionIdAndTraineeId(int sectionId, int traineeId);
        // get paged list of LearningsSectionPartitionDto by sectionId and traineeId
        Task<PagedResult<LearningsSectionPartitionDto>> GetSectionPartitionsBySectionIdAndTraineeIdPaged(int sectionId, int traineeId, int pageIndex, int pageSize);
        // get LearningsSectionPartitionDto by partitionId and traineeId
        Task<LearningsSectionPartitionDto> GetSectionPartitionByPartitionIdAndTraineeId(int partitionId, int traineeId);
    }
}
