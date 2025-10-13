using Lssctc.SimulationManagement.StepComponents.Dtos;

namespace Lssctc.SimulationManagement.StepComponents.Services
{
    public interface IPracticeStepComponentService
    {
        Task<List<PracticeStepComponentDto>> GetByPracticeStepIdAsync(int practiceStepId);
        Task<PracticeStepComponentDto> AssignSimulationComponentAsync(CreatePracticeStepComponentDto dto);
        Task<PracticeStepComponentDto?> UpdateOrderAsync(int id, UpdatePracticeStepComponentDto dto);
        Task<bool> RemoveAsync(int id);
    }

}
