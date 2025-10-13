using Lssctc.SimulationManagement.SimActions.Dtos;

namespace Lssctc.SimulationManagement.SimActions.Services
{
    public interface ISimActionService 
    {
        Task<List<SimActionDto>> GetAllSimActionsAsync();
        Task<SimActionDto?> GetSimActionByIdAsync(int id);
        Task<SimActionDto> CreateSimActionAsync(CreateSimActionDto dto);
        Task<SimActionDto?> UpdateSimActionAsync(int id, UpdateSimActionDto dto);
        Task<bool> DeleteSimActionAsync(int id);
    }
}
