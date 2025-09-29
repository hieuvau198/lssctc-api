using Lssctc.SimulationManagement.PracticeSteps.Dtos;

namespace Lssctc.SimulationManagement.PracticeSteps.Services
{
    public interface IPracticeStepService
    {
        // Get and CRUD PracticeStep
        Task<List<PracticeStepDto>> GetPracticeStepsByPracticeIdAsync(int practiceId);
        Task<PracticeStepDto?> GetPracticeStepByIdAsync(int id);
        Task<PracticeStepDto> CreatePracticeStepAsync(CreatePracticeStepDto dto);
        Task<PracticeStepDto?> UpdatePracticeStepAsync(int id, UpdatePracticeStepDto dto);
        Task<bool> DeletePracticeStepAsync(int id);

        // Get PracticeStepComponents by PracticeStepId
        // Assign SimulationComponent to PracticeStep, result mean PracticeStepComponent
        // Update ComponentOrder of PracticeStepComponent, up or down
        // Remove PracticeStepComponent
    }
}
