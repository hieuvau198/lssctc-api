using Lssctc.ProgramManagement.Practices.Dtos;
using Lssctc.Share.Common;
using Lssctc.Share.Entities;
using Lssctc.Share.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Lssctc.ProgramManagement.Practices.Services
{
    public class PracticesService : IPracticesService
    {
        private readonly IUnitOfWork _uow;

        public PracticesService(IUnitOfWork uow)
        {
            _uow = uow;
        }

        public async Task<IEnumerable<PracticeDto>> GetAllPracticesAsync()
        {
            var practices = await _uow.PracticeRepository
                .GetAllAsQueryable()
                .Where(p => p.IsDeleted == null || p.IsDeleted == false)
                .ToListAsync();

            return practices.Select(MapToDto);
        }

        public async Task<PagedResult<PracticeDto>> GetPracticesAsync(int pageNumber, int pageSize)
        {
            if (pageNumber < 1) pageNumber = 1;
            if (pageSize < 1) pageSize = 10;

            var query = _uow.PracticeRepository
                .GetAllAsQueryable()
                .Where(p => p.IsDeleted == null || p.IsDeleted == false)
                .Select(p => MapToDto(p));

            return await query.ToPagedResultAsync(pageNumber, pageSize);
        }

        public async Task<PracticeDto?> GetPracticeByIdAsync(int id)
        {
            var practice = await _uow.PracticeRepository.GetByIdAsync(id);
            if (practice == null || practice.IsDeleted == true) return null;
            return MapToDto(practice);
        }

        public async Task<PracticeDto> CreatePracticeAsync(CreatePracticeDto createDto)
        {
            if (string.IsNullOrWhiteSpace(createDto.PracticeName))
                throw new ArgumentException("Practice name is required.");

            if (!string.IsNullOrWhiteSpace(createDto.PracticeCode))
            {
                var normalizedCode = createDto.PracticeCode.Trim();
                bool codeExists = await _uow.PracticeRepository
                    .ExistsAsync(p => p.PracticeCode != null &&
                                     p.PracticeCode.ToLower() == normalizedCode.ToLower() &&
                                     (p.IsDeleted == null || p.IsDeleted == false));

                if (codeExists) throw new ArgumentException($"Practice code '{normalizedCode}' already exists.");
            }

            var practice = new Practice
            {
                PracticeName = createDto.PracticeName.Trim(),
                PracticeDescription = createDto.PracticeDescription?.Trim(),
                EstimatedDurationMinutes = createDto.EstimatedDurationMinutes,
                DifficultyLevel = createDto.DifficultyLevel,
                MaxAttempts = createDto.MaxAttempts,
                PracticeCode = createDto.PracticeCode?.Trim(),
                CreatedDate = DateTime.UtcNow,
                IsActive = createDto.IsActive ?? true,
                IsDeleted = false
            };

            await _uow.PracticeRepository.CreateAsync(practice);
            await _uow.SaveChangesAsync();
            return MapToDto(practice);
        }

        public async Task<PracticeDto> UpdatePracticeAsync(int id, UpdatePracticeDto updateDto)
        {
            var practice = await _uow.PracticeRepository.GetByIdAsync(id);
            if (practice == null || practice.IsDeleted == true)
                throw new KeyNotFoundException($"Practice with ID {id} not found.");

            if (!string.IsNullOrWhiteSpace(updateDto.PracticeCode))
            {
                var normalizedCode = updateDto.PracticeCode.Trim();
                if (practice.PracticeCode?.ToLower() != normalizedCode.ToLower())
                {
                    bool codeExists = await _uow.PracticeRepository
                        .ExistsAsync(p => p.Id != id &&
                                         p.PracticeCode != null &&
                                         p.PracticeCode.ToLower() == normalizedCode.ToLower() &&
                                         (p.IsDeleted == null || p.IsDeleted == false));
                    if (codeExists) throw new ArgumentException($"Practice code '{normalizedCode}' already exists.");
                }
            }

            practice.PracticeName = updateDto.PracticeName?.Trim() ?? practice.PracticeName;
            practice.PracticeDescription = updateDto.PracticeDescription?.Trim() ?? practice.PracticeDescription;
            practice.EstimatedDurationMinutes = updateDto.EstimatedDurationMinutes ?? practice.EstimatedDurationMinutes;
            practice.DifficultyLevel = updateDto.DifficultyLevel ?? practice.DifficultyLevel;
            practice.MaxAttempts = updateDto.MaxAttempts ?? practice.MaxAttempts;
            practice.IsActive = updateDto.IsActive ?? practice.IsActive;

            if (!string.IsNullOrWhiteSpace(updateDto.PracticeCode))
                practice.PracticeCode = updateDto.PracticeCode.Trim();

            await _uow.PracticeRepository.UpdateAsync(practice);
            await _uow.SaveChangesAsync();
            return MapToDto(practice);
        }

        public async Task DeletePracticeAsync(int id)
        {
            var practice = await _uow.PracticeRepository
                .GetAllAsQueryable()
                .Include(p => p.ActivityPractices)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (practice == null) throw new KeyNotFoundException($"Practice with ID {id} not found.");
            if (practice.ActivityPractices != null && practice.ActivityPractices.Any())
                throw new InvalidOperationException("Cannot delete a practice linked to activities.");

            practice.IsDeleted = true;
            await _uow.PracticeRepository.UpdateAsync(practice);
            await _uow.SaveChangesAsync();
        }

        public async Task<IEnumerable<PracticeDto>> GetPracticesByActivityAsync(int activityId)
        {
            var practices = await _uow.ActivityPracticeRepository
                .GetAllAsQueryable()
                .Include(ap => ap.Practice)
                .Where(ap => ap.ActivityId == activityId && (ap.IsDeleted == null || ap.IsDeleted == false))
                .Select(ap => ap.Practice)
                .Where(p => p.IsDeleted == null || p.IsDeleted == false)
                .ToListAsync();

            return practices.Select(p => MapToDto(p));
        }

        public async Task AddPracticeToActivityAsync(int activityId, int practiceId)
        {
            var activity = await _uow.ActivityRepository
                .GetAllAsQueryable()
                .Include(a => a.ActivityMaterials)
                .Include(a => a.ActivityQuizzes)
                .Include(a => a.ActivityPractices)
                .FirstOrDefaultAsync(a => a.Id == activityId);

            var practice = await _uow.PracticeRepository.GetByIdAsync(practiceId);

            if (activity == null) throw new KeyNotFoundException($"Activity with ID {activityId} not found.");
            if (practice == null) throw new KeyNotFoundException($"Practice with ID {practiceId} not found.");
            if (activity.ActivityPractices.Any()) throw new InvalidOperationException("This activity already has a practice assigned.");
            if (activity.ActivityMaterials.Any()) throw new InvalidOperationException("This activity already has a material assigned. Cannot add practice.");
            if (activity.ActivityQuizzes.Any()) throw new InvalidOperationException("This activity already has a quiz assigned. Cannot add practice.");

            bool exists = await _uow.ActivityPracticeRepository
                .GetAllAsQueryable()
                .AnyAsync(ap => ap.ActivityId == activityId && ap.PracticeId == practiceId);

            if (exists) throw new InvalidOperationException("This practice is already linked to the activity.");

            var link = new ActivityPractice
            {
                ActivityId = activityId,
                PracticeId = practiceId,
                IsActive = true,
                IsDeleted = false
            };

            await _uow.ActivityPracticeRepository.CreateAsync(link);
            await _uow.SaveChangesAsync();

            await ResetActivityRecordsAndRecalculateAsync(activityId);
        }

        public async Task RemovePracticeFromActivityAsync(int activityId, int practiceId)
        {
            var link = await _uow.ActivityPracticeRepository
                .GetAllAsQueryable()
                .FirstOrDefaultAsync(ap => ap.ActivityId == activityId && ap.PracticeId == practiceId);

            if (link == null) throw new KeyNotFoundException("Practice is not linked to this activity.");

            await _uow.ActivityPracticeRepository.DeleteAsync(link);
            await _uow.SaveChangesAsync();
        }

        private async Task ResetActivityRecordsAndRecalculateAsync(int activityId)
        {
            var records = await _uow.ActivityRecordRepository.GetAllAsQueryable()
                .Where(ar => ar.ActivityId == activityId)
                .Include(ar => ar.SectionRecord)
                .ToListAsync();

            if (records.Any())
            {
                foreach (var r in records)
                {
                    r.IsCompleted = false;
                    r.Status = 0;
                    r.Score = 0;
                    r.CompletedDate = null;
                    await _uow.ActivityRecordRepository.UpdateAsync(r);
                }
                await _uow.SaveChangesAsync();

                var recordsToRecalculate = records
                    .Select(r => new { r.SectionRecordId, r.SectionRecord.LearningProgressId })
                    .Distinct()
                    .ToList();

                foreach (var item in recordsToRecalculate)
                {
                    await RecalculateSectionAndLearningProgressAsync(item.SectionRecordId, item.LearningProgressId);
                }
            }
        }

        private async Task RecalculateSectionAndLearningProgressAsync(int sectionRecordId, int learningProgressId)
        {
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
                if (totalSectionsInCourse > 0) avgProgress = totalProgressSum / totalSectionsInCourse;
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

        private static PracticeDto MapToDto(Practice p)
        {
            return new PracticeDto
            {
                Id = p.Id,
                PracticeName = p.PracticeName,
                PracticeDescription = p.PracticeDescription,
                EstimatedDurationMinutes = p.EstimatedDurationMinutes,
                DifficultyLevel = p.DifficultyLevel,
                MaxAttempts = p.MaxAttempts,
                CreatedDate = p.CreatedDate,
                IsActive = p.IsActive,
                PracticeCode = p.PracticeCode
            };
        }
    }
}