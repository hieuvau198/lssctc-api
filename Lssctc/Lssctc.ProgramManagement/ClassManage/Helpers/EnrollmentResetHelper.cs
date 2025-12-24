using Lssctc.Share.Contexts;
using Lssctc.Share.Entities;
using Lssctc.Share.Enums;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Lssctc.ProgramManagement.ClassManage.Helpers
{
    public class EnrollmentResetHelper
    {
        private readonly LssctcDbContext _context;

        public EnrollmentResetHelper(LssctcDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Resets attendance status to 'NotStarted' for all timeslots in an enrollment.
        /// </summary>
        public async Task ResetAttendanceAsync(int enrollmentId)
        {
            var enrollment = await _context.Enrollments
                .Include(e => e.Attendances)
                .FirstOrDefaultAsync(e => e.Id == enrollmentId);

            if (enrollment == null) throw new Exception("Enrollment not found");

            foreach (var attendance in enrollment.Attendances)
            {
                attendance.Status = (int)AttendanceStatusEnum.NotStarted;
            }

            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Resets learning progress, section records, activity records, and attempts to 'NotStarted' or initial values.
        /// </summary>
        public async Task ResetLearningProgressAsync(int enrollmentId)
        {
            var enrollment = await _context.Enrollments
                .Include(e => e.LearningProgresses)
                    .ThenInclude(lp => lp.SectionRecords)
                        .ThenInclude(sr => sr.ActivityRecords)
                            .ThenInclude(ar => ar.QuizAttempts)
                .Include(e => e.LearningProgresses)
                    .ThenInclude(lp => lp.SectionRecords)
                        .ThenInclude(sr => sr.ActivityRecords)
                            .ThenInclude(ar => ar.PracticeAttempts)
                .FirstOrDefaultAsync(e => e.Id == enrollmentId);

            if (enrollment == null) throw new Exception("Enrollment not found");

            foreach (var progress in enrollment.LearningProgresses)
            {
                // Reset Progress Totals
                progress.Status = (int)LearningProgressStatusEnum.NotStarted;
                progress.ProgressPercentage = 0;
                progress.TheoryScore = 0;
                progress.PracticalScore = 0;
                progress.FinalScore = 0;
                progress.LastUpdated = DateTime.Now;

                foreach (var sectionRecord in progress.SectionRecords)
                {
                    // Reset Section
                    sectionRecord.IsCompleted = false;
                    sectionRecord.IsTraineeAttended = false;
                    sectionRecord.Progress = 0;

                    foreach (var activityRecord in sectionRecord.ActivityRecords)
                    {
                        // Reset Activity
                        activityRecord.Status = (int)ActivityRecordStatusEnum.NotStarted;
                        activityRecord.Score = 0;
                        activityRecord.IsCompleted = false;
                        activityRecord.CompletedDate = null;

                        // Reset Quiz Attempts
                        foreach (var quizAttempt in activityRecord.QuizAttempts)
                        {
                            quizAttempt.Status = (int)QuizAttemptStatusEnum.InProgress; // Revert to in-progress or initial
                            quizAttempt.IsPass = null;
                            quizAttempt.AttemptScore = 0;
                        }

                        // Reset Practice Attempts
                        foreach (var practiceAttempt in activityRecord.PracticeAttempts)
                        {
                            practiceAttempt.AttemptStatus = (int)ActivityRecordStatusEnum.NotStarted;
                            practiceAttempt.IsPass = null;
                            practiceAttempt.Score = 0;
                        }
                    }
                }
            }

            // Revert Enrollment Status if it was completed via progress
            if (enrollment.Status == (int)EnrollmentStatusEnum.Completed || enrollment.Status == (int)EnrollmentStatusEnum.Failed)
            {
                enrollment.Status = (int)EnrollmentStatusEnum.Inprogress;
            }

            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Resets the final exam, its partials (SE, TE, PE), and checklist items to 'NotYet' started.
        /// </summary>
        public async Task ResetFinalExamAsync(int enrollmentId)
        {
            var enrollment = await _context.Enrollments
                .Include(e => e.FinalExams)
                    .ThenInclude(fe => fe.FinalExamPartials)
                        .ThenInclude(fep => fep.PeChecklists)
                .Include(e => e.FinalExams)
                    .ThenInclude(fe => fe.FinalExamPartials)
                        .ThenInclude(fep => fep.FeSimulations)
                            .ThenInclude(s => s.SeTasks)
                .FirstOrDefaultAsync(e => e.Id == enrollmentId);

            if (enrollment == null) throw new Exception("Enrollment not found");

            foreach (var finalExam in enrollment.FinalExams)
            {
                // Reset Final Exam Root
                finalExam.IsPass = null;
                finalExam.TotalMarks = 0;
                finalExam.CompleteTime = null;
                finalExam.Status = (int)FinalExamStatusEnum.NotYet;

                foreach (var partial in finalExam.FinalExamPartials)
                {
                    // Reset Partials
                    partial.IsPass = null;
                    partial.Status = (int)FinalExamPartialStatus.NotYet;
                    partial.Marks = 0;
                    partial.CompleteTime = null;

                    // Reset Simulation Tasks
                    if (partial.FeSimulations != null)
                    {
                        foreach (var sim in partial.FeSimulations)
                        {
                            foreach (var task in sim.SeTasks)
                            {
                                task.IsPass = null;
                                task.Status = 0; // Assuming 0 is NotStarted/Pending
                                task.CompleteTime = null;
                            }
                        }
                    }

                    // Reset Checklist Items
                    if (partial.PeChecklists != null)
                    {
                        foreach (var checklist in partial.PeChecklists)
                        {
                            checklist.IsPass = false;
                        }
                    }
                }
            }

            // Revert Enrollment Status if it was completed via Final Exam
            if (enrollment.Status == (int)EnrollmentStatusEnum.Completed || enrollment.Status == (int)EnrollmentStatusEnum.Failed)
            {
                enrollment.Status = (int)EnrollmentStatusEnum.Inprogress;
            }

            await _context.SaveChangesAsync();
        }
    }
}