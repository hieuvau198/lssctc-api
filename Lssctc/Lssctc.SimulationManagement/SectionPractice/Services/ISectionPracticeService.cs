using Lssctc.SimulationManagement.SectionPractice.Dtos;

namespace Lssctc.SimulationManagement.SectionPractice.Services
{
    public interface ISectionPracticeService
    {
        Task<(IReadOnlyList<SectionPracticeDto> Items, int Total)> GetPagedAsync(
             int pageIndex, int pageSize, int? sectionPartitionId, int? practiceId, int? status, string? search);
        Task<SectionPracticeDto?> GetByIdAsync(int id);
        Task<int> CreateAsync(CreateSectionPracticeDto dto);
        Task<bool> UpdateAsync(int id, UpdateSectionPracticeDto dto);
        Task<bool> DeleteAsync(int id);
    }
}
