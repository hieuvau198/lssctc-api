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

            // REMOVED: Lock check (IsActivityLockedAsync) to allow updates at any time.

            // 1. Validation: Unique Title Check
            if (updateDto.ActivityTitle != null)
            {
                var normalizedTitle = string.Join(' ', updateDto.ActivityTitle.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries));

                if (!string.Equals(activity.ActivityTitle, normalizedTitle, StringComparison.CurrentCultureIgnoreCase))
                {
                    var isDuplicate = await _uow.ActivityRepository.GetAllAsQueryable()
                        .AnyAsync(a => a.ActivityTitle == normalizedTitle && a.Id != id && a.IsDeleted != true);

                    if (isDuplicate)
                    {
                        throw new InvalidOperationException($"Activity title '{normalizedTitle}' already exists.");
                    }
                    activity.ActivityTitle = normalizedTitle;
                }
            }

            // 2. Update other fields
            activity.ActivityDescription = string.IsNullOrWhiteSpace(updateDto.ActivityDescription)
                ? null
                : string.Join(' ', updateDto.ActivityDescription.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries));

            if (updateDto.ActivityType != null)
            {
                activity.ActivityType = ParseActivityType(updateDto.ActivityType);
            }

            if (updateDto.EstimatedDurationMinutes.HasValue)
            {
                activity.EstimatedDurationMinutes = updateDto.EstimatedDurationMinutes;
            }

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

            // REMOVED: IsActivityLockedAsync check. 
            // The constraint below is sufficient: if it's in a section, you can't delete it.
            // User must remove it from the section first (using RemoveActivityFromSectionAsync which now handles the "hard delete" of records).

            if (activity.SectionActivities != null && activity.SectionActivities.Any())
            {
                throw new InvalidOperationException("Cannot delete activity associated with sections. Please remove it from all sections first.");
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
            // REMOVED: Lock check (IsSectionLockedAsync)

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

            // 1. Add to SectionActivity (The Template)
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

            // 2. Propagate to Active Classes (The Live Data)
            // Find all existing SectionRecords for this SectionId
            var affectedSectionRecords = await _uow.SectionRecordRepository
                .GetAllAsQueryable()
                .Where(sr => sr.SectionId == sectionId)
                .Include(sr => sr.LearningProgress)
                .ToListAsync();

            if (affectedSectionRecords.Any())
            {
                foreach (var sectionRecord in affectedSectionRecords)
                {
                    // Create a new ActivityRecord for this trainee
                    var newActivityRecord = new ActivityRecord
                    {
                        SectionRecordId = sectionRecord.Id,
                        ActivityId = activityId,
                        ActivityType = activity.ActivityType,
                        IsCompleted = false,
                        Status = 0, // 0 = Not Started
                        Score = 0,
                        CompletedDate = null
                    };

                    await _uow.ActivityRecordRepository.CreateAsync(newActivityRecord);

                    // We need to save here to ensure the ActivityRecord exists before recalculating 
                    // (though Recalculate usually fetches from DB, so yes, save first)
                }
                await _uow.SaveChangesAsync();

                // 3. Recalculate Progress for all affected records
                foreach (var sectionRecord in affectedSectionRecords)
                {
                    await RecalculateSectionAndLearningProgressAsync(sectionRecord.Id, sectionRecord.LearningProgressId);
                }
            }
        }

        public async Task<ActivityDto> CreateActivityForSectionAsync(int sectionId, CreateActivityDto createDto)
        {
            // 1. Validation for Section existence check is handled in AddActivityToSectionAsync, 
            // but we can check early if needed. However, since we are creating the activity first,
            // we should probably check section first to avoid orphan activity creation if section is invalid.

            var section = await _uow.SectionRepository.GetByIdAsync(sectionId);
            if (section == null)
                throw new KeyNotFoundException($"Section with ID {sectionId} not found.");

            // 2. Create the Activity
            // We reuse the existing logic which handles trimming and enum parsing
            var activityDto = await CreateActivityAsync(createDto);

            // 3. Assign to Section
            // This method contains the logic to propagate changes to active classes (ActivityRecords, etc.)
            await AddActivityToSectionAsync(sectionId, activityDto.Id);

            return activityDto;
        }

        public async Task RemoveActivityFromSectionAsync(int sectionId, int activityId)
        {
            // REMOVED: Lock check (IsSectionLockedAsync)

            var sectionActivity = await _uow.SectionActivityRepository
                .GetAllAsQueryable()
                .Where(sa => sa.SectionId == sectionId && sa.ActivityId == activityId)
                .FirstOrDefaultAsync();

            if (sectionActivity == null)
                throw new KeyNotFoundException($"Activity with ID {activityId} is not assigned to section ID {sectionId}.");

            // 1. Handle Active Classes (Deep Delete of Activity Records)
            var affectedSectionRecords = await _uow.SectionRecordRepository
                .GetAllAsQueryable()
                .Where(sr => sr.SectionId == sectionId)
                .Include(sr => sr.LearningProgress)
                .ToListAsync();

            if (affectedSectionRecords.Any())
            {
                var sectionRecordIds = affectedSectionRecords.Select(sr => sr.Id).ToList();

                // Find all ActivityRecords for this specific Activity in these Sections
                var activityRecordsToDelete = await _uow.ActivityRecordRepository
                    .GetAllAsQueryable()
                    .Where(ar => sectionRecordIds.Contains(ar.SectionRecordId) && ar.ActivityId == activityId)
                    .ToListAsync();

                if (activityRecordsToDelete.Any())
                {
                    // Clean up children (PracticeAttempts, QuizAttempts, etc.)
                    // EF Core Cascade Delete is preferred, but manual cleanup ensures safety
                    var arIds = activityRecordsToDelete.Select(ar => ar.Id).ToList();

                    // Example: Delete PracticeAttempts linked to these ActivityRecords
                    await _uow.PracticeAttemptRepository
                        .GetAllAsQueryable()
                        .Where(pa => arIds.Contains(pa.ActivityRecordId))
                        .ExecuteDeleteAsync();

                    // Example: Delete QuizAttempts
                    await _uow.QuizAttemptRepository
                        .GetAllAsQueryable()
                        .Where(qa => arIds.Contains(qa.ActivityRecordId))
                        .ExecuteDeleteAsync();

                    // Delete the ActivityRecords themselves
                    await _uow.ActivityRecordRepository
                        .GetAllAsQueryable()
                        .Where(ar => arIds.Contains(ar.Id))
                        .ExecuteDeleteAsync();
                }

                // 2. Remove from SectionActivity (The Template)
                await _uow.SectionActivityRepository.DeleteAsync(sectionActivity);
                await _uow.SaveChangesAsync();

                // 3. Recalculate Progress for affected records (after deletion)
                foreach (var sectionRecord in affectedSectionRecords)
                {
                    await RecalculateSectionAndLearningProgressAsync(sectionRecord.Id, sectionRecord.LearningProgressId);
                }
            }
            else
            {
                // If no active classes, just remove the template link
                await _uow.SectionActivityRepository.DeleteAsync(sectionActivity);
                await _uow.SaveChangesAsync();
            }
        }

        public async Task UpdateSectionActivityOrderAsync(int sectionId, int activityId, int newOrder)
        {
            // REMOVED: Lock check (IsSectionLockedAsync) to allow reordering at any time

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

        #region Progress Calculation Logic (New)

        /// <summary>
        /// Recalculates the progress for a SectionRecord and then triggers recalculation for its parent LearningProgress.
        /// </summary>
        private async Task RecalculateSectionAndLearningProgressAsync(int sectionRecordId, int learningProgressId)
        {
            // ---------------------------------------------------------
            // 1. RECALCULATE SECTION RECORD
            // ---------------------------------------------------------

            // A. Read Data (Detached): Use existing repo method to get graph for calculation
            // We use AsNoTracking (via GetAllAsQueryable) to avoid messing with the Context's state
            var sectionRecordData = await _uow.SectionRecordRepository
                .GetAllAsQueryable()
                .Include(sr => sr.ActivityRecords)
                .FirstOrDefaultAsync(sr => sr.Id == sectionRecordId);

            if (sectionRecordData != null)
            {
                // B. Perform Calculation in Memory
                int totalActivities = sectionRecordData.ActivityRecords.Count();
                int completedActivities = sectionRecordData.ActivityRecords.Count(ar => ar.IsCompleted == true);

                decimal newProgress = 0;
                bool newIsCompleted = false;

                if (totalActivities == 0)
                {
                    newProgress = 100;
                    newIsCompleted = true;
                }
                else
                {
                    newProgress = (decimal)completedActivities / totalActivities * 100;
                    newIsCompleted = completedActivities == totalActivities;
                }

                // C. Update Entity (Tracked): Fetch the specific entity to update
                // GetByIdAsync uses FindAsync, which returns a TRACKED entity and usually DOES NOT load children.
                // This prevents the "Identity Conflict" with the ActivityRecords we just created.
                var sectionRecordToUpdate = await _uow.SectionRecordRepository.GetByIdAsync(sectionRecordId);

                if (sectionRecordToUpdate != null)
                {
                    sectionRecordToUpdate.Progress = newProgress;
                    sectionRecordToUpdate.IsCompleted = newIsCompleted;

                    // UpdateAsync on a tracked entity just marks it modified
                    await _uow.SectionRecordRepository.UpdateAsync(sectionRecordToUpdate);

                    // Save here to ensure the DB is updated before we calculate LearningProgress
                    await _uow.SaveChangesAsync();
                }
            }

            // ---------------------------------------------------------
            // 2. RECALCULATE LEARNING PROGRESS
            // ---------------------------------------------------------

            // A. Read Data (Detached)
            // Since we saved above, this query will retrieve the updated SectionRecord progress from DB
            var learningProgressData = await _uow.LearningProgressRepository
                .GetAllAsQueryable()
                .Include(lp => lp.SectionRecords)
                .FirstOrDefaultAsync(lp => lp.Id == learningProgressId);

            if (learningProgressData != null)
            {
                // B. Perform Calculation
                var totalSectionsInCourse = await _uow.CourseSectionRepository
                    .GetAllAsQueryable()
                    .CountAsync(cs => cs.CourseId == learningProgressData.CourseId);

                decimal totalProgressSum = learningProgressData.SectionRecords.Sum(sr => sr.Progress ?? 0);
                decimal avgProgress = 0;

                if (totalSectionsInCourse > 0)
                {
                    avgProgress = totalProgressSum / totalSectionsInCourse;
                }

                if (avgProgress > 100) avgProgress = 100;

                // C. Update Entity (Tracked)
                var learningProgressToUpdate = await _uow.LearningProgressRepository.GetByIdAsync(learningProgressId);

                if (learningProgressToUpdate != null)
                {
                    learningProgressToUpdate.ProgressPercentage = avgProgress;
                    learningProgressToUpdate.LastUpdated = DateTime.UtcNow;

                    await _uow.LearningProgressRepository.UpdateAsync(learningProgressToUpdate);
                    await _uow.SaveChangesAsync();
                }
            }
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