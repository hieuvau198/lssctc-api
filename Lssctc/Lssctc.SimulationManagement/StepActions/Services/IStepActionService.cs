using Lssctc.SimulationManagement.StepActions.Dtos;

namespace Lssctc.SimulationManagement.StepActions.Services
{
    public interface IStepActionService
    {
        Task<List<StepActionDto>> GetAllStepActionsAsync();
        Task<StepActionDto?> GetStepActionByStepId(int stepId);
        Task<StepActionDto?> GetStepActionByIdAsync(int id);
        Task<StepActionDto> CreateStepActionAsync(CreateStepActionDto dto);
        Task<StepActionDto?> UpdateStepActionAsync(int id, UpdateStepActionDto dto);
        Task<bool> DeleteStepActionAsync(int id);
    }
}
