using Lssctc.ProgramManagement.Dashboard.Dtos;
using Lssctc.Share.Enums;
using Lssctc.Share.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Lssctc.ProgramManagement.Dashboard.Services
{
    public class AdminDashboardService : IAdminDashboardService
    {
        private readonly IUnitOfWork _uow;

        public AdminDashboardService(IUnitOfWork uow)
        {
            _uow = uow;
        }

        #region Part 1: System Overview

        public async Task<SystemSummaryDto> GetSystemSummaryAsync()
        {
            // Total Active Programs (Not Deleted, Active)
            var totalPrograms = await _uow.ProgramRepository
                .GetAllAsQueryable()
                .AsNoTracking()
                .Where(p => p.IsActive == true && p.IsDeleted == false)
                .CountAsync();

            // Total Active Courses (Not Deleted, Active)
            var totalCourses = await _uow.CourseRepository
                .GetAllAsQueryable()
                .AsNoTracking()
                .Where(c => c.IsActive == true && c.IsDeleted == false)
                .CountAsync();

            // Total Trainees (Not Deleted)
            var totalTrainees = await _uow.TraineeRepository
                .GetAllAsQueryable()
                .AsNoTracking()
                .Where(t => t.IsDeleted == false)
                .CountAsync();

            // Total Instructors (Not Deleted)
            var totalInstructors = await _uow.InstructorRepository
                .GetAllAsQueryable()
                .AsNoTracking()
                .Where(i => i.IsDeleted == false)
                .CountAsync();

            // Total Classes (All statuses)
            var totalClasses = await _uow.ClassRepository
                .GetAllAsQueryable()
                .AsNoTracking()
                .CountAsync();

            // Total Practices (Not Deleted)
            var totalPractices = await _uow.PracticeRepository
                .GetAllAsQueryable()
                .AsNoTracking()
                .Where(p => p.IsDeleted == false)
                .CountAsync();

            return new SystemSummaryDto
            {
                TotalPrograms = totalPrograms,
                TotalCourses = totalCourses,
                TotalTrainees = totalTrainees,
                TotalInstructors = totalInstructors,
                TotalClasses = totalClasses,
                TotalPractices = totalPractices
            };
        }

        #endregion

        #region Part 2: Advanced Statistics

        public async Task<IEnumerable<PopularCourseDto>> GetTopPopularCoursesAsync(int topCount = 5)
        {
            // Path: Enrollment -> Class -> ProgramCourse -> Course
            // Count enrollments per course
            var popularCourses = await _uow.EnrollmentRepository
                .GetAllAsQueryable()
                .AsNoTracking()
                .Include(e => e.Class)
                    .ThenInclude(c => c.ProgramCourse)
                        .ThenInclude(pc => pc.Course)
                .Where(e => e.Class.ProgramCourse.Course != null)
                .GroupBy(e => new
                {
                    CourseId = e.Class.ProgramCourse.CourseId,
                    CourseName = e.Class.ProgramCourse.Course.Name
                })
                .Select(g => new PopularCourseDto
                {
                    CourseName = g.Key.CourseName,
                    TotalEnrollments = g.Count()
                })
                .OrderByDescending(x => x.TotalEnrollments)
                .Take(topCount)
                .ToListAsync();

            return popularCourses;
        }

        public async Task<IEnumerable<ActiveTraineeDto>> GetTopActiveTraineesAsync(int topCount = 5)
        {
            // Count distinct courses per trainee
            // We need to get trainees with the most enrollments
            var activeTrainees = await _uow.EnrollmentRepository
                .GetAllAsQueryable()
                .AsNoTracking()
                .Include(e => e.Trainee)
                    .ThenInclude(t => t.IdNavigation)
                .Include(e => e.Class)
                    .ThenInclude(c => c.ProgramCourse)
                .Where(e => e.Trainee != null && e.Trainee.IsDeleted == false)
                .GroupBy(e => new
                {
                    TraineeId = e.TraineeId,
                    TraineeName = e.Trainee.IdNavigation.Fullname ?? e.Trainee.IdNavigation.Username,
                    TraineeCode = e.Trainee.TraineeCode
                })
                .Select(g => new
                {
                    g.Key.TraineeName,
                    g.Key.TraineeCode,
                    // Count distinct courses
                    EnrolledCourseCount = g.Select(e => e.Class.ProgramCourse.CourseId).Distinct().Count()
                })
                .OrderByDescending(x => x.EnrolledCourseCount)
                .Take(topCount)
                .ToListAsync();

            return activeTrainees.Select(x => new ActiveTraineeDto
            {
                TraineeName = x.TraineeName,
                TraineeCode = x.TraineeCode,
                EnrolledCourseCount = x.EnrolledCourseCount
            });
        }

        public async Task<IEnumerable<ClassStatusDistributionDto>> GetClassStatusDistributionAsync()
        {
            // Group classes by status
            var classDistribution = await _uow.ClassRepository
                .GetAllAsQueryable()
                .AsNoTracking()
                .GroupBy(c => c.Status)
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

        public async Task<IEnumerable<CourseCompletionTrendDto>> GetDailyCourseCompletionTrendsAsync(int month, int year)
        {
            // Default to current month and year if not specified or invalid
            if (month <= 0 || month > 12)
                month = DateTime.UtcNow.Month;
            
            if (year <= 0)
                year = DateTime.UtcNow.Year;

            // Validate year is reasonable
            if (year < 2000 || year > DateTime.UtcNow.Year + 1)
                year = DateTime.UtcNow.Year;

            // Calculate number of days in the specified month
            var daysInMonth = DateTime.DaysInMonth(year, month);

            // Get all certificates issued in the specified month and year
            var certificates = await _uow.TraineeCertificateRepository
                .GetAllAsQueryable()
                .AsNoTracking()
                .Where(tc => tc.IssuedDate.HasValue &&
                            tc.IssuedDate.Value.Year == year &&
                            tc.IssuedDate.Value.Month == month)
                .Select(tc => new
                {
                    Day = tc.IssuedDate!.Value.Day
                })
                .ToListAsync();

            // Group by day and count
            var dailyCompletions = certificates
                .GroupBy(c => c.Day)
                .Select(g => new
                {
                    Day = g.Key,
                    Count = g.Count()
                })
                .ToDictionary(x => x.Day, x => x.Count);

            // Create result for all days in the month (fill with 0 if no data)
            var result = Enumerable.Range(1, daysInMonth)
                .Select(day => new CourseCompletionTrendDto
                {
                    Day = day,
                    CompletedCount = dailyCompletions.ContainsKey(day) ? dailyCompletions[day] : 0
                })
                .ToList();

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
