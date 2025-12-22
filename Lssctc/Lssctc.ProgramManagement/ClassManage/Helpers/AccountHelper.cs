using Lssctc.Share.Contexts;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Lssctc.ProgramManagement.ClassManage.Helpers
{
    public class AccountHelper
    {
        private readonly LssctcDbContext _context;

        public AccountHelper(LssctcDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Completely removes a user and all related data (Profiles, Enrollments, Records, Attempts, etc.) from the system.
        /// </summary>
        /// <param name="userId">The ID of the user to remove.</param>
        public async Task HardDeleteUserAccountAsync(int userId)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var user = await _context.Users
                    .Include(u => u.Trainee)
                    .Include(u => u.Instructor)
                    .Include(u => u.Admin)
                    .Include(u => u.SimulationManager)
                    .FirstOrDefaultAsync(u => u.Id == userId);

                if (user == null) return;

                // ============================
                // 1. Handle Trainee Data
                // ============================
                if (user.Trainee != null)
                {
                    // Fetch all enrollments for this trainee
                    var enrollments = await _context.Enrollments
                        .Where(e => e.TraineeId == userId)
                        .ToListAsync();

                    foreach (var enrollment in enrollments)
                    {
                        // 1.1 Remove Final Exam Data (Deep Hierarchy)
                        var finalExams = await _context.FinalExams
                            .Where(fe => fe.EnrollmentId == enrollment.Id)
                            .Include(fe => fe.FinalExamPartials)
                                .ThenInclude(fep => fep.FeSimulations)
                                    .ThenInclude(fes => fes.SeTasks)
                            .Include(fe => fe.FinalExamPartials)
                                .ThenInclude(fep => fep.FeTheories)
                            .Include(fe => fe.FinalExamPartials)
                                .ThenInclude(fep => fep.PeChecklists)
                            .ToListAsync();

                        foreach (var fe in finalExams)
                        {
                            foreach (var fep in fe.FinalExamPartials)
                            {
                                // Remove Simulation Tasks and Simulations
                                foreach (var sim in fep.FeSimulations)
                                {
                                    _context.SeTasks.RemoveRange(sim.SeTasks);
                                }
                                _context.FeSimulations.RemoveRange(fep.FeSimulations);

                                // Remove Theories and Checklists
                                _context.FeTheories.RemoveRange(fep.FeTheories);
                                _context.PeChecklists.RemoveRange(fep.PeChecklists);
                            }
                            // Remove Partials
                            _context.FinalExamPartials.RemoveRange(fe.FinalExamPartials);
                        }
                        _context.FinalExams.RemoveRange(finalExams);

                        // 1.2 Remove Learning Progress (Records, Attempts, etc.)
                        var learningProgresses = await _context.LearningProgresses
                            .Where(lp => lp.EnrollmentId == enrollment.Id)
                            .ToListAsync();

                        foreach (var lp in learningProgresses)
                        {
                            var sectionRecords = await _context.SectionRecords
                                .Where(sr => sr.LearningProgressId == lp.Id)
                                .ToListAsync();

                            foreach (var sr in sectionRecords)
                            {
                                var activityRecords = await _context.ActivityRecords
                                    .Where(ar => ar.SectionRecordId == sr.Id)
                                    .ToListAsync();

                                foreach (var ar in activityRecords)
                                {
                                    // Remove Feedbacks received on this record
                                    var feedbacks = await _context.InstructorFeedbacks
                                        .Where(f => f.ActivityRecordId == ar.Id)
                                        .ToListAsync();
                                    _context.InstructorFeedbacks.RemoveRange(feedbacks);

                                    // Remove Practice Attempts & Tasks
                                    var practiceAttempts = await _context.PracticeAttempts
                                        .Where(pa => pa.ActivityRecordId == ar.Id)
                                        .Include(pa => pa.PracticeAttemptTasks)
                                        .ToListAsync();
                                    foreach (var pa in practiceAttempts)
                                    {
                                        _context.PracticeAttemptTasks.RemoveRange(pa.PracticeAttemptTasks);
                                    }
                                    _context.PracticeAttempts.RemoveRange(practiceAttempts);

                                    // Remove Quiz Attempts, Questions & Answers
                                    var quizAttempts = await _context.QuizAttempts
                                        .Where(qa => qa.ActivityRecordId == ar.Id)
                                        .Include(qa => qa.QuizAttemptQuestions)
                                            .ThenInclude(qaq => qaq.QuizAttemptAnswers)
                                        .ToListAsync();
                                    foreach (var qa in quizAttempts)
                                    {
                                        foreach (var qaq in qa.QuizAttemptQuestions)
                                        {
                                            _context.QuizAttemptAnswers.RemoveRange(qaq.QuizAttemptAnswers);
                                        }
                                        _context.QuizAttemptQuestions.RemoveRange(qa.QuizAttemptQuestions);
                                    }
                                    _context.QuizAttempts.RemoveRange(quizAttempts);
                                }
                                _context.ActivityRecords.RemoveRange(activityRecords);
                            }
                            _context.SectionRecords.RemoveRange(sectionRecords);
                        }
                        _context.LearningProgresses.RemoveRange(learningProgresses);

                        // 1.3 Remove Attendances
                        var attendances = await _context.Attendances
                            .Where(a => a.EnrollmentId == enrollment.Id)
                            .ToListAsync();
                        _context.Attendances.RemoveRange(attendances);

                        // 1.4 Remove Certificates
                        var certificates = await _context.TraineeCertificates
                            .Where(c => c.EnrollmentId == enrollment.Id)
                            .ToListAsync();
                        _context.TraineeCertificates.RemoveRange(certificates);
                    }

                    // Remove Enrollments
                    _context.Enrollments.RemoveRange(enrollments);

                    // Remove Trainee Profile
                    var traineeProfile = await _context.TraineeProfiles.FindAsync(userId);
                    if (traineeProfile != null)
                    {
                        _context.TraineeProfiles.Remove(traineeProfile);
                    }

                    // Remove Trainee
                    _context.Trainees.Remove(user.Trainee);
                }

                // ============================
                // 2. Handle Instructor Data
                // ============================
                if (user.Instructor != null)
                {
                    // Remove Class Instructor Assignments
                    var classInstructors = await _context.ClassInstructors
                        .Where(ci => ci.InstructorId == userId)
                        .ToListAsync();
                    _context.ClassInstructors.RemoveRange(classInstructors);

                    // Remove Material Authorship
                    var materialAuthors = await _context.MaterialAuthors
                        .Where(ma => ma.InstructorId == userId)
                        .ToListAsync();
                    _context.MaterialAuthors.RemoveRange(materialAuthors);

                    // Remove Quiz Authorship
                    var quizAuthors = await _context.QuizAuthors
                        .Where(qa => qa.InstructorId == userId)
                        .ToListAsync();
                    _context.QuizAuthors.RemoveRange(quizAuthors);

                    // Remove Feedbacks given by this Instructor
                    var givenFeedbacks = await _context.InstructorFeedbacks
                        .Where(f => f.InstructorId == userId)
                        .ToListAsync();
                    _context.InstructorFeedbacks.RemoveRange(givenFeedbacks);

                    // Remove Instructor Profile
                    var instructorProfile = await _context.InstructorProfiles.FindAsync(userId);
                    if (instructorProfile != null)
                    {
                        _context.InstructorProfiles.Remove(instructorProfile);
                    }

                    // Remove Instructor
                    _context.Instructors.Remove(user.Instructor);
                }

                // ============================
                // 3. Handle Other Roles
                // ============================
                if (user.Admin != null)
                {
                    _context.Admins.Remove(user.Admin);
                }

                if (user.SimulationManager != null)
                {
                    _context.SimulationManagers.Remove(user.SimulationManager);
                }

                // ============================
                // 4. Remove User Account
                // ============================
                _context.Users.Remove(user);

                await _context.SaveChangesAsync();
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