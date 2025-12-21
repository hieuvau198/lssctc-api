using Lssctc.Share.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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

                // 3. Delete enrollment-related child data (Reverse order of dependency)
                if (enrollmentIds.Any())
                {
                    // --- Final Exam Student Data (Deepest first) ---
                    var finalExams = await _uow.FinalExamRepository.GetAllAsQueryable()
                        .Where(f => enrollmentIds.Contains(f.EnrollmentId))
                        .ToListAsync();

                    if (finalExams.Any())
                    {
                        var finalExamIds = finalExams.Select(f => f.Id).ToList();

                        var partials = await _uow.FinalExamPartialRepository.GetAllAsQueryable()
                            .Where(p => finalExamIds.Contains(p.FinalExamId))
                            .ToListAsync();

                        if (partials.Any())
                        {
                            var partialIds = partials.Select(p => p.Id).ToList();

                            // FeSimulations & SeTasks
                            var simulations = await _uow.FeSimulationRepository.GetAllAsQueryable()
                                .Where(s => partialIds.Contains(s.FinalExamPartialId))
                                .ToListAsync();

                            if (simulations.Any())
                            {
                                var simIds = simulations.Select(s => s.Id).ToList();
                                var seTasks = await _uow.SeTaskRepository.GetAllAsQueryable()
                                    .Where(t => simIds.Contains(t.FeSimulationId))
                                    .ToListAsync();
                                foreach (var x in seTasks) await _uow.SeTaskRepository.DeleteAsync(x);
                                foreach (var x in simulations) await _uow.FeSimulationRepository.DeleteAsync(x);
                            }

                            // FeTheories
                            var theories = await _uow.FeTheoryRepository.GetAllAsQueryable()
                                .Where(t => partialIds.Contains(t.FinalExamPartialId))
                                .ToListAsync();
                            foreach (var x in theories) await _uow.FeTheoryRepository.DeleteAsync(x);

                            // PeChecklists
                            var checklists = await _uow.PeChecklistRepository.GetAllAsQueryable()
                                .Where(c => partialIds.Contains(c.FinalExamPartialId))
                                .ToListAsync();
                            foreach (var x in checklists) await _uow.PeChecklistRepository.DeleteAsync(x);

                            // Delete Partials
                            foreach (var x in partials) await _uow.FinalExamPartialRepository.DeleteAsync(x);
                        }

                        // Delete Final Exams
                        foreach (var x in finalExams) await _uow.FinalExamRepository.DeleteAsync(x);
                    }

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
                }

                // --- Class-Level Data Cleanup ---

                // 4.1 Timeslots & Attendances
                var timeslots = await _uow.TimeslotRepository.GetAllAsQueryable()
                    .Where(t => t.ClassId == classId)
                    .ToListAsync();

                var timeslotIds = timeslots.Select(t => t.Id).ToList();
                if (timeslotIds.Any())
                {
                    var attendances = await _uow.AttendanceRepository.GetAllAsQueryable()
                        .Where(a => timeslotIds.Contains(a.TimeslotId))
                        .ToListAsync();
                    foreach (var x in attendances) await _uow.AttendanceRepository.DeleteAsync(x);
                }

                // 4.2 Enrollments (Safe to delete now that Attendances are gone)
                foreach (var x in enrollments) await _uow.EnrollmentRepository.DeleteAsync(x);

                // 4.3 Timeslots
                foreach (var x in timeslots) await _uow.TimeslotRepository.DeleteAsync(x);

                // --- NEW: Final Exam Templates Cleanup ---
                // Must delete these before Class because FinalExamTemplate depends on Class
                var finalExamTemplates = await _uow.FinalExamTemplateRepository.GetAllAsQueryable()
                    .Where(t => t.ClassId == classId)
                    .ToListAsync();

                if (finalExamTemplates.Any())
                {
                    var templateIds = finalExamTemplates.Select(t => t.Id).ToList();

                    // Delete Partial Templates first (FK to Template)
                    var partialTemplates = await _uow.FinalExamPartialsTemplateRepository.GetAllAsQueryable()
                        .Where(pt => templateIds.Contains(pt.FinalExamTemplateId))
                        .ToListAsync();

                    foreach (var x in partialTemplates) await _uow.FinalExamPartialsTemplateRepository.DeleteAsync(x);

                    // Delete Templates
                    foreach (var x in finalExamTemplates) await _uow.FinalExamTemplateRepository.DeleteAsync(x);
                }

                // 4.4 Activity Sessions
                var activitySessions = await _uow.ActivitySessionRepository.GetAllAsQueryable()
                    .Where(s => s.ClassId == classId)
                    .ToListAsync();
                foreach (var x in activitySessions) await _uow.ActivitySessionRepository.DeleteAsync(x);

                // 4.5 Class Instructors
                var instructors = await _uow.ClassInstructorRepository.GetAllAsQueryable()
                    .Where(ci => ci.ClassId == classId)
                    .ToListAsync();
                foreach (var x in instructors) await _uow.ClassInstructorRepository.DeleteAsync(x);

                // 5. Delete Class
                await _uow.ClassRepository.DeleteAsync(classEntity);

                // 6. Commit
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