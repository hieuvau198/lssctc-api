using Lssctc.ProgramManagement.BrandModel.DTOs;
using Lssctc.Share.Common;

namespace Lssctc.ProgramManagement.BrandModel.Services
{
    public interface IBrandModel
    {
        Task<PagedResult<SimulationComponentDto>> GetSimulationComponentsByBrandModelIdAsync(
            int brandModelId,
            int page,
            int pageSize,
            CancellationToken cancellationToken = default);
    }
}
