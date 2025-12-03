using Lssctc.ProgramManagement.Dashboard.Dtos;
using Lssctc.Share.Enums;
using Lssctc.Share.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Globalization;

namespace Lssctc.ProgramManagement.Dashboard.Services
{
    public class InstructorDashboardService : IInstructorDashboardService
    {
        private readonly IUnitOfWork _uow;

        public InstructorDashboardService(IUnitOfWork uow)
        {
            _uow = uow;
        }

        #region Part 1: Overview (Summary Cards)

        public async Task<InstructorSummaryDto> GetInstructorSummaryAsync(int instructorId)
        {
            // Validate instructor exists
            var instructorExists = await _uow.InstructorRepository
                .GetAllAsQueryable()
                .AsNoTracking()
                .AnyAsync(i => i.Id == instructorId && i.IsDeleted == false);

            if (!instructorExists)
                throw new KeyNotFoundException($"Instructor with ID {instructorId} not found or has been deleted.");

            // Total Assigned Trainees: Count unique Trainees enrolled in classes assigned to this instructor
            // Path: ClassInstructor -> Class -> Enrollment -> Count distinct TraineeId
            var totalAssignedTrainees = await _uow.ClassInstructorRepository
                .GetAllAsQueryable()
                .AsNoTracking()
                .Where(ci => ci.InstructorId == instructorId)
                .Include(ci => ci.Class)
                    .ThenInclude(c => c.Enrollments)
                .Where(ci => ci.Class != null)
                .SelectMany(ci => ci.Class.Enrollments)
                .Where(e => e.IsDeleted == false)
                .Select(e => e.TraineeId)
                .Distinct()
                .CountAsync();

            // Total Assigned Classes: Count classes where this instructor is assigned
            var totalAssignedClasses = await _uow.ClassInstructorRepository
                .GetAllAsQueryable()
                .AsNoTracking()
                .Include(ci => ci.Class)
                .Where(ci => ci.InstructorId == instructorId && 
                            ci.Class != null)
                .Select(ci => ci.ClassId)
                .Distinct()
                .CountAsync();

            // Total Materials Created: Count learning materials authored by this instructor
            var totalMaterialsCreated = await _uow.MaterialAuthorRepository
                .GetAllAsQueryable()
                .AsNoTracking()
                .Where(ma => ma.InstructorId == instructorId)
                .CountAsync();

            // Total Quizzes Created: Count quizzes authored by this instructor
            var totalQuizzesCreated = await _uow.QuizAuthorRepository
                .GetAllAsQueryable()
                .AsNoTracking()
                .Where(qa => qa.InstructorId == instructorId)
                .CountAsync();

            return new InstructorSummaryDto
            {
                TotalAssignedTrainees = totalAssignedTrainees,
                TotalAssignedClasses = totalAssignedClasses,
                TotalMaterialsCreated = totalMaterialsCreated,
                TotalQuizzesCreated = totalQuizzesCreated
            };
        }

        #endregion

        #region Part 2: Charts & Analytics

        public async Task<IEnumerable<ClassTraineeCountDto>> GetTopClassesByTraineeCountAsync(int instructorId, int topCount = 5)
        {
            // Validate instructor exists
            var instructorExists = await _uow.InstructorRepository
                .GetAllAsQueryable()
                .AsNoTracking()
                .AnyAsync(i => i.Id == instructorId && i.IsDeleted == false);

            if (!instructorExists)
                throw new KeyNotFoundException($"Instructor with ID {instructorId} not found or has been deleted.");

            // Get classes assigned to the instructor, order by number of Enrollments (descending), take top N
            var topClasses = await _uow.ClassInstructorRepository
                .GetAllAsQueryable()
                .AsNoTracking()
                .Where(ci => ci.InstructorId == instructorId)
                .Include(ci => ci.Class)
                    .ThenInclude(c => c.ClassCode)
                .Include(ci => ci.Class)
                    .ThenInclude(c => c.Enrollments)
                .Where(ci => ci.Class != null)
                .Select(ci => new
                {
                    ClassId = ci.ClassId,
                    ClassName = ci.Class.Name,
                    ClassCode = ci.Class.ClassCode != null ? ci.Class.ClassCode.Name : "N/A",
                    // Count enrollments that are not deleted
                    TraineeCount = ci.Class.Enrollments.Count(e => e.IsDeleted == false)
                })
                .OrderByDescending(x => x.TraineeCount)
                .Take(topCount)
                .ToListAsync();

            return topClasses.Select(x => new ClassTraineeCountDto
            {
                ClassName = x.ClassName,
                ClassCode = x.ClassCode,
                TraineeCount = x.TraineeCount
            });
        }

