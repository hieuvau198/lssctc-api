using Lssctc.LearningManagement.SectionPartition.DTOs;
using Lssctc.Share.Common;

namespace Lssctc.LearningManagement.SectionPartition.Services
{
    public interface ISectionPartitionService
    {
        Task<PagedResult<SectionPartitionDto>> GetPagedAsync(
        int pageIndex, int pageSize, int? sectionId, int? partitionTypeId, string? search);

        Task<SectionPartitionDto?> GetByIdAsync(int id);

        Task<int> CreateAsync(CreateSectionPartitionDto dto);

        Task<bool> UpdateAsync(int id, UpdateSectionPartitionDto dto);

        Task<bool> DeleteAsync(int id);

    }

}
