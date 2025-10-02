using Lssctc.ProgramManagement.Classes.DTOs;
using Lssctc.Share.Common;

namespace Lssctc.ProgramManagement.TrainingResult.Services
{
    public interface ITrainingResultService
    {
        Task<PagedResult<TrainingResultDto>> GetTrainingResults(int pageIndex, int pageSize);
        Task<IReadOnlyList<TrainingResultDto>> GetTrainingResultsNoPagination();
        Task<TrainingResultDto?> GetTrainingResultById(int id);
        Task<int> CreateTrainingResult(CreateTrainingResultDto dto);
        Task<bool> UpdateTrainingResult(int id, UpdateTrainingResultDto dto);
        Task<bool> DeleteTrainingResultById(int id);
    }
}
