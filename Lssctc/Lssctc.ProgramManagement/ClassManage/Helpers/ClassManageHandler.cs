using Lssctc.Share.Entities;
using Lssctc.Share.Enums;
using Lssctc.Share.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Lssctc.ProgramManagement.ClassManage.Helpers
{
    public class ClassManageHandler
    {
        private readonly IUnitOfWork _uow;

        public ClassManageHandler(IUnitOfWork uow)
        {
            _uow = uow;
        }

        public async Task<string> EnsureProgressScaffoldingForTraineeAsync(int classId, int traineeId)
        {
            // 1. Get the class and the full course template
            var (targetClass, courseTemplate, courseId) = await GetClassAndCourseTemplateAsync(classId);
            if (targetClass == null || targetClass.Id == 0)
                return "Error: Class not found.";

            if (courseId == 0)
                return "Error: Course not found for class.";

            // 2. Check Class Status
            if (targetClass.Status != (int)ClassStatusEnum.Open && targetClass.Status != (int)ClassStatusEnum.Inprogress)
                return $"Error: Class status is '{targetClass.Status}', must be Open or Inprogress.";

            // 3. Get the specific enrollment
            var enrollment = await _uow.EnrollmentRepository
                .GetAllAsQueryable()
                .FirstOrDefaultAsync(e => e.ClassId == classId && e.TraineeId == traineeId);

            if (enrollment == null)
                return "Error: Enrollment not found for this trainee in this class.";

            // 4. Check Enrollment Status
            if (enrollment.Status != (int)EnrollmentStatusEnum.Enrolled && enrollment.Status != (int)EnrollmentStatusEnum.Inprogress)
                return $"Error: Enrollment status is '{enrollment.Status}', must be Enrolled or Inprogress.";

            // 5. Check if progress already exists
            var existingProgress = await _uow.LearningProgressRepository
                .GetAllAsQueryable()
                .AnyAsync(lp => lp.EnrollmentId == enrollment.Id);

            if (existingProgress)
                return "Trainee already has learning progress.";

            // 6. Create the full scaffolding
            var newProgress = CreateProgressScaffolding(enrollment, courseTemplate, courseId);

            // 7. Save the new progress tree
            await _uow.LearningProgressRepository.CreateAsync(newProgress);
            await _uow.SaveChangesAsync();

            return $"Successfully created learning progress for trainee {traineeId}.";
        }

        public async Task<List<string>> EnsureProgressScaffoldingForClassAsync(int classId)
        {
            var results = new List<string>();

            // 1. Get the class and the full course template
            var (targetClass, courseTemplate, courseId) = await GetClassAndCourseTemplateAsync(classId);
            if (targetClass == null || targetClass.Id == 0)
            {
                results.Add("Error: Class not found.");
                return results;
            }

            if (courseId == 0)
            {
                results.Add("Error: Course not found for class.");
                return results;
            }

            // 2. Check Class Status
            if (targetClass.Status != (int)ClassStatusEnum.Open && targetClass.Status != (int)ClassStatusEnum.Inprogress)
            {
                results.Add($"Error: Class status is '{targetClass.Status}', must be Open or Inprogress.");
                return results;
            }

            // 3. Get all valid enrollments for this class
            var validEnrollmentStatuses = new[] { (int)EnrollmentStatusEnum.Enrolled, (int)EnrollmentStatusEnum.Inprogress };
            var enrollments = await _uow.EnrollmentRepository
                .GetAllAsQueryable()
                .Where(e => e.ClassId == classId && validEnrollmentStatuses.Contains(e.Status ?? -1))
                .ToListAsync();

            if (!enrollments.Any())
            {
                results.Add("No valid enrollments found to process.");
                return results;
            }

            // 4. Get all enrollments that *already* have progress
            var enrollmentIds = enrollments.Select(e => e.Id).ToList();
            var existingProgressEnrollmentIdsList = await _uow.LearningProgressRepository
                .GetAllAsQueryable()
                .Where(lp => enrollmentIds.Contains(lp.EnrollmentId))
                .Select(lp => lp.EnrollmentId)
                .ToListAsync();

            var existingProgressEnrollmentIds = new HashSet<int>(existingProgressEnrollmentIdsList); // Convert to HashSet

            // 5. Filter to find only enrollments that NEED progress
            var enrollmentsToCreate = enrollments
                .Where(e => !existingProgressEnrollmentIds.Contains(e.Id))
                .ToList();

            if (!enrollmentsToCreate.Any())
            {
                results.Add("All valid trainees already have learning progress.");
                return results;
            }

            // 6. Create scaffolding for all new enrollments in memory
            int createdCount = 0;
            foreach (var enrollment in enrollmentsToCreate)
            {
                var newProgress = CreateProgressScaffolding(enrollment, courseTemplate, courseId);
                await _uow.LearningProgressRepository.CreateAsync(newProgress);
                results.Add($"Trainee {enrollment.TraineeId}: Progress queued for creation.");
                createdCount++;
            }

            // 7. Save all new progress trees in one transaction
            if (createdCount > 0)
            {
                await _uow.SaveChangesAsync();
                results.Add($"Successfully created {createdCount} new learning progress records.");
            }

            return results;
        }

        private async Task<(Class, List<Section>, int CourseId)> GetClassAndCourseTemplateAsync(int classId)
        {
            var targetClass = await _uow.ClassRepository
                .GetAllAsQueryable()
                .Include(c => c.ProgramCourse)
                    .ThenInclude(pc => pc.Course)
                        .ThenInclude(co => co.CourseSections)
                            .ThenInclude(cs => cs.Section)
                                .ThenInclude(s => s.SectionActivities)
                                    .ThenInclude(sa => sa.Activity)
                .FirstOrDefaultAsync(c => c.Id == classId);

            if (targetClass == null || targetClass.ProgramCourse?.Course == null)
                return (new Class(), new List<Section>(), 0); // Return 0 for CourseId

            // Get the ordered list of sections
            var courseTemplate = targetClass.ProgramCourse.Course.CourseSections
                .OrderBy(cs => cs.SectionOrder)
                .Select(cs => cs.Section)
                .ToList();

            return (targetClass, courseTemplate, targetClass.ProgramCourse.Course.Id);
        }

        private LearningProgress CreateProgressScaffolding(Enrollment enrollment, List<Section> courseTemplate, int courseId)
        {
            // 1. Create the root LearningProgress
            var newProgress = new LearningProgress
            {
                EnrollmentId = enrollment.Id,
                CourseId = courseId, 
                Status = (int)LearningProgressStatusEnum.NotStarted,
                ProgressPercentage = 0,
                StartDate = DateTime.UtcNow,
                LastUpdated = DateTime.UtcNow,
                SectionRecords = new List<SectionRecord>()
            };

            // 2. Loop through the course template to create SectionRecords
            foreach (var section in courseTemplate)
            {
                var newSectionRecord = new SectionRecord
                {
                    LearningProgress = newProgress,
                    SectionId = section.Id,
                    SectionName = section.SectionTitle,
                    DurationMinutes = section.EstimatedDurationMinutes,
                    IsCompleted = false,
                    IsTraineeAttended = false,
                    Progress = 0,
                    ActivityRecords = new List<ActivityRecord>()
                };

                // 3. Loop through the section's activities to create ActivityRecords
                var orderedActivities = section.SectionActivities
                    .OrderBy(sa => sa.ActivityOrder)
                    .Select(sa => sa.Activity);

                foreach (var activity in orderedActivities)
                {
                    var newActivityRecord = new ActivityRecord
                    {
                        SectionRecord = newSectionRecord,
                        ActivityId = activity.Id,
                        Status = (int)ActivityRecordStatusEnum.NotStarted,
                        Score = null,
                        IsCompleted = false,
                        CompletedDate = null,
                        ActivityType = activity.ActivityType
                    };
                    newSectionRecord.ActivityRecords.Add(newActivityRecord);
                }

                newProgress.SectionRecords.Add(newSectionRecord);
            }

            return newProgress;
        }
    }
}