        public async Task<IEnumerable<ClassStatusDistributionDto>> GetInstructorClassStatusDistributionAsync(int instructorId)
        {
            // Validate instructor exists
            var instructorExists = await _uow.InstructorRepository
                .GetAllAsQueryable()
                .AsNoTracking()
                .AnyAsync(i => i.Id == instructorId && i.IsDeleted == false);

            if (!instructorExists)
                throw new KeyNotFoundException($"Instructor with ID {instructorId} not found or has been deleted.");

            // Get class status distribution for classes assigned to this instructor
            var classDistribution = await _uow.ClassInstructorRepository
                .GetAllAsQueryable()
                .AsNoTracking()
                .Where(ci => ci.InstructorId == instructorId)
                .Include(ci => ci.Class)
                .Where(ci => ci.Class != null)
                .GroupBy(ci => ci.Class.Status)
                .Select(g => new
                {
                    Status = g.Key,
                    Count = g.Count()
                })
                .ToListAsync();

            // Map status enum to name
            var result = classDistribution.Select(x => new ClassStatusDistributionDto
            {
                StatusName = GetClassStatusName(x.Status),
                Count = x.Count
            }).ToList();

            // Ensure all statuses are represented (even if count is 0)
            var allStatuses = Enum.GetValues(typeof(ClassStatusEnum))
                .Cast<ClassStatusEnum>()
                .Select(status => new ClassStatusDistributionDto
                {
                    StatusName = status.ToString(),
                    Count = result.FirstOrDefault(r => r.StatusName == status.ToString())?.Count ?? 0
                })
                .ToList();

            return allStatuses;
        }

        public async Task<IEnumerable<InstructorCourseCompletionTrendDto>> GetInstructorYearlyCompletionTrendsAsync(int instructorId, int year)
        {
            // Validate instructor exists
            var instructorExists = await _uow.InstructorRepository
                .GetAllAsQueryable()
                .AsNoTracking()
                .AnyAsync(i => i.Id == instructorId && i.IsDeleted == false);

            if (!instructorExists)
                throw new KeyNotFoundException($"Instructor with ID {instructorId} not found or has been deleted.");

            // Default to current year if not specified
            if (year <= 0)
                year = DateTime.UtcNow.Year;

            // Get all class IDs assigned to this instructor
            var instructorClassIds = await _uow.ClassInstructorRepository
                .GetAllAsQueryable()
                .AsNoTracking()
                .Where(ci => ci.InstructorId == instructorId)
                .Select(ci => ci.ClassId)
                .Distinct()
                .ToListAsync();

            if (!instructorClassIds.Any())
            {
                // Return empty result for all 12 months
                return Enumerable.Range(1, 12)
                    .Select(month => new InstructorCourseCompletionTrendDto
                    {
                        Month = month,
                        MonthName = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(month),
                        CompletedCount = 0
                    })
                    .ToList();
            }

            // Get certificates issued in the specified year for enrollments in instructor's classes
            var certificates = await _uow.TraineeCertificateRepository
                .GetAllAsQueryable()
                .AsNoTracking()
                .Include(tc => tc.Enrollment)
                .Where(tc => tc.IssuedDate.HasValue &&
                            tc.IssuedDate.Value.Year == year &&
                            tc.Enrollment != null &&
                            instructorClassIds.Contains(tc.Enrollment.ClassId))
                .Select(tc => new
                {
                    Month = tc.IssuedDate!.Value.Month
                })
                .ToListAsync();

            // Group by month and count
            var monthlyCompletions = certificates
                .GroupBy(c => c.Month)
                .Select(g => new
                {
                    Month = g.Key,
                    Count = g.Count()
                })
                .ToDictionary(x => x.Month, x => x.Count);

            // Create result for all 12 months
            var result = Enumerable.Range(1, 12)
                .Select(month => new InstructorCourseCompletionTrendDto
                {
                    Month = month,
                    MonthName = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(month),
                    CompletedCount = monthlyCompletions.ContainsKey(month) ? monthlyCompletions[month] : 0
                })
                .ToList();

            return result;
        }

