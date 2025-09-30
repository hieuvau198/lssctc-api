using Lssctc.ProgramManagement.SectionMaterials.DTOs;
using Lssctc.Share.Common;

namespace Lssctc.ProgramManagement.SectionMaterials.Services
{
    public interface ISectionMaterialService
    {
        Task<PagedResult<SectionMaterialDto>> GetSectionMaterialsPaged(int pageIndex, int pageSize);
        Task<SectionMaterialDto?> GetSectionMateriaById(int id);
        Task<int> CreateSectionMateria(CreateSectionMaterialDto dto);
        Task<bool> UpdateSectionMateria(int id, UpdateSectionMaterialDto dto);
        Task<bool> DeleteSectionMateria(int id);
    }
}
