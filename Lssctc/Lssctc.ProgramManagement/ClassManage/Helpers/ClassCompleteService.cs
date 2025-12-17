using Lssctc.ProgramManagement.Certificates.Services;
using Lssctc.Share.Contexts;
using Lssctc.Share.Entities;
using Lssctc.Share.Enums;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Lssctc.ProgramManagement.ClassManage.Helpers
{
    public class ClassCompleteService
    {
        private readonly LssctcDbContext _context;
        private readonly ITraineeCertificatesService _certificateService;

        public ClassCompleteService(LssctcDbContext context, ITraineeCertificatesService certificateService)
        {
            _context = context;
            _certificateService = certificateService;
        }

        /// <summary>
        /// Main orchestrator method to simulate/force the completion of a class.
        /// 1. Completes Attendance.
        /// 2. Completes Learning Progress.
        /// 3. Completes Final Exams (and marks Enrollments as Completed).
        /// 4. Updates Class Status to Completed.
        /// 5. Triggers Certificate Generation and Emailing.
        /// </summary>
        /// <param name="classId"></param>
        /// <returns></returns>
        public async Task AutoCompleteClass(int classId)
        {
            // 1. Validate Class Exists
            var classEntity = await _context.Classes.FindAsync(classId);
            if (classEntity == null) throw new Exception("Class not found");

            // 2. Run Auto-Completion Logic for Components
            // These methods ensure all data (records, attempts, scores) are populated and passed.
            await AutoCompleteAttendance(classId);
            await AutoCompleteLearningProgress(classId);
            await AutoCompleteFinalExam(classId); // This also sets Enrollment Status to Completed

            // 3. Update Class Status
            // We must save this status change before generating certificates, 
            // as the certificate service validates if the class is 'Completed'.
            if (classEntity.Status != (int)ClassStatusEnum.Completed)
            {
                classEntity.Status = (int)ClassStatusEnum.Completed;
                classEntity.EndDate = DateTime.Now; // Ensure EndDate is set if missing
                await _context.SaveChangesAsync();
            }

            // 4. Generate Certificates and Send Emails
            // This service looks for Enrollments that are 'Completed' in a Class that is 'Completed'.
            await _certificateService.CreateTraineeCertificatesForCompleteClass(classId);
        }

        /// <summary>
        /// Auto completes the learning progress for all enrollments in the specified class.
        /// Generates/Updates LearningProgress, SectionRecords, ActivityRecords, and Attempts (Quiz/Practice).
        /// Defaults scores to 100% and statuses to Completed/Passed.
        /// </summary>
        /// <param name="classId">The ID of the class.</param>
        public async Task AutoCompleteLearningProgress(int classId)
        {
            // 1. Fetch Class and Course Structure
            var classInfo = await _context.Classes
                .Include(c => c.ProgramCourse)
                    .ThenInclude(pc => pc.Course)
                        .ThenInclude(co => co.CourseSections)
                            .ThenInclude(cs => cs.Section)
                                .ThenInclude(s => s.SectionActivities)
                                    .ThenInclude(sa => sa.Activity)
                                        .ThenInclude(a => a.ActivityQuizzes) // To get QuizId
             .Include(c => c.ProgramCourse)
                    .ThenInclude(pc => pc.Course)
                        .ThenInclude(co => co.CourseSections)
                            .ThenInclude(cs => cs.Section)
                                .ThenInclude(s => s.SectionActivities)
                                    .ThenInclude(sa => sa.Activity)
                                        .ThenInclude(a => a.ActivityPractices) // To get PracticeId
                .FirstOrDefaultAsync(c => c.Id == classId);

            if (classInfo == null) return;

            var course = classInfo.ProgramCourse.Course;
            if (course == null) return;

            // 2. Fetch Enrollments with existing progress data
            var enrollments = await _context.Enrollments
                .Include(e => e.LearningProgresses)
                    .ThenInclude(lp => lp.SectionRecords)
                        .ThenInclude(sr => sr.ActivityRecords)
                            .ThenInclude(ar => ar.QuizAttempts)
                .Include(e => e.LearningProgresses)
                    .ThenInclude(lp => lp.SectionRecords)
                        .ThenInclude(sr => sr.ActivityRecords)
                            .ThenInclude(ar => ar.PracticeAttempts)
                .Where(e => e.ClassId == classId && e.IsDeleted != true)
                .ToListAsync();

            // 3. Process Each Enrollment
            foreach (var enrollment in enrollments)
            {
                // A. Get or Create LearningProgress
                var progress = enrollment.LearningProgresses.FirstOrDefault(lp => lp.CourseId == course.Id);
                if (progress == null)
                {
                    progress = new LearningProgress
                    {
                        EnrollmentId = enrollment.Id,
                        CourseId = course.Id,
                        StartDate = classInfo.StartDate,
                        Name = $"Progress for {course.Name}",
                        Description = "Auto-generated progress"
                    };
                    _context.LearningProgresses.Add(progress);
                }

                // Update Progress Stats
                progress.Status = (int)LearningProgressStatusEnum.Completed;
                progress.ProgressPercentage = 100;
                progress.TheoryScore = 100; // Default max score
                progress.PracticalScore = 100; // Default max score
                progress.FinalScore = 100;
                progress.LastUpdated = DateTime.Now;

                // B. Iterate through Course Sections
                // Sort by ID or Order if available to keep consistency, though not strictly required for logic
                foreach (var courseSection in course.CourseSections)
                {
                    var section = courseSection.Section;
                    if (section == null) continue;

                    // Get or Create SectionRecord
                    var sectionRecord = progress.SectionRecords.FirstOrDefault(sr => sr.SectionId == section.Id);
                    if (sectionRecord == null)
                    {
                        sectionRecord = new SectionRecord
                        {
                            LearningProgress = progress, // Ensure link if progress is new
                            LearningProgressId = progress.Id,
                            SectionId = section.Id,
                            SectionName = section.SectionTitle,
                            DurationMinutes = section.EstimatedDurationMinutes
                        };
                        // Add to collection if it's a tracked entity, or context will handle it via navigation
                        if (progress.Id != 0) _context.SectionRecords.Add(sectionRecord);
                        else progress.SectionRecords.Add(sectionRecord);
                    }

                    // Update Section Stats
                    sectionRecord.IsCompleted = true;
                    sectionRecord.IsTraineeAttended = true;
                    sectionRecord.Progress = 100;

                    // C. Iterate through Activities
                    foreach (var sectionActivity in section.SectionActivities)
                    {
                        var activity = sectionActivity.Activity;
                        if (activity == null) continue;

                        // Get or Create ActivityRecord
                        var activityRecord = sectionRecord.ActivityRecords.FirstOrDefault(ar => ar.ActivityId == activity.Id);
                        if (activityRecord == null)
                        {
                            activityRecord = new ActivityRecord
                            {
                                SectionRecord = sectionRecord,
                                SectionRecordId = sectionRecord.Id,
                                ActivityId = activity.Id,
                                ActivityType = activity.ActivityType
                            };
                            if (sectionRecord.Id != 0) _context.ActivityRecords.Add(activityRecord);
                            else sectionRecord.ActivityRecords.Add(activityRecord);
                        }

                        // Update Activity Stats
                        activityRecord.Status = (int)ActivityRecordStatusEnum.Completed;
                        activityRecord.IsCompleted = true;
                        activityRecord.Score = 100;
                        activityRecord.CompletedDate = DateTime.Now;

                        // D. Handle Quiz Attempts (Type 2)
                        if (activity.ActivityType == (int)ActivityType.Quiz)
                        {
                            // Find linked Quiz
                            var activityQuiz = activity.ActivityQuizzes.FirstOrDefault();
                            int? quizId = activityQuiz?.QuizId;

                            // Check if an attempt exists (current one)
                            var quizAttempt = activityRecord.QuizAttempts.FirstOrDefault(qa => qa.IsCurrent);
                            if (quizAttempt == null)
                            {
                                quizAttempt = new QuizAttempt
                                {
                                    ActivityRecord = activityRecord,
                                    ActivityRecordId = activityRecord.Id,
                                    QuizId = quizId, // Can be null if configuration is missing
                                    Name = activity.ActivityTitle + " - Auto Attempt",
                                    QuizAttemptDate = DateTime.Now,
                                    IsCurrent = true,
                                    AttemptOrder = 1
                                };
                                if (activityRecord.Id != 0) _context.QuizAttempts.Add(quizAttempt);
                                else activityRecord.QuizAttempts.Add(quizAttempt);
                            }

                            // Update Attempt Stats
                            quizAttempt.Status = (int)QuizAttemptStatusEnum.Submitted;
                            quizAttempt.IsPass = true;
                            quizAttempt.AttemptScore = 10; // Default max assumption
                            quizAttempt.MaxScore = 10;
                        }

                        // E. Handle Practice Attempts (Type 3)
                        else if (activity.ActivityType == (int)ActivityType.Practice)
                        {
                            // Find linked Practice
                            var activityPractice = activity.ActivityPractices.FirstOrDefault();
                            int? practiceId = activityPractice?.PracticeId;

                            var practiceAttempt = activityRecord.PracticeAttempts.FirstOrDefault(pa => pa.IsCurrent);
                            if (practiceAttempt == null)
                            {
                                practiceAttempt = new PracticeAttempt
                                {
                                    ActivityRecord = activityRecord,
                                    ActivityRecordId = activityRecord.Id,
                                    PracticeId = practiceId,
                                    AttemptDate = DateTime.Now,
                                    IsCurrent = true,
                                    Description = "Auto-generated practice attempt"
                                };
                                if (activityRecord.Id != 0) _context.PracticeAttempts.Add(practiceAttempt);
                                else activityRecord.PracticeAttempts.Add(practiceAttempt);
                            }

                            // Update Attempt Stats
                            practiceAttempt.AttemptStatus = (int)ActivityRecordStatusEnum.Completed; // Re-using existing status enum logic
                            practiceAttempt.IsPass = true;
                            practiceAttempt.Score = 100;
                        }
                    }
                }
            }

            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Auto completes attendance for all enrollments in the specified class.
        /// Marks all timeslots as Completed and all students as Present.
        /// </summary>
        /// <param name="classId">The ID of the class.</param>
        public async Task AutoCompleteAttendance(int classId)
        {
            // 1. Fetch Timeslots and Enrollments
            var timeslots = await _context.Timeslots
                .Where(t => t.ClassId == classId && t.IsDeleted != true)
                .ToListAsync();

            if (!timeslots.Any()) return;

            var enrollments = await _context.Enrollments
                .Include(e => e.Attendances)
                .Where(e => e.ClassId == classId && e.IsDeleted != true)
                .ToListAsync();

            // 2. Iterate Timeslots
            foreach (var timeslot in timeslots)
            {
                // Update Timeslot Status
                timeslot.Status = (int)TimeslotStatusEnum.Completed;

                // 3. Iterate Enrollments for Attendance
                foreach (var enrollment in enrollments)
                {
                    var attendance = enrollment.Attendances.FirstOrDefault(a => a.TimeslotId == timeslot.Id);

                    if (attendance == null)
                    {
                        attendance = new Attendance
                        {
                            EnrollmentId = enrollment.Id,
                            TimeslotId = timeslot.Id,
                            Name = "Auto Attendance",
                            IsActive = true,
                            IsDeleted = false
                        };
                        _context.Attendances.Add(attendance);
                    }

                    // Mark as Present
                    attendance.Status = (int)AttendanceStatusEnum.Present;
                }
            }

            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Auto completes the final exam for all enrollments in the class.
        /// Marks FinalExam as Passed and approves all existing partials (SE, TE, PE).
        /// Sets Enrollment Status to Completed.
        /// </summary>
        public async Task AutoCompleteFinalExam(int classId)
        {
            var enrollments = await _context.Enrollments
                .Include(e => e.FinalExams)
                    .ThenInclude(fe => fe.FinalExamPartials)
                        .ThenInclude(fep => fep.PeChecklists)
                .Where(e => e.ClassId == classId && e.IsDeleted != true)
                .ToListAsync();

            foreach (var enrollment in enrollments)
            {
                // 1. Get or Create Final Exam
                var finalExam = enrollment.FinalExams.FirstOrDefault();
                if (finalExam == null)
                {
                    finalExam = new FinalExam
                    {
                        EnrollmentId = enrollment.Id,
                        ExamCode = "AUTO-" + Guid.NewGuid().ToString().Substring(0, 6).ToUpper(),
                        TotalMarks = 0 // Will be updated
                    };
                    _context.FinalExams.Add(finalExam);
                }

                // 2. Mark Final Exam as Passed
                finalExam.IsPass = true;
                finalExam.TotalMarks = 100; // Assuming 100 scale
                finalExam.CompleteTime = DateTime.Now;

                // 3. Handle Partials (SE, TE, PE)
                // We iterate existing partials. If the course logic created them, we approve them.
                foreach (var partial in finalExam.FinalExamPartials)
                {
                    partial.IsPass = true;
                    partial.Status = (int)FinalExamPartialStatus.Approved;
                    partial.Marks = 100; // Assuming 100 scale or max weight
                    partial.CompleteTime = DateTime.Now;

                    // Pass all checklist items for Practical Exams
                    foreach (var checklist in partial.PeChecklists)
                    {
                        checklist.IsPass = true;
                    }
                }

                // 4. Complete the Enrollment
                // This ensures the Certificate Service picks it up
                enrollment.Status = (int)EnrollmentStatusEnum.Completed;
            }

            await _context.SaveChangesAsync();
        }
    }
}