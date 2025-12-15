using Lssctc.ProgramManagement.Activities.Dtos;
using Lssctc.Share.Common;

namespace Lssctc.ProgramManagement.Activities.Services
{
    public interface IActivitiesService
    {
        #region Activities
        Task<IEnumerable<ActivityDto>> GetAllActivitiesAsync();
        Task<PagedResult<ActivityDto>> GetActivitiesAsync(int pageNumber, int pageSize);
        Task<ActivityDto?> GetActivityByIdAsync(int id);
        Task<ActivityDto> CreateActivityAsync(CreateActivityDto createDto);
        Task<ActivityDto> UpdateActivityAsync(int id, UpdateActivityDto updateDto);
        Task DeleteActivityAsync(int id);
        #endregion

        #region Section Activities
        Task<IEnumerable<ActivityDto>> GetActivitiesBySectionIdAsync(int sectionId);
        Task<ActivityDto> CreateActivityForSectionAsync(int sectionId, CreateActivityDto createDto);
        Task AddActivityToSectionAsync(int sectionId, int activityId);
        Task RemoveActivityFromSectionAsync(int sectionId, int activityId);
        Task UpdateSectionActivityOrderAsync(int sectionId, int activityId, int newOrder);
        #endregion

    }
}
