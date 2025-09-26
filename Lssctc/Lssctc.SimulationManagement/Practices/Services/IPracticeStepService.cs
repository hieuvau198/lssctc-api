using Lssctc.SimulationManagement.Practices.Dtos;

namespace Lssctc.SimulationManagement.Practices.Services
{
    public interface IPracticeStepService
    {
        Task<List<PracticeStepDto>> GetPracticeStepsByPracticeIdAsync(int practiceId);
        Task<PracticeStepDto?> GetPracticeStepByIdAsync(int id);
        Task<PracticeStepDto> CreatePracticeStepAsync(CreatePracticeStepDto dto);
        Task<PracticeStepDto?> UpdatePracticeStepAsync(int id, UpdatePracticeStepDto dto);
        Task<bool> DeletePracticeStepAsync(int id);
    }
}
