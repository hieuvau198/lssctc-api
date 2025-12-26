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
        private readonly IUnitOfWork _uow;
        private readonly IActivitySessionService _activitySessionService;

        public ActivitiesService(IUnitOfWork uow, IActivitySessionService activitySessionService)
        {
            _uow = uow;
            _activitySessionService = activitySessionService;
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
                throw new KeyNotFoundException($"Không tìm thấy hoạt động với ID {id}.");
            }

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
                        throw new InvalidOperationException($"Tiêu đề hoạt động '{normalizedTitle}' đã tồn tại.");
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
                throw new KeyNotFoundException($"Không tìm thấy hoạt động với ID {id}.");
            }

            // Check if any section using this activity is locked by an active class
            foreach (var sa in activity.SectionActivities)
            {
                if (await IsSectionLockedAsync(sa.SectionId))
                {
                    throw new InvalidOperationException("Không thể xóa hoạt động này vì nó đang được sử dụng trong một lớp học đang diễn ra hoặc đã kết thúc.");
                }
            }

            // Enforce cleanup first
            if (activity.SectionActivities != null && activity.SectionActivities.Any())
            {
                throw new InvalidOperationException("Không thể xóa hoạt động này vì nó đang được gán vào một hoặc nhiều phần học. Vui lòng gỡ bỏ hoạt động khỏi các phần học trước.");
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
            // Lock Check
            if (await IsSectionLockedAsync(sectionId))
            {
                throw new InvalidOperationException("Không thể thêm hoạt động vào phần học này vì lớp học đang diễn ra hoặc đã kết thúc.");
            }

            var section = await _uow.SectionRepository.GetByIdAsync(sectionId);
            if (section == null)
                throw new KeyNotFoundException($"Không tìm thấy phần học với ID {sectionId}.");

            var activity = await _uow.ActivityRepository.GetByIdAsync(activityId);
            if (activity == null || activity.IsDeleted == true)
                throw new KeyNotFoundException($"Không tìm thấy hoạt động với ID {activityId}.");

            bool alreadyExists = await _uow.SectionActivityRepository
                .GetAllAsQueryable()
                .AnyAsync(sa => sa.SectionId == sectionId && sa.ActivityId == activityId);

            if (alreadyExists)
                throw new InvalidOperationException("Hoạt động này đã được gán vào phần học.");

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

            // 2. Propagate to Open/Draft Classes (Data Consistency for non-locked classes)
            // If class is locked, we threw exception above. If we are here, class is likely Draft or Open.
            var affectedSectionRecords = await _uow.SectionRecordRepository
                .GetAllAsQueryable()
                .Where(sr => sr.SectionId == sectionId)
                .Include(sr => sr.LearningProgress)
                    .ThenInclude(lp => lp.Enrollment)
                .ToListAsync();

            if (affectedSectionRecords.Any())
            {
                // A. Create Activity Records
                foreach (var sectionRecord in affectedSectionRecords)
                {
                    var newActivityRecord = new ActivityRecord
                    {
                        SectionRecordId = sectionRecord.Id,
                        ActivityId = activityId,
                        ActivityType = activity.ActivityType,
                        IsCompleted = false,
                        Status = 0,
                        Score = 0,
                        CompletedDate = null
                    };

                    await _uow.ActivityRecordRepository.CreateAsync(newActivityRecord);
                }
                await _uow.SaveChangesAsync();

                // B. Create Activity Sessions for Affected Classes
                var affectedClassIds = affectedSectionRecords
                    .Select(sr => sr.LearningProgress.Enrollment.ClassId)
                    .Distinct()
                    .ToList();

                foreach (var classId in affectedClassIds)
                {
                    await _activitySessionService.CreateSessionsOnClassStartAsync(classId);
                }

                // C. Recalculate Progress
                foreach (var sectionRecord in affectedSectionRecords)
                {
                    await RecalculateSectionAndLearningProgressAsync(sectionRecord.Id, sectionRecord.LearningProgressId);
                }
            }
        }

        public async Task<ActivityDto> CreateActivityForSectionAsync(int sectionId, CreateActivityDto createDto)
        {
            // Lock Check
            if (await IsSectionLockedAsync(sectionId))
            {
                throw new InvalidOperationException("Không thể tạo và thêm hoạt động vào phần học này vì lớp học đang diễn ra hoặc đã kết thúc.");
            }

            var section = await _uow.SectionRepository.GetByIdAsync(sectionId);
            if (section == null)
                throw new KeyNotFoundException($"Không tìm thấy phần học với ID {sectionId}.");

            // 2. Create the Activity
            var activityDto = await CreateActivityAsync(createDto);

            // 3. Assign to Section
            await AddActivityToSectionAsync(sectionId, activityDto.Id);

            return activityDto;
        }

        public async Task RemoveActivityFromSectionAsync(int sectionId, int activityId)
        {
            // Lock Check
            if (await IsSectionLockedAsync(sectionId))
            {
                throw new InvalidOperationException("Không thể xóa hoạt động khỏi phần học này vì lớp học đang diễn ra hoặc đã kết thúc.");
            }

            var sectionActivity = await _uow.SectionActivityRepository
                .GetAllAsQueryable()
                .Where(sa => sa.SectionId == sectionId && sa.ActivityId == activityId)
                .FirstOrDefaultAsync();

            if (sectionActivity == null)
                throw new KeyNotFoundException($"Hoạt động với ID {activityId} chưa được gán vào phần học ID {sectionId}.");

            // 1. Handle Open Classes (Cleanup for non-locked classes)
            var affectedSectionRecords = await _uow.SectionRecordRepository
                .GetAllAsQueryable()
                .Where(sr => sr.SectionId == sectionId)
                .Include(sr => sr.LearningProgress)
                .ToListAsync();

            if (affectedSectionRecords.Any())
            {
                var sectionRecordIds = affectedSectionRecords.Select(sr => sr.Id).ToList();

                var activityRecordsToDelete = await _uow.ActivityRecordRepository
                    .GetAllAsQueryable()
                    .Where(ar => sectionRecordIds.Contains(ar.SectionRecordId) && ar.ActivityId == activityId)
                    .ToListAsync();

                if (activityRecordsToDelete.Any())
                {
                    var arIds = activityRecordsToDelete.Select(ar => ar.Id).ToList();

                    // Cleanup children
                    await _uow.PracticeAttemptRepository
                        .GetAllAsQueryable()
                        .Where(pa => arIds.Contains(pa.ActivityRecordId))
                        .ExecuteDeleteAsync();

                    await _uow.QuizAttemptRepository
                        .GetAllAsQueryable()
                        .Where(qa => arIds.Contains(qa.ActivityRecordId))
                        .ExecuteDeleteAsync();

                    // Delete the ActivityRecords
                    await _uow.ActivityRecordRepository
                        .GetAllAsQueryable()
                        .Where(ar => arIds.Contains(ar.Id))
                        .ExecuteDeleteAsync();
                }

                // 2. Remove from SectionActivity
                await _uow.SectionActivityRepository.DeleteAsync(sectionActivity);
                await _uow.SaveChangesAsync();

                // 3. Recalculate Progress
                foreach (var sectionRecord in affectedSectionRecords)
                {
                    await RecalculateSectionAndLearningProgressAsync(sectionRecord.Id, sectionRecord.LearningProgressId);
                }
            }
            else
            {
                await _uow.SectionActivityRepository.DeleteAsync(sectionActivity);
                await _uow.SaveChangesAsync();
            }
        }

        public async Task UpdateSectionActivityOrderAsync(int sectionId, int activityId, int newOrder)
        {
            // Lock Check
            if (await IsSectionLockedAsync(sectionId))
            {
                throw new InvalidOperationException("Không thể cập nhật thứ tự hoạt động trong phần học này vì lớp học đang diễn ra hoặc đã kết thúc.");
            }

            var current = await _uow.SectionActivityRepository
                .GetAllAsQueryable()
                .Where(sa => sa.SectionId == sectionId && sa.ActivityId == activityId)
                .FirstOrDefaultAsync();

            if (current == null)
                throw new KeyNotFoundException($"Hoạt động với ID {activityId} chưa được gán vào phần học ID {sectionId}.");

            if (current.ActivityOrder == newOrder)
                return;

            await EnsureUniqueOrderAsync(sectionId, newOrder, current);

            current.ActivityOrder = newOrder;
            await _uow.SectionActivityRepository.UpdateAsync(current);
            await _uow.SaveChangesAsync();
        }

        #endregion

        #region Helpers

        private async Task<bool> IsSectionLockedAsync(int sectionId)
        {
            var lockedStatuses = new[] {
                (int)ClassStatusEnum.Open,
                (int)ClassStatusEnum.Inprogress,
                (int)ClassStatusEnum.Completed,
                (int)ClassStatusEnum.Cancelled
            };

            // Find all CourseIDs this Section is linked to
            var courseIds = await _uow.CourseSectionRepository
                .GetAllAsQueryable()
                .Where(cs => cs.SectionId == sectionId)
                .Select(cs => cs.CourseId)
                .Distinct()
                .ToListAsync();

            if (!courseIds.Any())
            {
                return false;
            }

            // Check if ANY of those courses are linked to an active class
            return await _uow.ClassRepository
                .GetAllAsQueryable()
                .AnyAsync(c => courseIds.Contains(c.ProgramCourse.CourseId) &&
                               c.Status.HasValue &&
                               lockedStatuses.Contains(c.Status.Value));
        }

        #endregion

        #region Progress Calculation Logic
        private async Task RecalculateSectionAndLearningProgressAsync(int sectionRecordId, int learningProgressId)
        {
            // 1. Recalculate Section Record
            var sectionRecordData = await _uow.SectionRecordRepository
                .GetAllAsQueryable()
                .Include(sr => sr.ActivityRecords)
                .FirstOrDefaultAsync(sr => sr.Id == sectionRecordId);

            if (sectionRecordData != null)
            {
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

                var sectionRecordToUpdate = await _uow.SectionRecordRepository.GetByIdAsync(sectionRecordId);
                if (sectionRecordToUpdate != null)
                {
                    sectionRecordToUpdate.Progress = newProgress;
                    sectionRecordToUpdate.IsCompleted = newIsCompleted;
                    await _uow.SectionRecordRepository.UpdateAsync(sectionRecordToUpdate);
                    await _uow.SaveChangesAsync();
                }
            }

            // 2. Recalculate Learning Progress
            var learningProgressData = await _uow.LearningProgressRepository
                .GetAllAsQueryable()
                .Include(lp => lp.SectionRecords)
                .FirstOrDefaultAsync(lp => lp.Id == learningProgressId);

            if (learningProgressData != null)
            {
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

            throw new ArgumentException($"Loại hoạt động không hợp lệ: {activityType}");
        }
        #endregion
    }
}