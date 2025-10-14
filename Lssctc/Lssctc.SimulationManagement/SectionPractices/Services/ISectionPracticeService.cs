using Lssctc.Share.Common;
using Lssctc.SimulationManagement.SectionPractices.Dtos;

namespace Lssctc.SimulationManagement.SectionPractices.Services
{
    public interface ISectionPracticeService
    {
        Task<PagedResult<SectionPracticeDto>> GetSectionPracticesPaged(int pageIndex, int pageSize);
        Task<SectionPracticeDto?> GetSectionPracticeById(int id);
       Task<PagedResult<SectionPracticeListDto>> GetSectionPracticesByClassId(int classId, int page, int pageSize);
        Task<int> CreateSectionPractice(CreateSectionPracticeDto dto);
        Task<bool> UpdateSectionPractice(int id, UpdateSectionPracticeDto dto);
        Task<bool> DeleteSectionPractice(int id);
    }
}
