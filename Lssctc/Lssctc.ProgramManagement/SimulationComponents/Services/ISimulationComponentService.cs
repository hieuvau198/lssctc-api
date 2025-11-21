using Lssctc.ProgramManagement.SimulationComponents.DTOs;
using Lssctc.Share.Common;

namespace Lssctc.ProgramManagement.SimulationComponents.Services
{
    public interface ISimulationComponentService
    {
        Task<SimulationComponentDto?> GetSimulationComponentById(int id, CancellationToken cancellationToken = default);
        Task<SimulationComponentDto?> GetSimulationComponentByCode(string code, CancellationToken cancellationToken = default);
        Task<PagedResult<SimulationComponentDto>> GetSimulationComponentsByBrandModelId(
            int brandModelId, 
            int page, 
            int pageSize, 
            CancellationToken cancellationToken = default);
        Task<PagedResult<SimulationComponentDto>> GetAllSimulationComponents(
            int page, 
            int pageSize, 
            CancellationToken cancellationToken = default);
        Task<int> CreateSimulationComponent(CreateSimulationComponentDto dto, CancellationToken cancellationToken = default);
        Task<bool> UpdateSimulationComponent(int id, UpdateSimulationComponentDto dto, CancellationToken cancellationToken = default);
        Task<bool> DeleteSimulationComponent(int id, CancellationToken cancellationToken = default);
    }
}
