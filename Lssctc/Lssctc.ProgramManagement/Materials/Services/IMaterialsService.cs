using Lssctc.ProgramManagement.Materials.Dtos;
using Lssctc.Share.Common;

namespace Lssctc.ProgramManagement.Materials.Services
{
    public interface IMaterialsService
    {
        #region Learning Materials
        Task<IEnumerable<MaterialDto>> GetAllMaterialsAsync();
        Task<PagedResult<MaterialDto>> GetMaterialsAsync(int pageNumber, int pageSize);
        Task<MaterialDto?> GetMaterialByIdAsync(int id);
        Task<MaterialDto> CreateMaterialAsync(CreateMaterialDto createDto);
        Task<MaterialDto> UpdateMaterialAsync(int id, UpdateMaterialDto updateDto);
        Task DeleteMaterialAsync(int id);
        #endregion

        #region Activity Materials
        Task<IEnumerable<ActivityMaterialDto>> GetMaterialsByActivityAsync(int activityId);
        Task AddMaterialToActivityAsync(int activityId, int materialId);
        Task RemoveMaterialFromActivityAsync(int activityId, int materialId);
        #endregion
    }
}
