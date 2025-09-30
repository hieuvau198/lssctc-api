using Lssctc.LearningManagement.SectionMaterial.DTOs;

namespace Lssctc.LearningManagement.SectionMaterial.Services
{
    public interface ISectionMaterialService
    {
        Task<(IReadOnlyList<SectionMaterialDto> Items, int Total)> GetSectionMaterialsPaged(
    int pageIndex, int pageSize);
        Task<SectionMaterialDto?> GetSectionMateriaById(int id);
        Task<int> CreateSectionMateria(CreateSectionMaterialDto dto);
        Task<bool> UpdateSectionMateria(int id, UpdateSectionMaterialDto dto);
        Task<bool> DeleteSectionMateria(int id);
    }
}
