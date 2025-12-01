using Lssctc.ProgramManagement.Materials.Dtos;
using Lssctc.Share.Common;

namespace Lssctc.ProgramManagement.Materials.Services
{
    public interface IMaterialsService
    {
        #region Learning Materials
        Task<IEnumerable<MaterialDto>> GetAllMaterialsAsync();
        Task<PagedResult<MaterialDto>> GetMaterialsAsync(int pageNumber, int pageSize);
        Task<PagedResult<MaterialDto>> GetMaterialsAsync(int pageNumber, int pageSize, int? instructorId);
        Task<MaterialDto?> GetMaterialByIdAsync(int id);
        Task<MaterialDto?> GetMaterialByIdAsync(int id, int? instructorId);
        Task<MaterialDto> CreateMaterialAsync(CreateMaterialDto createDto);
        Task<MaterialDto> CreateMaterialAsync(CreateMaterialDto createDto, int instructorId);
        Task<MaterialDto> UpdateMaterialAsync(int id, UpdateMaterialDto updateDto);
        Task<MaterialDto> UpdateMaterialAsync(int id, UpdateMaterialDto updateDto, int? instructorId);
        Task DeleteMaterialAsync(int id, int? instructorId);
        #endregion

        #region Activity Materials
        Task<IEnumerable<ActivityMaterialDto>> GetMaterialsByActivityAsync(int activityId);
        Task AddMaterialToActivityAsync(int activityId, int materialId);
        Task RemoveMaterialFromActivityAsync(int activityId, int materialId);
        #endregion
    }
}
