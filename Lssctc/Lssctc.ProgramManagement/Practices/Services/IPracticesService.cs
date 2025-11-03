using Lssctc.ProgramManagement.Practices.Dtos;
using Lssctc.Share.Common;
using Lssctc.Share.Entities;

namespace Lssctc.ProgramManagement.Practices.Services
{
    public interface IPracticesService
    {
        #region Practices
        Task<IEnumerable<PracticeDto>> GetAllPracticesAsync();
        Task<PagedResult<PracticeDto>> GetPracticesAsync(int pageNumber, int pageSize);
        Task<PracticeDto?> GetPracticeByIdAsync(int id);
        Task<PracticeDto> CreatePracticeAsync(CreatePracticeDto createDto);
        Task<PracticeDto> UpdatePracticeAsync(int id, UpdatePracticeDto updateDto);
        Task DeletePracticeAsync(int id);
        #endregion

        #region Activity Practices
        // business rules: one practice can belong to many activities, one activity can have only one practice (if activity has quiz or material, it can not have practice)
        Task<IEnumerable<PracticeDto>> GetPracticesByActivityAsync(int activityId);
        Task AddPracticeToActivityAsync(int activityId, int practiceId);
        Task RemovePracticeFromActivityAsync(int activityId, int practiceId);
        #endregion
    }
}
