using Lssctc.ProgramManagement.Activities.Dtos;
using Lssctc.Share.Common;
using Lssctc.Share.Entities;
using Lssctc.Share.Enums;
using Lssctc.Share.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Lssctc.ProgramManagement.Activities.Services
{
    public class ActivitiesService : IActivitiesService
    {
        private IUnitOfWork _uow;
        public ActivitiesService(IUnitOfWork uow)
        {
            _uow = uow;
        }

        #region Activities
        public async Task<IEnumerable<ActivityDto>> GetAllActivitiesAsync()
        {
            var activities = await _uow.ActivityRepository
                .GetAllAsQueryable()
                .Where(a => a.IsDeleted != true)
                .ToListAsync();

            return activities.Select(MapToDto);
        }

        public async Task<PagedResult<ActivityDto>> GetActivitiesAsync(int pageNumber, int pageSize)
        {
            if (pageNumber < 1) pageNumber = 1;
            if (pageSize < 1) pageSize = 10;

            var query = _uow.ActivityRepository
                .GetAllAsQueryable()
                .Where(a => a.IsDeleted != true)
                .Select(a => MapToDto(a));

            var pagedResult = await query.ToPagedResultAsync(pageNumber, pageSize);
            return pagedResult;
        }

        public async Task<ActivityDto?> GetActivityByIdAsync(int id)
        {
            var activity = await _uow.ActivityRepository
                .GetAllAsQueryable()
                .Where(a => a.Id == id && a.IsDeleted != true)
                .FirstOrDefaultAsync();

            return activity == null ? null : MapToDto(activity);
        }

        public async Task<ActivityDto> CreateActivityAsync(CreateActivityDto createDto)
        {
            var activity = new Activity
            {
                ActivityTitle = createDto.ActivityTitle!.Trim(),
                ActivityDescription = string.IsNullOrWhiteSpace(createDto.ActivityDescription)
                    ? null
                    : createDto.ActivityDescription.Trim(),
                ActivityType = ParseActivityType(createDto.ActivityType),
                EstimatedDurationMinutes = createDto.EstimatedDurationMinutes,
                IsDeleted = false
            };

            await _uow.ActivityRepository.CreateAsync(activity);
            await _uow.SaveChangesAsync();

            return MapToDto(activity);
        }

        public async Task<ActivityDto> UpdateActivityAsync(int id, UpdateActivityDto updateDto)
        {
            var activity = await _uow.ActivityRepository.GetByIdAsync(id);
            if (activity == null || activity.IsDeleted == true)
            {
                throw new KeyNotFoundException($"Activity with ID {id} not found.");
            }

            activity.ActivityTitle = updateDto.ActivityTitle!.Trim();
            activity.ActivityDescription = string.IsNullOrWhiteSpace(updateDto.ActivityDescription)
                ? null
                : updateDto.ActivityDescription.Trim();
            activity.ActivityType = ParseActivityType(updateDto.ActivityType);
            activity.EstimatedDurationMinutes = updateDto.EstimatedDurationMinutes;

            await _uow.ActivityRepository.UpdateAsync(activity);
            await _uow.SaveChangesAsync();

            return MapToDto(activity);
        }



        public async Task DeleteActivityAsync(int id)
        {
            var activity = await _uow.ActivityRepository
                .GetAllAsQueryable()
                .Where(a => a.Id == id)
                .Include(a => a.SectionActivities)
                .FirstOrDefaultAsync();

            if (activity == null || activity.IsDeleted == true)
            {
                throw new KeyNotFoundException($"Activity with ID {id} not found.");
            }

            if (activity.SectionActivities != null && activity.SectionActivities.Any())
            {
                throw new InvalidOperationException("Cannot delete activity associated with sections.");
            }

            activity.IsDeleted = true;
            await _uow.ActivityRepository.UpdateAsync(activity);
            await _uow.SaveChangesAsync();
        }
        #endregion

        #region Mapping
        private static ActivityDto MapToDto(Activity a)
        {
            return new ActivityDto
            {
                Id = a.Id,
                ActivityTitle = a.ActivityTitle,
                ActivityDescription = a.ActivityDescription,
                ActivityType = a.ActivityType.HasValue
                    ? Enum.GetName(typeof(ActivityType), a.ActivityType.Value)
                    : null,
                EstimatedDurationMinutes = a.EstimatedDurationMinutes
            };
        }

        private static int? ParseActivityType(string? activityType)
        {
            if (string.IsNullOrWhiteSpace(activityType)) return null;

            if (Enum.TryParse(typeof(ActivityType), activityType, true, out var parsed))
            {
                return (int)(ActivityType)parsed!;
            }

            throw new ArgumentException($"Invalid ActivityType value: {activityType}");
        }
        #endregion

    }
}
