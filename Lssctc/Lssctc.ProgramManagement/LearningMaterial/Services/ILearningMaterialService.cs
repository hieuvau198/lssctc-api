using Lssctc.ProgramManagement.LearningMaterial.DTOs;
using Lssctc.Share.Common;

namespace Lssctc.ProgramManagement.LearningMaterial.Services
{
    public interface ILearningMaterialService
    {
        Task<PagedResult<LearningMaterialDto>> GetLearningMaterialsPagination(int pageIndex, int pageSize);
        Task<IReadOnlyList<LearningMaterialDto>> GetLearningMaterialsNoPagination();

        Task<LearningMaterialDto?> GetLearningMaterialById(int id);
        Task<int> CreateLearningMaterial(CreateLearningMaterialDto dto);
        Task<bool> UpdateLearningMaterial(int id, UpdateLearningMaterialDto dto);
        Task<bool> DeleteLearningMaterial(int id);
    }
}
