using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Lssctc.Share.Entities;
using Lssctc.Share.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Lssctc.ProgramManagement.ClassManage.Helpers
{
    public class ClassCustomizeService : IClassCustomizeService
    {
        private readonly IUnitOfWork _unitOfWork;

        public ClassCustomizeService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task DeleteClassCompletelyAsync(int classId)
        {
            // We use the Execution Strategy or a Transaction via the DbContext accessed through UoW
            // to ensure atomicity of this large delete operation.
            var dbContext = _unitOfWork.GetDbContext();
            using var transaction = await dbContext.Database.BeginTransactionAsync();

            try
            {
                var classEntity = await _unitOfWork.ClassRepository.GetByIdAsync(classId);

                if (classEntity == null)
                {
                    return; // Class doesn't exist, nothing to do
                }

                // 1. Identify all Enrollments for this class
                var enrollments = await _unitOfWork.EnrollmentRepository
                    .GetAllAsQueryable()
                    .Where(e => e.ClassId == classId)
                    .ToListAsync();

                var enrollmentIds = enrollments.Select(e => e.Id).ToList();

                if (enrollmentIds.Any())
                {
                    // --- A. Delete Trainee Certificates ---
                    var traineeCerts = await _unitOfWork.TraineeCertificateRepository
                        .GetAllAsQueryable()
                        .Where(tc => enrollmentIds.Contains(tc.EnrollmentId))
                        .ToListAsync();

                    foreach (var cert in traineeCerts)
                    {
                        await _unitOfWork.TraineeCertificateRepository.DeleteAsync(cert);
                    }

                    // --- B. Delete Final Exam Tree ---
                    var finalExams = await _unitOfWork.FinalExamRepository
                        .GetAllAsQueryable()
                        .Where(fe => enrollmentIds.Contains(fe.EnrollmentId))
                        .ToListAsync();

                    var finalExamIds = finalExams.Select(fe => fe.Id).ToList();

                    if (finalExamIds.Any())
                    {
                        var partials = await _unitOfWork.FinalExamPartialRepository
                            .GetAllAsQueryable()
                            .Where(fep => finalExamIds.Contains(fep.FinalExamId))
                            .ToListAsync();

                        var partialIds = partials.Select(p => p.Id).ToList();

                        if (partialIds.Any())
                        {
                            // Delete SE Tasks & Simulations
                            var feSimulations = await _unitOfWork.FeSimulationRepository
                                .GetAllAsQueryable()
                                .Where(s => partialIds.Contains(s.FinalExamPartialId))
                                .ToListAsync();

                            var feSimIds = feSimulations.Select(s => s.Id).ToList();
                            if (feSimIds.Any())
                            {
                                var seTasks = await _unitOfWork.SeTaskRepository
                                    .GetAllAsQueryable()
                                    .Where(t => feSimIds.Contains(t.FeSimulationId))
                                    .ToListAsync();

                                foreach (var item in seTasks) await _unitOfWork.SeTaskRepository.DeleteAsync(item);
                            }

                            foreach (var item in feSimulations) await _unitOfWork.FeSimulationRepository.DeleteAsync(item);

                            // Delete FE Theories
                            var feTheories = await _unitOfWork.FeTheoryRepository
                                .GetAllAsQueryable()
                                .Where(t => partialIds.Contains(t.FinalExamPartialId))
                                .ToListAsync();
                            foreach (var item in feTheories) await _unitOfWork.FeTheoryRepository.DeleteAsync(item);

                            // Delete PE Checklists
                            // Note: PeChecklistRepository is missing from IUnitOfWork definition provided, 
                            // so we access it via the DbContext directly to ensure cleanup.
                            var peChecklists = await dbContext.Set<PeChecklist>()
                                .Where(p => partialIds.Contains(p.FinalExamPartialId))
                                .ToListAsync();
                            dbContext.Set<PeChecklist>().RemoveRange(peChecklists);
                        }

                        foreach (var item in partials) await _unitOfWork.FinalExamPartialRepository.DeleteAsync(item);
                    }

                    foreach (var item in finalExams) await _unitOfWork.FinalExamRepository.DeleteAsync(item);

                    // --- C. Delete Learning Progress Tree ---
                    var learningProgresses = await _unitOfWork.LearningProgressRepository
                        .GetAllAsQueryable()
                        .Where(lp => enrollmentIds.Contains(lp.EnrollmentId))
                        .ToListAsync();

                    var lpIds = learningProgresses.Select(lp => lp.Id).ToList();

                    if (lpIds.Any())
                    {
                        var sectionRecords = await _unitOfWork.SectionRecordRepository
                            .GetAllAsQueryable()
                            .Where(sr => lpIds.Contains(sr.LearningProgressId))
                            .ToListAsync();

                        var srIds = sectionRecords.Select(sr => sr.Id).ToList();

                        if (srIds.Any())
                        {
                            var activityRecords = await _unitOfWork.ActivityRecordRepository
                                .GetAllAsQueryable()
                                .Where(ar => srIds.Contains(ar.SectionRecordId))
                                .ToListAsync();

                            var arIds = activityRecords.Select(ar => ar.Id).ToList();

                            if (arIds.Any())
                            {
                                // Instructor Feedbacks
                                var feedbacks = await _unitOfWork.InstructorFeedbackRepository
                                    .GetAllAsQueryable()
                                    .Where(f => arIds.Contains(f.ActivityRecordId))
                                    .ToListAsync();
                                foreach (var item in feedbacks) await _unitOfWork.InstructorFeedbackRepository.DeleteAsync(item);

                                // Practice Attempts & Tasks
                                var practiceAttempts = await _unitOfWork.PracticeAttemptRepository
                                    .GetAllAsQueryable()
                                    .Where(pa => arIds.Contains(pa.ActivityRecordId))
                                    .ToListAsync();

                                var paIds = practiceAttempts.Select(pa => pa.Id).ToList();
                                if (paIds.Any())
                                {
                                    var paTasks = await _unitOfWork.PracticeAttemptTaskRepository
                                        .GetAllAsQueryable()
                                        .Where(pat => paIds.Contains(pat.PracticeAttemptId))
                                        .ToListAsync();
                                    foreach (var item in paTasks) await _unitOfWork.PracticeAttemptTaskRepository.DeleteAsync(item);
                                }
                                foreach (var item in practiceAttempts) await _unitOfWork.PracticeAttemptRepository.DeleteAsync(item);

                                // Quiz Attempts & Questions/Answers
                                var quizAttempts = await _unitOfWork.QuizAttemptRepository
                                    .GetAllAsQueryable()
                                    .Where(qa => arIds.Contains(qa.ActivityRecordId))
                                    .ToListAsync();

                                var qaIds = quizAttempts.Select(qa => qa.Id).ToList();
                                if (qaIds.Any())
                                {
                                    var qaQuestions = await _unitOfWork.QuizAttemptQuestionRepository
                                        .GetAllAsQueryable()
                                        .Where(qaq => qaIds.Contains(qaq.QuizAttemptId))
                                        .ToListAsync();

                                    var qaqIds = qaQuestions.Select(q => q.Id).ToList();
                                    if (qaqIds.Any())
                                    {
                                        var qaAnswers = await _unitOfWork.QuizAttemptAnswerRepository
                                            .GetAllAsQueryable()
                                            .Where(qaa => qaqIds.Contains(qaa.QuizAttemptQuestionId))
                                            .ToListAsync();
                                        foreach (var item in qaAnswers) await _unitOfWork.QuizAttemptAnswerRepository.DeleteAsync(item);
                                    }
                                    foreach (var item in qaQuestions) await _unitOfWork.QuizAttemptQuestionRepository.DeleteAsync(item);
                                }
                                foreach (var item in quizAttempts) await _unitOfWork.QuizAttemptRepository.DeleteAsync(item);
                            }

                            foreach (var item in activityRecords) await _unitOfWork.ActivityRecordRepository.DeleteAsync(item);
                        }

                        foreach (var item in sectionRecords) await _unitOfWork.SectionRecordRepository.DeleteAsync(item);
                    }

                    foreach (var item in learningProgresses) await _unitOfWork.LearningProgressRepository.DeleteAsync(item);

                    // --- D. Delete Attendances ---
                    var attendances = await _unitOfWork.AttendanceRepository
                        .GetAllAsQueryable()
                        .Where(a => enrollmentIds.Contains(a.EnrollmentId))
                        .ToListAsync();
                    foreach (var item in attendances) await _unitOfWork.AttendanceRepository.DeleteAsync(item);

                    // --- E. Delete Enrollments ---
                    foreach (var item in enrollments) await _unitOfWork.EnrollmentRepository.DeleteAsync(item);
                }

                // 2. Delete Class Direct Children

                // A. Activity Sessions
                var activitySessions = await _unitOfWork.ActivitySessionRepository
                    .GetAllAsQueryable()
                    .Where(x => x.ClassId == classId)
                    .ToListAsync();
                foreach (var item in activitySessions) await _unitOfWork.ActivitySessionRepository.DeleteAsync(item);

                // B. Timeslots
                var timeslots = await _unitOfWork.TimeslotRepository
                    .GetAllAsQueryable()
                    .Where(t => t.ClassId == classId)
                    .ToListAsync();
                foreach (var item in timeslots) await _unitOfWork.TimeslotRepository.DeleteAsync(item);

                // C. Class Instructors
                var classInstructors = await _unitOfWork.ClassInstructorRepository
                    .GetAllAsQueryable()
                    .Where(ci => ci.ClassId == classId)
                    .ToListAsync();
                foreach (var item in classInstructors) await _unitOfWork.ClassInstructorRepository.DeleteAsync(item);

                // 3. Delete The Class and potentially ClassCode
                var classCodeId = classEntity.ClassCodeId;

                await _unitOfWork.ClassRepository.DeleteAsync(classEntity);

                // Check if we can delete the ClassCode (if it's not used by any other class)
                // We check count. Since we just deleted one (in tracking), if there are 0 left in DB (effectively), we delete.
                // However, since SaveChanges isn't called, CountAsync might still see the old one unless filtered.
                // Safer logic: Count all with that ID. If it is 1 (the one we are deleting), then we can delete the code too.
                if (classCodeId.HasValue)
                {
                    var usageCount = await _unitOfWork.ClassRepository
                        .CountAsync(c => c.ClassCodeId == classCodeId.Value);

                    if (usageCount <= 1)
                    {
                        var classCode = await _unitOfWork.ClassCodeRepository.GetByIdAsync(classCodeId.Value);
                        if (classCode != null)
                        {
                            await _unitOfWork.ClassCodeRepository.DeleteAsync(classCode);
                        }
                    }
                }

                await _unitOfWork.SaveChangesAsync();
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