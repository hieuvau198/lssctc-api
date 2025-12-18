using Lssctc.ProgramManagement.Practices.Dtos;
using Lssctc.Share.Common;

namespace Lssctc.ProgramManagement.Practices.Services
{
    public interface IPracticesService
    {
        Task<IEnumerable<PracticeDto>> GetAllPracticesAsync();
        Task<PagedResult<PracticeDto>> GetPracticesAsync(int pageNumber, int pageSize);
        Task<PracticeDto?> GetPracticeByIdAsync(int id);
        Task<PracticeDto> CreatePracticeAsync(CreatePracticeDto createDto);
        Task<PracticeDto> UpdatePracticeAsync(int id, UpdatePracticeDto updateDto);
        Task DeletePracticeAsync(int id);
        Task<IEnumerable<PracticeDto>> GetPracticesByActivityAsync(int activityId);
        Task AddPracticeToActivityAsync(int activityId, int practiceId);
        Task RemovePracticeFromActivityAsync(int activityId, int practiceId);
    }
}