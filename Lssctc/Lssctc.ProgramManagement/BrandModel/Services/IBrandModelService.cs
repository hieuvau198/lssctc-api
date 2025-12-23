using Lssctc.ProgramManagement.BrandModel.DTOs;
using Lssctc.Share.Common;

namespace Lssctc.ProgramManagement.BrandModel.Services
{
    public interface IBrandModelService
    {
        Task<BrandModelDto?> GetBrandModelById(int id, CancellationToken cancellationToken = default);
        Task<PagedResult<BrandModelDto>> GetAllBrandModels(int page, int pageSize, CancellationToken cancellationToken = default);
        Task<int> CreateBrandModel(CreateBrandModelDto dto, CancellationToken cancellationToken = default);
        Task<bool> UpdateBrandModel(int id, UpdateBrandModelDto dto, CancellationToken cancellationToken = default);
        Task<bool> DeleteBrandModel(int id, CancellationToken cancellationToken = default);
    }
}
