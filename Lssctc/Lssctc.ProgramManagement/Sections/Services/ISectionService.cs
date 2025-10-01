using Lssctc.ProgramManagement.Sections.DTOs;
using Lssctc.Share.Common;

namespace Lssctc.ProgramManagement.Sections.Services
{
    public interface ISectionService
    {
        // get all without pagination
        Task<PagedResult<SectionListItemDto>> GetSections(SectionQueryParameters parameters);

        Task<SectionDto?> GetSectionById(int id);
        Task<int> CreateSection(CreateSectionDto dto);
        Task<bool> UpdateSection(int id, UpdateSectionDto dto);
        Task<bool> DeleteSectionById(int id);
    }
}
