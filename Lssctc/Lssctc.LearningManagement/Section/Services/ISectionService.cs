using Lssctc.LearningManagement.Section.DTOs;
using Lssctc.Share.Common;

namespace Lssctc.LearningManagement.Section.Services
{
    public interface ISectionService
    {
        Task<PagedResult<SectionListItemDto>> GetSections(
        int pageIndex, int pageSize,
        int? classesId = null,
        int? syllabusSectionId = null,
        int? status = null,
        string? search = null);

        Task<SectionDto?> GetById(int id);
        Task<int> Create(CreateSectionDto dto);
        Task<bool> Update(int id, UpdateSectionDto dto);
        Task<bool> Delete(int id);
    }
}