        public async Task<IEnumerable<ClassGradeDistributionDto>> GetClassGradeDistributionAsync(int instructorId)
        {
            // Validate instructor exists
            var instructorExists = await _uow.InstructorRepository
                .GetAllAsQueryable()
                .AsNoTracking()
                .AnyAsync(i => i.Id == instructorId && i.IsDeleted == false);

            if (!instructorExists)
                throw new KeyNotFoundException($"Instructor with ID {instructorId} not found or has been deleted.");

            // For each class assigned to the instructor, calculate the average FinalScore
            var classesWithScores = await _uow.ClassInstructorRepository
                .GetAllAsQueryable()
                .AsNoTracking()
                .Where(ci => ci.InstructorId == instructorId)
                .Include(ci => ci.Class)
                    .ThenInclude(c => c.Enrollments)
                        .ThenInclude(e => e.LearningProgresses)
                .Where(ci => ci.Class != null)
                .Select(ci => new
                {
                    ClassId = ci.ClassId,
                    ClassName = ci.Class.Name,
                    Enrollments = ci.Class.Enrollments
                        .Where(e => e.IsDeleted == false)
                        .ToList()
                })
                .ToListAsync();

            // Calculate average scores for each class and categorize into ranges
            var classAverages = classesWithScores
                .Select(c =>
                {
                    // Get all final scores from learning progresses
                    var scores = c.Enrollments
                        .SelectMany(e => e.LearningProgresses)
                        .Where(lp => lp.FinalScore.HasValue && lp.FinalScore.Value > 0)
                        .Select(lp => lp.FinalScore!.Value)
                        .ToList();

                    // Calculate average if there are any scores
                    return scores.Any() ? (decimal?)scores.Average() : null;
                })
                .Where(avg => avg.HasValue)
                .Select(avg => avg!.Value)
                .ToList();

            // Group averages into ranges
            var excellentCount = classAverages.Count(avg => avg >= 8.0m);
            var goodCount = classAverages.Count(avg => avg >= 6.5m && avg < 8.0m);
            var averageCount = classAverages.Count(avg => avg >= 5.0m && avg < 6.5m);
            var poorCount = classAverages.Count(avg => avg < 5.0m);

            var result = new List<ClassGradeDistributionDto>
            {
                new ClassGradeDistributionDto { RangeName = "Excellent (>= 8.0)", ClassCount = excellentCount },
                new ClassGradeDistributionDto { RangeName = "Good (6.5 - < 8.0)", ClassCount = goodCount },
                new ClassGradeDistributionDto { RangeName = "Average (5.0 - < 6.5)", ClassCount = averageCount },
                new ClassGradeDistributionDto { RangeName = "Poor (< 5.0)", ClassCount = poorCount }
            };

            return result;
        }

        #endregion

        #region Helper Methods

        private string GetClassStatusName(int? status)
        {
            if (!status.HasValue)
                return "Unknown";

            return status.Value switch
            {
                (int)ClassStatusEnum.Draft => ClassStatusEnum.Draft.ToString(),
                (int)ClassStatusEnum.Open => ClassStatusEnum.Open.ToString(),
                (int)ClassStatusEnum.Inprogress => ClassStatusEnum.Inprogress.ToString(),
                (int)ClassStatusEnum.Completed => ClassStatusEnum.Completed.ToString(),
                (int)ClassStatusEnum.Cancelled => ClassStatusEnum.Cancelled.ToString(),
                _ => "Unknown"
            };
        }

        #endregion
    }
}
