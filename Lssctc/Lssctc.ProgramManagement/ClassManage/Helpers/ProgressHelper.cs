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
            var activityRecord = await _uow.ActivityRecordRepository
                .GetAllAsQueryable()
                .Include(ar => ar.SectionRecord.LearningProgress.Enrollment)
                .Include(ar => ar.QuizAttempts)
                .Include(ar => ar.PracticeAttempts)
                .FirstOrDefaultAsync(ar => ar.Id == activityRecordId);

            if (activityRecord == null)
                throw new KeyNotFoundException("Activity record not found.");

            if (activityRecord.SectionRecord.LearningProgress.Enrollment.TraineeId != traineeId)
                throw new UnauthorizedAccessException("You are not authorized to update this activity record.");

            bool isCompleted = false;
            decimal? score = null;

            if (activityRecord.ActivityType == (int)ActivityType.Quiz)
            {
                var currentAttempt = activityRecord.QuizAttempts.FirstOrDefault(a => a.IsCurrent);
                if (currentAttempt != null)
                {
                    isCompleted = currentAttempt.IsPass ?? false;
                    score = currentAttempt.AttemptScore;
                }
            }
            else if (activityRecord.ActivityType == (int)ActivityType.Practice)
            {
                var currentAttempt = activityRecord.PracticeAttempts.FirstOrDefault(a => a.IsCurrent);
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
            else if (activityRecord.QuizAttempts.Any() || activityRecord.PracticeAttempts.Any())
            {
                activityRecord.Status = (int)ActivityStatusEnum.InProgress;
            }
            else
            {
                activityRecord.Status = (int)ActivityStatusEnum.NotStarted;
            }

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
            var sectionRecord = await _uow.SectionRecordRepository
                .GetAllAsQueryable()
                .Include(sr => sr.LearningProgress.Enrollment)
                .Include(sr => sr.ActivityRecords)
                .FirstOrDefaultAsync(sr => sr.Id == sectionRecordId);

            if (sectionRecord == null)
                throw new KeyNotFoundException("Section record not found.");

            if (sectionRecord.LearningProgress.Enrollment.TraineeId != traineeId)
                throw new UnauthorizedAccessException("You are not authorized to update this section record.");

            if (sectionRecord.SectionId == null)
                throw new InvalidOperationException("Section record is missing its SectionId link.");

            // Get the "template" total
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

            // Get the "completed" total
            int completedActivities = sectionRecord.ActivityRecords.Count(ar => ar.IsCompleted == true);

            decimal progress = Math.Round(((decimal)completedActivities / totalActivities) * 100, 2);
            sectionRecord.Progress = progress;
            sectionRecord.IsCompleted = (completedActivities == totalActivities);

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
            var learningProgress = await _uow.LearningProgressRepository
                .GetAllAsQueryable()
                .Include(lp => lp.Enrollment)
                .Include(lp => lp.SectionRecords)
                .FirstOrDefaultAsync(lp => lp.Id == learningProgressId);

            if (learningProgress == null)
                throw new KeyNotFoundException("Learning progress not found.");

            if (learningProgress.Enrollment.TraineeId != traineeId)
                throw new UnauthorizedAccessException("You are not authorized to update this learning progress.");

            if (learningProgress.CourseId == 0) // Assuming CourseId is non-nullable
                throw new InvalidOperationException("Learning progress is missing its CourseId link.");

            // Get the "template" total
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

            // Get the "completed" total
            int completedSections = learningProgress.SectionRecords.Count(sr => sr.IsCompleted == true);

            decimal percentage = Math.Round(((decimal)completedSections / totalSections) * 100, 2);
            learningProgress.ProgressPercentage = percentage;

            if (completedSections == totalSections)
            {
                learningProgress.Status = (int)LearningProgressStatusEnum.Completed;
            }
            // Check if *any* progress has been made or if it was already in progress
            else if (completedSections > 0 || learningProgress.SectionRecords.Any(sr => sr.Progress > 0) || learningProgress.Status == (int)LearningProgressStatusEnum.InProgress)
            {
                learningProgress.Status = (int)LearningProgressStatusEnum.InProgress;
            }
            else
            {
                learningProgress.Status = (int)LearningProgressStatusEnum.NotStarted;
            }

            learningProgress.LastUpdated = DateTime.UtcNow;

            await _uow.LearningProgressRepository.UpdateAsync(learningProgress);
            await _uow.SaveChangesAsync();
            return learningProgress;
        }

        #endregion
    }
}
