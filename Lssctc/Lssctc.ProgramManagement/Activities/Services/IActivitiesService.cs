using Lssctc.ProgramManagement.Activities.Dtos;
using Lssctc.Share.Common;

namespace Lssctc.ProgramManagement.Activities.Services
{
    public interface IActivitiesService
    {
        #region Activities
        // get all activities
        // get activities paged
        // get activity by id
        // create activity
        // update activity
        // delete activity
        Task<IEnumerable<ActivityDto>> GetAllActivitiesAsync();
        Task<PagedResult<ActivityDto>> GetActivitiesAsync(int pageNumber, int pageSize);
        Task<ActivityDto?> GetActivityByIdAsync(int id);
        Task<ActivityDto> CreateActivityAsync(CreateActivityDto createDto);
        Task<ActivityDto> UpdateActivityAsync(int id, UpdateActivityDto updateDto);
        Task DeleteActivityAsync(int id);
        #endregion

        #region Section Activities
        // get activities by section id
        // add activity to section
        // remove activity from section
        // update section activity
        #endregion
    }
}
