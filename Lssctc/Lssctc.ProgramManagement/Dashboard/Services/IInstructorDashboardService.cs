using Lssctc.ProgramManagement.Dashboard.Dtos;

namespace Lssctc.ProgramManagement.Dashboard.Services
{
    public interface IInstructorDashboardService
    {
        // Part 1: Overview (Summary Cards)
        Task<InstructorSummaryDto> GetInstructorSummaryAsync(int instructorId);

        // Part 2: Charts & Analytics
        Task<IEnumerable<ClassTraineeCountDto>> GetTopClassesByTraineeCountAsync(int instructorId, int topCount = 5);
        Task<IEnumerable<ClassStatusDistributionDto>> GetInstructorClassStatusDistributionAsync(int instructorId);
        Task<IEnumerable<InstructorCourseCompletionTrendDto>> GetInstructorYearlyCompletionTrendsAsync(int instructorId, int year);
        Task<IEnumerable<ClassGradeDistributionDto>> GetClassGradeDistributionAsync(int instructorId);
    }
}
