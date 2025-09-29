using Lssctc.LearningManagement.Section.DTOs;
using Lssctc.Share.Common;

namespace Lssctc.LearningManagement.Section.Services
{
    public interface ISectionService
    {
        Task<PagedResult<SectionListItemDto>> GetSections(SectionQueryParameters parameters);

        Task<SectionDto?> GetSectionById(int id);
        Task<int> CreateSection(CreateSectionDto dto);
        Task<bool> UpdateSection(int id, UpdateSectionDto dto);
        Task<bool> DeleteSectionById(int id);
    }
}
