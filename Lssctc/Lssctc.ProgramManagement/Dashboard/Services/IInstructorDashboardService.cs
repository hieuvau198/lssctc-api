using Lssctc.ProgramManagement.Dashboard.Dtos;

namespace Lssctc.ProgramManagement.Dashboard.Services
{
    public interface IInstructorDashboardService
    {
        // Part 1: Overview (Summary Cards)
        Task<InstructorSummaryDto> GetInstructorSummaryAsync(int userId);

        // Part 2: Charts & Analytics
        Task<IEnumerable<ClassTraineeCountDto>> GetTopClassesByTraineeCountAsync(int userId, int topCount = 5);
        Task<IEnumerable<ClassStatusDistributionDto>> GetInstructorClassStatusDistributionAsync(int userId);
        Task<IEnumerable<InstructorCourseCompletionTrendDto>> GetInstructorYearlyCompletionTrendsAsync(int userId, int year);
        Task<IEnumerable<ClassGradeDistributionDto>> GetClassGradeDistributionAsync(int userId);
    }
}
