using Lssctc.ProgramManagement.Dashboard.Dtos;

namespace Lssctc.ProgramManagement.Dashboard.Services
{
    public interface IAdminDashboardService
    {
        // Part 1: System Overview
        Task<SystemSummaryDto> GetSystemSummaryAsync();

        // Part 2: Advanced Statistics
        Task<IEnumerable<PopularCourseDto>> GetTopPopularCoursesAsync(int topCount = 5);
        Task<IEnumerable<ActiveTraineeDto>> GetTopActiveTraineesAsync(int topCount = 5);
        Task<IEnumerable<ClassStatusDistributionDto>> GetClassStatusDistributionAsync();
        Task<IEnumerable<CourseCompletionTrendDto>> GetDailyCourseCompletionTrendsAsync(int month, int year);
    }
}
