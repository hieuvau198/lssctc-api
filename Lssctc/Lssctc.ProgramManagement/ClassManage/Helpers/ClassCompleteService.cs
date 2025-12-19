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
    public class ClassCompleteService : IClassCompleteService
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
            var classInfo = await GetClassWithCourseStructureAsync(classId);
            if (classInfo == null || classInfo.ProgramCourse?.Course == null) return;

            var course = classInfo.ProgramCourse.Course;

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
                ProcessEnrollmentProgress(enrollment, course, classInfo.StartDate);
            }

            await _context.SaveChangesAsync();
        }

        public async Task AutoCompleteLearningProgressForEnrollment(int enrollmentId)
        {
            // 1. Fetch Enrollment to get ClassId
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

            // 2. Fetch Class and Course Structure
            var classInfo = await GetClassWithCourseStructureAsync(enrollment.ClassId);
            if (classInfo == null || classInfo.ProgramCourse?.Course == null) return;

            var course = classInfo.ProgramCourse.Course;

            // 3. Process Single Enrollment
            ProcessEnrollmentProgress(enrollment, course, classInfo.StartDate);

            await _context.SaveChangesAsync();
        }

        private async Task<Class?> GetClassWithCourseStructureAsync(int classId)
        {
            return await _context.Classes
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
        }

        private void ProcessEnrollmentProgress(Enrollment enrollment, Course course, DateTime? startDate)
        {
            // A. Get or Create LearningProgress
            var progress = enrollment.LearningProgresses.FirstOrDefault(lp => lp.CourseId == course.Id);
            if (progress == null)
            {
                progress = new LearningProgress
                {
                    EnrollmentId = enrollment.Id,
                    CourseId = course.Id,
                    StartDate = startDate ?? DateTime.UtcNow,
                    Name = $"Progress for {course.Name}",
                    Description = "Auto-generated progress"
                };
                _context.LearningProgresses.Add(progress);
            }

            // Update Progress Stats
            progress.Status = (int)LearningProgressStatusEnum.Completed;
            progress.ProgressPercentage = 10;
            progress.TheoryScore = 10;
            progress.PracticalScore = 10;
            progress.FinalScore = 10;
            progress.LastUpdated = DateTime.Now;

            // B. Iterate through Course Sections
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
                        LearningProgress = progress,
                        LearningProgressId = progress.Id,
                        SectionId = section.Id,
                        SectionName = section.SectionTitle,
                        DurationMinutes = section.EstimatedDurationMinutes
                    };
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
                    activityRecord.Score = 10;
                    activityRecord.CompletedDate = DateTime.Now;

                    // D. Handle Quiz Attempts (Type 2)
                    if (activity.ActivityType == (int)ActivityType.Quiz)
                    {
                        var activityQuiz = activity.ActivityQuizzes.FirstOrDefault();
                        int? quizId = activityQuiz?.QuizId;

                        var quizAttempt = activityRecord.QuizAttempts.FirstOrDefault(qa => qa.IsCurrent);
                        if (quizAttempt == null)
                        {
                            quizAttempt = new QuizAttempt
                            {
                                ActivityRecord = activityRecord,
                                ActivityRecordId = activityRecord.Id,
                                QuizId = quizId,
                                Name = activity.ActivityTitle + " - Auto Attempt",
                                QuizAttemptDate = DateTime.Now,
                                IsCurrent = true,
                                AttemptOrder = 1
                            };
                            if (activityRecord.Id != 0) _context.QuizAttempts.Add(quizAttempt);
                            else activityRecord.QuizAttempts.Add(quizAttempt);
                        }

                        quizAttempt.Status = (int)QuizAttemptStatusEnum.Submitted;
                        quizAttempt.IsPass = true;
                        quizAttempt.AttemptScore = 10;
                        quizAttempt.MaxScore = 10;
                    }

                    // E. Handle Practice Attempts (Type 3)
                    else if (activity.ActivityType == (int)ActivityType.Practice)
                    {
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

                        practiceAttempt.AttemptStatus = (int)ActivityRecordStatusEnum.Completed;
                        practiceAttempt.IsPass = true;
                        practiceAttempt.Score = 10;
                    }
                }
            }
        }

        /// <summary>
        /// Auto completes attendance for all enrollments in the specified class.
        /// Marks all timeslots as Completed and all students as Present.
        /// </summary>
        /// <param name="classId">The ID of the class.</param>
        public async Task AutoCompleteAttendance(int classId)
        {
            // 1. Fetch Timeslots
            var timeslots = await _context.Timeslots
                .Where(t => t.ClassId == classId && t.IsDeleted != true)
                .ToListAsync();

            if (!timeslots.Any()) return;

            // Update Timeslot Status first
            foreach (var ts in timeslots)
            {
                ts.Status = (int)TimeslotStatusEnum.Completed;
            }

            var enrollments = await _context.Enrollments
                .Include(e => e.Attendances)
                .Where(e => e.ClassId == classId && e.IsDeleted != true)
                .ToListAsync();

            // 2. Process Enrollments
            foreach (var enrollment in enrollments)
            {
                ProcessEnrollmentAttendance(enrollment, timeslots);
            }

            await _context.SaveChangesAsync();
        }

        public async Task AutoCompleteAttendanceForEnrollment(int enrollmentId)
        {
            // 1. Fetch Enrollment
            var enrollment = await _context.Enrollments
                .Include(e => e.Attendances)
                .FirstOrDefaultAsync(e => e.Id == enrollmentId);

            if (enrollment == null) throw new Exception("Enrollment not found");

            // 2. Fetch Timeslots for that class
            var timeslots = await _context.Timeslots
                .Where(t => t.ClassId == enrollment.ClassId && t.IsDeleted != true)
                .ToListAsync();

            if (!timeslots.Any()) return;

            // 3. Process Single Enrollment
            ProcessEnrollmentAttendance(enrollment, timeslots);

            await _context.SaveChangesAsync();
        }

        private void ProcessEnrollmentAttendance(Enrollment enrollment, System.Collections.Generic.List<Timeslot> timeslots)
        {
            foreach (var timeslot in timeslots)
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
                .Include(e => e.FinalExams) // Include Simulation Structure
                    .ThenInclude(fe => fe.FinalExamPartials)
                        .ThenInclude(fep => fep.FeSimulations)
                            .ThenInclude(s => s.SeTasks)
                .Where(e => e.ClassId == classId && e.IsDeleted != true)
                .ToListAsync();

            foreach (var enrollment in enrollments)
            {
                await ProcessEnrollmentFinalExam(enrollment);
            }

            await _context.SaveChangesAsync();
        }

        public async Task AutoCompleteFinalExamForEnrollment(int enrollmentId)
        {
            var enrollment = await _context.Enrollments
                .Include(e => e.FinalExams)
                    .ThenInclude(fe => fe.FinalExamPartials)
                        .ThenInclude(fep => fep.PeChecklists)
                .Include(e => e.FinalExams) // Include Simulation Structure
                    .ThenInclude(fe => fe.FinalExamPartials)
                        .ThenInclude(fep => fep.FeSimulations)
                            .ThenInclude(s => s.SeTasks)
                .FirstOrDefaultAsync(e => e.Id == enrollmentId);

            if (enrollment == null) throw new Exception("Enrollment not found");

            await ProcessEnrollmentFinalExam(enrollment);

            await _context.SaveChangesAsync();
        }

        private async Task ProcessEnrollmentFinalExam(Enrollment enrollment)
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
            finalExam.TotalMarks = 10;
            finalExam.CompleteTime = DateTime.Now;
            finalExam.Status = (int)FinalExamStatusEnum.Completed;

            // 3. Handle Partials (SE, TE, PE)
            foreach (var partial in finalExam.FinalExamPartials)
            {
                partial.IsPass = true;
                partial.Status = (int)FinalExamPartialStatus.Approved;
                partial.Marks = 10;
                partial.CompleteTime = DateTime.Now;

                // [NEW] Handle Simulation Exams (SE) - Create & Pass SeTasks
                if (partial.Type == (int)FinalExamPartialType.Simulation)
                {
                    var feSim = partial.FeSimulations.FirstOrDefault();
                    if (feSim != null)
                    {
                        // Fetch all practice tasks defined for this simulation's practice
                        var practiceTasks = await _context.PracticeTasks
                            .Include(pt => pt.Task)
                            .Where(pt => pt.PracticeId == feSim.PracticeId)
                            .ToListAsync();

                        foreach (var pt in practiceTasks)
                        {
                            // Check if student already has a record for this task
                            var seTask = feSim.SeTasks.FirstOrDefault(t => t.SimTaskId == pt.TaskId);
                            if (seTask == null)
                            {
                                seTask = new SeTask
                                {
                                    FeSimulationId = feSim.Id,
                                    SimTaskId = pt.TaskId,
                                    Name = pt.Task?.TaskName ?? "Auto Task",
                                    Description = pt.Task?.TaskDescription
                                };
                                _context.SeTasks.Add(seTask);
                            }

                            // Mark task as passed
                            seTask.IsPass = true;
                            seTask.Status = 1; // 1 = Completed
                            seTask.CompleteTime = DateTime.Now;
                            seTask.DurationSecond = 60; // Mock duration
                        }
                    }
                }

                // Pass all checklist items for Practical Exams
                foreach (var checklist in partial.PeChecklists)
                {
                    checklist.IsPass = true;
                }
            }

            // 4. Complete the Enrollment
            enrollment.Status = (int)EnrollmentStatusEnum.Completed;
        }
    }
}