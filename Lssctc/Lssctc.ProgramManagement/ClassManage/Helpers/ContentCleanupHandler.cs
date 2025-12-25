using Lssctc.Share.Contexts;
using Lssctc.Share.Entities;
using Lssctc.Share.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Lssctc.ProgramManagement.ClassManage.Helpers
{
    public class ContentCleanupHandler
    {
        private readonly IUnitOfWork _unitOfWork;

        public ContentCleanupHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task CleanupUnusedContentAsync()
        {
            var db = _unitOfWork.GetDbContext();

            // 1. Cleanup Unused Sections (Sections not linked to any Course)
            await CleanupOrphanedSectionsAsync(db);

            // 2. Cleanup Unused Activities (Activities not linked to any Section)
            // Note: Deleting sections above removes their SectionActivities, potentially orphaning more activities.
            await CleanupOrphanedActivitiesAsync(db);
        }

        private async Task CleanupOrphanedSectionsAsync(DbContext db)
        {
            // Find sections that are NOT in any CourseSection
            var unusedSections = await db.Set<Section>()
                .Where(s => !s.CourseSections.Any())
                .ToListAsync();

            if (!unusedSections.Any()) return;

            var sectionIds = unusedSections.Select(s => s.Id).ToList();

            // 1. Delete SectionActivities linking these sections
            var sectionActivities = await db.Set<SectionActivity>()
                .Where(sa => sectionIds.Contains(sa.SectionId))
                .ToListAsync();
            db.RemoveRange(sectionActivities);

            // 2. Delete SectionRecords associated with these sections
            var sectionRecords = await db.Set<SectionRecord>()
                .Where(sr => sectionIds.Contains(sr.SectionId ?? -1))
                .ToListAsync();

            // For SectionRecords, we must also clean up dependent ActivityRecords
            if (sectionRecords.Any())
            {
                var sectionRecordIds = sectionRecords.Select(sr => sr.Id).ToList();
                var activityRecords = await db.Set<ActivityRecord>()
                    .Where(ar => sectionRecordIds.Contains(ar.SectionRecordId))
                    .ToListAsync();

                await DeleteActivityRecordsDeepAsync(db, activityRecords);
                db.RemoveRange(sectionRecords);
            }

            // 3. Delete the Sections
            db.RemoveRange(unusedSections);
            await db.SaveChangesAsync();
        }

        private async Task CleanupOrphanedActivitiesAsync(DbContext db)
        {
            // Find activities that are NOT in any SectionActivity
            var unusedActivities = await db.Set<Activity>()
                .Where(a => !a.SectionActivities.Any())
                .ToListAsync();

            if (!unusedActivities.Any()) return;

            var activityIds = unusedActivities.Select(a => a.Id).ToList();

            // 1. Delete Child Relations

            // Materials
            var materials = await db.Set<ActivityMaterial>().Where(x => activityIds.Contains(x.ActivityId)).ToListAsync();
            db.RemoveRange(materials);

            // Practices (Link only)
            var practices = await db.Set<ActivityPractice>().Where(x => activityIds.Contains(x.ActivityId)).ToListAsync();
            db.RemoveRange(practices);

            // Quizzes (Link only)
            var quizzes = await db.Set<ActivityQuiz>().Where(x => activityIds.Contains(x.ActivityId)).ToListAsync();
            db.RemoveRange(quizzes);

            // Sessions
            var sessions = await db.Set<ActivitySession>().Where(x => activityIds.Contains(x.ActivityId)).ToListAsync();
            db.RemoveRange(sessions);

            // 2. Delete ActivityRecords associated with these activities (if any remain)
            var records = await db.Set<ActivityRecord>().Where(x => activityIds.Contains(x.ActivityId ?? -1)).ToListAsync();
            await DeleteActivityRecordsDeepAsync(db, records);

            // 3. Delete the Activities
            db.RemoveRange(unusedActivities);
            await db.SaveChangesAsync();
        }

        private async Task DeleteActivityRecordsDeepAsync(DbContext db, List<ActivityRecord> records)
        {
            if (!records.Any()) return;

            var recordIds = records.Select(r => r.Id).ToList();

            // Delete Instructor Feedbacks
            var feedbacks = await db.Set<InstructorFeedback>().Where(x => recordIds.Contains(x.ActivityRecordId)).ToListAsync();
            db.RemoveRange(feedbacks);

            // Delete Quiz Attempts (Deep delete)
            var quizAttempts = await db.Set<QuizAttempt>().Where(x => recordIds.Contains(x.ActivityRecordId)).ToListAsync();
            if (quizAttempts.Any())
            {
                var qaIds = quizAttempts.Select(q => q.Id).ToList();

                // Quiz Attempt Questions
                var qaQuestions = await db.Set<QuizAttemptQuestion>().Where(x => qaIds.Contains(x.QuizAttemptId)).ToListAsync();
                if (qaQuestions.Any())
                {
                    var qaqIds = qaQuestions.Select(qq => qq.Id).ToList();
                    var qaAnswers = await db.Set<QuizAttemptAnswer>().Where(x => qaqIds.Contains(x.QuizAttemptQuestionId)).ToListAsync();
                    db.RemoveRange(qaAnswers);
                    db.RemoveRange(qaQuestions);
                }
                db.RemoveRange(quizAttempts);
            }

            // Delete Practice Attempts (Deep delete)
            var practiceAttempts = await db.Set<PracticeAttempt>().Where(x => recordIds.Contains(x.ActivityRecordId)).ToListAsync();
            if (practiceAttempts.Any())
            {
                var paIds = practiceAttempts.Select(p => p.Id).ToList();
                var paTasks = await db.Set<PracticeAttemptTask>().Where(x => paIds.Contains(x.PracticeAttemptId)).ToListAsync();
                db.RemoveRange(paTasks);
                db.RemoveRange(practiceAttempts);
            }

            // Finally delete the activity records
            db.RemoveRange(records);
        }
    }
}