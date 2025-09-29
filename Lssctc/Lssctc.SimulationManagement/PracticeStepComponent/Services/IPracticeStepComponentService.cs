using Lssctc.Share.Common;
using Lssctc.SimulationManagement.PracticeStepComponent.Dtos;

namespace Lssctc.SimulationManagement.PracticeStepComponent.Services
{
    public interface IPracticeStepComponentService
    {
        Task<PagedResult<PracticeStepComponentDto>> GetPagedAsync(
            int pageIndex, int pageSize, int? practiceStepId, int? simulationComponentId, string? search);

        Task<PracticeStepComponentDto?> GetByIdAsync(int id);

        Task<int> CreateAsync(CreatePracticeStepComponentDto dto);

        Task<bool> UpdateAsync(int id, UpdatePracticeStepComponentDto dto);

        Task<bool> DeleteAsync(int id);


    }
}
