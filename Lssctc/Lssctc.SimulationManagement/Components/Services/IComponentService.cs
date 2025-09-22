using Lssctc.Share.Common;
using Lssctc.SimulationManagement.Components.Dtos;

namespace Lssctc.SimulationManagement.Components.Services
{
    public interface IComponentService
    {
        Task<PagedResult<SimulationComponentDto>> GetAllAsync(SimulationComponentQueryDto query);
        Task<SimulationComponentDto?> GetByIdAsync(int id);
        Task<SimulationComponentDto> CreateAsync(CreateSimulationComponentDto dto);
        Task<SimulationComponentDto?> UpdateAsync(int id, UpdateSimulationComponentDto dto);
        Task<bool> DeleteAsync(int id);
        Task<bool> ExistsAsync(int id);
        Task<bool> ExistsAsync(string name, int? excludeId = null);
    }
}
