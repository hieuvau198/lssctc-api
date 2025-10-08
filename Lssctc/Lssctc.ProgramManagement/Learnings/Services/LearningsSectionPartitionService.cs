using Lssctc.ProgramManagement.Learnings.Dtos;
using Lssctc.Share.Common;
using Lssctc.Share.Interfaces;

namespace Lssctc.ProgramManagement.Learnings.Services
{
    public class LearningsSectionPartitionService : ILearningsSectionPartitionService
    {
        private readonly IUnitOfWork _unitOfWork;
        public LearningsSectionPartitionService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        public async Task<List<LearningsSectionPartitionDto>> GetAllSectionPartitionsBySectionIdAndTraineeId(int sectionId, int traineeId)
        {
            throw new NotImplementedException();
        }

        public async Task<LearningsSectionPartitionDto> GetSectionPartitionByPartitionIdAndTraineeId(int partitionId, int traineeId)
        {
            throw new NotImplementedException();
        }

        public async Task<PagedResult<LearningsSectionPartitionDto>> GetSectionPartitionsBySectionIdAndTraineeIdPaged(int sectionId, int traineeId, int pageIndex, int pageSize)
        {
            throw new NotImplementedException();
        }
    }
}
