using Lssctc.LearningManagement.SectionPartition.DTOs;
using Lssctc.Share.Common;

namespace Lssctc.LearningManagement.SectionPartition.Services
{
    public interface ISectionPartitionService
    {
        Task<PagedResult<SectionPartitionDto>> GetSectionPartitionsPaged(int pageIndex, int pageSize);

        Task<SectionPartitionDto?> GetSectionPartitionById(int id);

        Task<int> CreateSectionPartition(CreateSectionPartitionDto dto);

        Task<bool> UpdateSectionPartition(int id, UpdateSectionPartitionDto dto);

        Task<bool> DeleteSectionPartition(int id);

    }

}
