using Lssctc.Share.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Lssctc.ProgramManagement.ClassManage.Helpers
{
    public class ClassCleanupHandler
    {
        private readonly IUnitOfWork _uow;

        public ClassCleanupHandler(IUnitOfWork uow)
        {
            _uow = uow;
        }

        public async Task DeleteClassDataRecursiveAsync(int classId)
        {
            // Begin database transaction
            var dbContext = _uow.GetDbContext();
            await using var transaction = await dbContext.Database.BeginTransactionAsync();

            try
            {
                // 1. Check if class exists
                var classEntity = await _uow.ClassRepository
                    .GetAllAsQueryable()
                    .FirstOrDefaultAsync(c => c.Id == classId);

                if (classEntity == null)
                    throw new KeyNotFoundException($"Class with ID {classId} not found.");

                // 2. Get all enrollments
                var enrollments = await _uow.EnrollmentRepository
                    .GetAllAsQueryable()
                    .Where(e => e.ClassId == classId)
                    .ToListAsync();

                var enrollmentIds = enrollments.Select(e => e.Id).ToList();

                // 3. Delete child data (Reverse order of dependency)

                if (enrollmentIds.Any())
                {
                    // 3.1 Quiz Answers
                    var quizAnswers = await _uow.QuizAttemptAnswerRepository.GetAllAsQueryable()
                        .Where(a => enrollmentIds.Contains(a.QuizAttemptQuestion.QuizAttempt.ActivityRecord.SectionRecord.LearningProgress.EnrollmentId))
                        .ToListAsync();
                    foreach (var x in quizAnswers) await _uow.QuizAttemptAnswerRepository.DeleteAsync(x);

                    // 3.2 Quiz Questions
                    var quizQuestions = await _uow.QuizAttemptQuestionRepository.GetAllAsQueryable()
                        .Where(q => enrollmentIds.Contains(q.QuizAttempt.ActivityRecord.SectionRecord.LearningProgress.EnrollmentId))
                        .ToListAsync();
                    foreach (var x in quizQuestions) await _uow.QuizAttemptQuestionRepository.DeleteAsync(x);

                    // 3.3 Quiz Attempts
                    var quizAttempts = await _uow.QuizAttemptRepository.GetAllAsQueryable()
                        .Where(q => enrollmentIds.Contains(q.ActivityRecord.SectionRecord.LearningProgress.EnrollmentId))
                        .ToListAsync();
                    foreach (var x in quizAttempts) await _uow.QuizAttemptRepository.DeleteAsync(x);

                    // 3.4 Practice Tasks
                    var practiceTasks = await _uow.PracticeAttemptTaskRepository.GetAllAsQueryable()
                         .Where(p => enrollmentIds.Contains(p.PracticeAttempt.ActivityRecord.SectionRecord.LearningProgress.EnrollmentId))
                         .ToListAsync();
                    foreach (var x in practiceTasks) await _uow.PracticeAttemptTaskRepository.DeleteAsync(x);

                    // 3.5 Practice Attempts
                    var practiceAttempts = await _uow.PracticeAttemptRepository.GetAllAsQueryable()
                        .Where(p => enrollmentIds.Contains(p.ActivityRecord.SectionRecord.LearningProgress.EnrollmentId))
                        .ToListAsync();
                    foreach (var x in practiceAttempts) await _uow.PracticeAttemptRepository.DeleteAsync(x);

                    // 3.6 Instructor Feedback
                    var feedbacks = await _uow.InstructorFeedbackRepository.GetAllAsQueryable()
                        .Where(f => enrollmentIds.Contains(f.ActivityRecord.SectionRecord.LearningProgress.EnrollmentId))
                        .ToListAsync();
                    foreach (var x in feedbacks) await _uow.InstructorFeedbackRepository.DeleteAsync(x);

                    // 3.7 Activity Records
                    var activityRecords = await _uow.ActivityRecordRepository.GetAllAsQueryable()
                        .Where(a => enrollmentIds.Contains(a.SectionRecord.LearningProgress.EnrollmentId))
                        .ToListAsync();
                    foreach (var x in activityRecords) await _uow.ActivityRecordRepository.DeleteAsync(x);

                    // 3.8 Section Records
                    var sectionRecords = await _uow.SectionRecordRepository.GetAllAsQueryable()
                        .Where(s => enrollmentIds.Contains(s.LearningProgress.EnrollmentId))
                        .ToListAsync();
                    foreach (var x in sectionRecords) await _uow.SectionRecordRepository.DeleteAsync(x);

                    // 3.9 Certificates
                    var certs = await _uow.TraineeCertificateRepository.GetAllAsQueryable()
                        .Where(c => enrollmentIds.Contains(c.EnrollmentId))
                        .ToListAsync();
                    foreach (var x in certs) await _uow.TraineeCertificateRepository.DeleteAsync(x);

                    // 3.10 Learning Progress
                    var progress = await _uow.LearningProgressRepository.GetAllAsQueryable()
                        .Where(l => enrollmentIds.Contains(l.EnrollmentId))
                        .ToListAsync();
                    foreach (var x in progress) await _uow.LearningProgressRepository.DeleteAsync(x);

                    // 3.11 Enrollments
                    foreach (var x in enrollments) await _uow.EnrollmentRepository.DeleteAsync(x);
                }

                // 3.12 Class Instructors
                var instructors = await _uow.ClassInstructorRepository.GetAllAsQueryable()
                    .Where(ci => ci.ClassId == classId)
                    .ToListAsync();
                foreach (var x in instructors) await _uow.ClassInstructorRepository.DeleteAsync(x);

                // 4. Delete Class
                await _uow.ClassRepository.DeleteAsync(classEntity);

                // 5. Commit
                await _uow.SaveChangesAsync();
                await transaction.CommitAsync();
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                throw;
            }
        }
    }
}