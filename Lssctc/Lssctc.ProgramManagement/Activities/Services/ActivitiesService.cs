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

        #region Section Activities

        public async Task<IEnumerable<ActivityDto>> GetActivitiesBySectionIdAsync(int sectionId)
        {
            var sectionActivities = await _uow.SectionActivityRepository
                .GetAllAsQueryable()
                .Where(sa => sa.SectionId == sectionId && sa.Activity.IsDeleted != true)
                .Include(sa => sa.Activity)
                .OrderBy(sa => sa.ActivityOrder)
                .ToListAsync();

            return sectionActivities.Select(sa => MapToDto(sa.Activity));
        }

        public async Task AddActivityToSectionAsync(int sectionId, int activityId)
        {
            var section = await _uow.SectionRepository.GetByIdAsync(sectionId);
            if (section == null)
                throw new KeyNotFoundException($"Section with ID {sectionId} not found.");

            var activity = await _uow.ActivityRepository.GetByIdAsync(activityId);
            if (activity == null || activity.IsDeleted == true)
                throw new KeyNotFoundException($"Activity with ID {activityId} not found.");

            bool alreadyExists = await _uow.SectionActivityRepository
                .GetAllAsQueryable()
                .AnyAsync(sa => sa.SectionId == sectionId && sa.ActivityId == activityId);

            if (alreadyExists)
                throw new InvalidOperationException("This activity is already assigned to the section.");

            int newOrder = await GetNextAvailableOrderAsync(sectionId);

            await EnsureUniqueOrderAsync(sectionId, newOrder);

            var sectionActivity = new SectionActivity
            {
                SectionId = sectionId,
                ActivityId = activityId,
                ActivityOrder = newOrder
            };

            await _uow.SectionActivityRepository.CreateAsync(sectionActivity);
            await _uow.SaveChangesAsync();
        }

        public async Task RemoveActivityFromSectionAsync(int sectionId, int activityId)
        {
            var sectionActivity = await _uow.SectionActivityRepository
                .GetAllAsQueryable()
                .Where(sa => sa.SectionId == sectionId && sa.ActivityId == activityId)
                .FirstOrDefaultAsync();

            if (sectionActivity == null)
                throw new KeyNotFoundException($"Activity with ID {activityId} is not assigned to section ID {sectionId}.");

            await _uow.SectionActivityRepository.DeleteAsync(sectionActivity);
            await _uow.SaveChangesAsync();
        }

        public async Task UpdateSectionActivityOrderAsync(int sectionId, int activityId, int newOrder)
        {
            var current = await _uow.SectionActivityRepository
                .GetAllAsQueryable()
                .Where(sa => sa.SectionId == sectionId && sa.ActivityId == activityId)
                .FirstOrDefaultAsync();

            if (current == null)
                throw new KeyNotFoundException($"Activity with ID {activityId} is not assigned to section ID {sectionId}.");

            if (current.ActivityOrder == newOrder)
                return;

            await EnsureUniqueOrderAsync(sectionId, newOrder, current);

            current.ActivityOrder = newOrder;
            await _uow.SectionActivityRepository.UpdateAsync(current);
            await _uow.SaveChangesAsync();
        }


        #endregion

        #region Order Logic Handling
        private async Task EnsureUniqueOrderAsync(int sectionId, int targetOrder, SectionActivity? current = null)
        {
            var target = await _uow.SectionActivityRepository
                .GetAllAsQueryable()
                .Where(sa => sa.SectionId == sectionId && sa.ActivityOrder == targetOrder)
                .FirstOrDefaultAsync();

            if (target != null)
            {
                if (current != null)
                {
                    var oldOrder = current.ActivityOrder;
                    target.ActivityOrder = oldOrder;
                    await _uow.SectionActivityRepository.UpdateAsync(target);
                }
                else
                {
                    await ShiftOrdersDownAsync(sectionId, targetOrder);
                }
            }
        }

        private async Task ShiftOrdersDownAsync(int sectionId, int fromOrder)
        {
            var sectionActivities = await _uow.SectionActivityRepository
                .GetAllAsQueryable()
                .Where(sa => sa.SectionId == sectionId && sa.ActivityOrder >= fromOrder)
                .OrderByDescending(sa => sa.ActivityOrder) 
                .ToListAsync();

            foreach (var sa in sectionActivities)
            {
                sa.ActivityOrder += 1;
                await _uow.SectionActivityRepository.UpdateAsync(sa);
            }

            await _uow.SaveChangesAsync();
        }

        private async Task<int> GetNextAvailableOrderAsync(int sectionId)
        {
            int maxOrder = await _uow.SectionActivityRepository
                .GetAllAsQueryable()
                .Where(sa => sa.SectionId == sectionId)
                .MaxAsync(sa => (int?)sa.ActivityOrder) ?? 0;

            return maxOrder + 1;
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
