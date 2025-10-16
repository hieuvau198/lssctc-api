using Lssctc.ProgramManagement.SectionPartitions.DTOs;
using Lssctc.Share.Common;

namespace Lssctc.ProgramManagement.SectionPartitions.Services
{
    public interface ISectionPartitionService
    {
        Task<IReadOnlyList<SectionPartitionDto>> GetSectionPartitionsNoPagination();
        Task<PagedResult<SectionPartitionDto>> GetSectionPartitionsPaged(int pageIndex, int pageSize);

        // get by sectionId with pagination
        Task<PagedResult<SectionPartitionDto>> GetSectionPartitionBySectionId(int sectionId, int page, int pageSize);

        Task<SectionPartitionDto?> GetSectionPartitionById(int id);

        Task<int> CreateSectionPartition(CreateSectionPartitionDto dto);

        Task<bool> UpdateSectionPartition(int id, UpdateSectionPartitionDto dto);

        Task<bool> DeleteSectionPartition(int id);

        Task<SectionPartitionDto> AssignSectionPartition(AssignSectionPartitionDto dto);
    }
}
