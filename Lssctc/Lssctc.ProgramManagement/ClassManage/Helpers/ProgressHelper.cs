using Lssctc.Share.Entities;
using Lssctc.Share.Enums;
using Lssctc.Share.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Lssctc.ProgramManagement.ClassManage.Helpers
{
    public class ProgressHelper
    {
        private readonly IUnitOfWork _uow;
        public ProgressHelper(IUnitOfWork uow)
        {
            _uow = uow;
        }
        #region BR


        // each trainee can have multiple attempts for each practice, or each quiz
        // Logic: when submit new attempt, mark all previous attempts IsCurrent = false, and new one IsCurrent = true
        // each time when calculate progress, only consider attempts with IsCurrent = true
        // each trainee has only one enrollment and one learning progress per class
        // each learning progress can have only one set of section record that match course section
        // calculate learning progress by how many section record is completed
        // learning progress change status to Completed if all sections related has section record that is completed
        // each section record can have only one set of activity record that match section activity
        // calculate section record by how many activity record is completed
        // section record is completed if all activity related has activity record is completed

        #endregion

        #region Methods

        /// <summary>
        /// M1: Updates an ActivityRecord's score and completion status based on its *current* attempt.
        /// </summary>
        public async Task<ActivityRecord> UpdateActivityRecordProgressAsync(int traineeId, int activityRecordId)
        {
            // 1. Load the ActivityRecord *without* its child attempts.
            // We use .Find() or GetByIdAsync which *does* track by default, but only the single entity.
            // Since we get it from the UoW, it uses the shared context.
            var activityRecord = await _uow.ActivityRecordRepository.GetByIdAsync(activityRecordId);

            if (activityRecord == null)
                throw new KeyNotFoundException("Activity record not found.");

            // We still need to verify traineeId, so we load the parent chain AsNoTracking just for verification
            var verificationRecord = await _uow.ActivityRecordRepository
                .GetAllAsQueryable() // AsNoTracking()
                .Include(ar => ar.SectionRecord.LearningProgress.Enrollment)
                .Select(ar => new { ar.Id, ar.SectionRecord.LearningProgress.Enrollment.TraineeId })
                .FirstOrDefaultAsync(ar => ar.Id == activityRecordId);

            if (verificationRecord == null || verificationRecord.TraineeId != traineeId)
                throw new UnauthorizedAccessException("You are not authorized to update this activity record.");

            bool isCompleted = false;
            decimal? score = null;

            // 2. Load the attempts in *separate, non-tracking* queries.
            if (activityRecord.ActivityType == (int)ActivityType.Quiz)
            {
                var currentAttempt = await _uow.QuizAttemptRepository
                    .GetAllAsQueryable() // AsNoTracking()
                    .Where(a => a.ActivityRecordId == activityRecordId && a.IsCurrent)
                    .FirstOrDefaultAsync();

                if (currentAttempt != null)
                {
                    isCompleted = currentAttempt.IsPass ?? false;
                    score = currentAttempt.AttemptScore;
                }
            }
            else if (activityRecord.ActivityType == (int)ActivityType.Practice)
            {
                var currentAttempt = await _uow.PracticeAttemptRepository
                    .GetAllAsQueryable() // AsNoTracking()
                    .Where(a => a.ActivityRecordId == activityRecordId && a.IsCurrent)
                    .FirstOrDefaultAsync();

                if (currentAttempt != null)
                {
                    isCompleted = currentAttempt.IsPass ?? false;
                    score = currentAttempt.Score;
                }
            }
            else if (activityRecord.ActivityType == (int)ActivityType.Material)
            {
                isCompleted = activityRecord.IsCompleted ?? false;
                score = isCompleted ? 10 : 0;
            }

            activityRecord.IsCompleted = isCompleted;
            activityRecord.Score = score;

            if (isCompleted)
            {
                activityRecord.Status = (int)ActivityStatusEnum.Completed;
                activityRecord.CompletedDate = DateTime.UtcNow;
            }
            // We need to check if *any* attempts exist, which requires another query
            else if (activityRecord.ActivityType == (int)ActivityType.Quiz || activityRecord.ActivityType == (int)ActivityType.Practice)
            {
                bool hasAnyAttempts = await _uow.QuizAttemptRepository.ExistsAsync(qa => qa.ActivityRecordId == activityRecordId) ||
                                      await _uow.PracticeAttemptRepository.ExistsAsync(pa => pa.ActivityRecordId == activityRecordId);

                if (hasAnyAttempts)
                {
                    activityRecord.Status = (int)ActivityStatusEnum.InProgress;
                }
                else
                {
                    activityRecord.Status = (int)ActivityStatusEnum.NotStarted;
                }
            }
            else
            {
                activityRecord.Status = (int)ActivityStatusEnum.NotStarted;
            }

            // 3. Use the *tracked* entity from GetByIdAsync and call Update.
            // This will only mark this single entity as modified.
            await _uow.ActivityRecordRepository.UpdateAsync(activityRecord);
            await _uow.SaveChangesAsync();
            return activityRecord;
        }

        /// <summary>
        /// M2: Updates a SectionRecord's progress and completion status based on its child ActivityRecords
        /// compared against the course template.
        /// </summary>
        public async Task<SectionRecord> UpdateSectionRecordProgressAsync(int traineeId, int sectionRecordId)
        {
            // 1. Load the SectionRecord *without* its child ActivityRecords.
            var sectionRecord = await _uow.SectionRecordRepository.GetByIdAsync(sectionRecordId);

            if (sectionRecord == null)
                throw new KeyNotFoundException("Section record not found.");

            // Verify ownership
            var verificationRecord = await _uow.SectionRecordRepository
                .GetAllAsQueryable() // AsNoTracking
                .Include(sr => sr.LearningProgress.Enrollment)
                .Select(sr => new { sr.Id, sr.LearningProgress.Enrollment.TraineeId })
                .FirstOrDefaultAsync(sr => sr.Id == sectionRecordId);

            if (verificationRecord == null || verificationRecord.TraineeId != traineeId)
                throw new UnauthorizedAccessException("You are not authorized to update this section record.");

            if (sectionRecord.SectionId == null)
                throw new InvalidOperationException("Section record is missing its SectionId link.");

            // 2. Get the "template" total
            int totalActivities = await _uow.SectionActivityRepository
                .GetAllAsQueryable()
                .CountAsync(sa => sa.SectionId == sectionRecord.SectionId);

            if (totalActivities == 0)
            {
                sectionRecord.Progress = 100;
                sectionRecord.IsCompleted = true;

                await _uow.SectionRecordRepository.UpdateAsync(sectionRecord);
                await _uow.SaveChangesAsync();
                return sectionRecord;
            }

            // 3. Get the "completed" total from a *separate, non-tracking* query
            int completedActivities = await _uow.ActivityRecordRepository
                .GetAllAsQueryable() // AsNoTracking()
                .CountAsync(ar => ar.SectionRecordId == sectionRecordId && ar.IsCompleted == true);

            decimal progress = Math.Round(((decimal)completedActivities / totalActivities) * 100, 2);
            sectionRecord.Progress = progress;
            sectionRecord.IsCompleted = (completedActivities == totalActivities);

            // 4. Update the *tracked* entity
            await _uow.SectionRecordRepository.UpdateAsync(sectionRecord);
            await _uow.SaveChangesAsync();
            return sectionRecord;
        }

        /// <summary>
        /// M3: Updates a LearningProgress's percentage and status based on its child SectionRecords
        /// compared against the course template.
        /// </summary>
        public async Task<LearningProgress> UpdateLearningProgressProgressAsync(int traineeId, int learningProgressId)
        {
            // 1. Load the LearningProgress *without* its child SectionRecords.
            var learningProgress = await _uow.LearningProgressRepository.GetByIdAsync(learningProgressId);

            if (learningProgress == null)
                throw new KeyNotFoundException("Learning progress not found.");

            // Verify ownership
            var verificationRecord = await _uow.LearningProgressRepository
                .GetAllAsQueryable() // AsNoTracking
                .Include(lp => lp.Enrollment)
                .Select(lp => new { lp.Id, lp.Enrollment.TraineeId })
                .FirstOrDefaultAsync(lp => lp.Id == learningProgressId);

            if (verificationRecord == null || verificationRecord.TraineeId != traineeId)
                throw new UnauthorizedAccessException("You are not authorized to update this learning progress.");

            if (learningProgress.CourseId == 0)
                throw new InvalidOperationException("Learning progress is missing its CourseId link.");

            // 2. Get the "template" total
            int totalSections = await _uow.CourseSectionRepository
                .GetAllAsQueryable()
                .CountAsync(cs => cs.CourseId == learningProgress.CourseId);

            if (totalSections == 0)
            {
                learningProgress.ProgressPercentage = 100;
                learningProgress.Status = (int)LearningProgressStatusEnum.Completed;

                await _uow.LearningProgressRepository.UpdateAsync(learningProgress);
                await _uow.SaveChangesAsync();
                return learningProgress;
            }

            // 3. Get the "completed" total from a *separate, non-tracking* query
            int completedSections = await _uow.SectionRecordRepository
                .GetAllAsQueryable() // AsNoTracking()
                .CountAsync(sr => sr.LearningProgressId == learningProgressId && sr.IsCompleted == true);

            // 4. Get "any progress" from a separate query
            bool anyProgress = completedSections > 0 || await _uow.SectionRecordRepository
                .GetAllAsQueryable() // AsNoTracking()
                .AnyAsync(sr => sr.LearningProgressId == learningProgressId && sr.Progress > 0);

            decimal percentage = Math.Round(((decimal)completedSections / totalSections) * 100, 2);
            learningProgress.ProgressPercentage = percentage;

            if (completedSections == totalSections)
            {
                learningProgress.Status = (int)LearningProgressStatusEnum.Completed;
            }
            else if (anyProgress || learningProgress.Status == (int)LearningProgressStatusEnum.InProgress)
            {
                learningProgress.Status = (int)LearningProgressStatusEnum.InProgress;
            }
            else
            {
                learningProgress.Status = (int)LearningProgressStatusEnum.NotStarted;
            }

            learningProgress.LastUpdated = DateTime.UtcNow;

            // 5. Update the *tracked* entity
            await _uow.LearningProgressRepository.UpdateAsync(learningProgress);
            await _uow.SaveChangesAsync();
            return learningProgress;
        }

        #endregion
    }
}