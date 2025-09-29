using Lssctc.Share.Common;
using Lssctc.SimulationManagement.SectionPractice.Dtos;

namespace Lssctc.SimulationManagement.SectionPractice.Services
{
    public interface ISectionPracticeService
    {
        Task<PagedResult<SectionPracticeDto>> GetSectionPracticesPaged(int pageIndex, int pageSize);
        Task<SectionPracticeDto?> GetSectionPracticeById(int id);
        Task<int> CreateSectionPractice(CreateSectionPracticeDto dto);
        Task<bool> UpdateSectionPractice(int id, UpdateSectionPracticeDto dto);
        Task<bool> DeleteSectionPractice(int id);
    }
}
