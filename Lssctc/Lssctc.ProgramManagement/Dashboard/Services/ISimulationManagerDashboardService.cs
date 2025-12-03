using Lssctc.ProgramManagement.Dashboard.Dtos;

namespace Lssctc.ProgramManagement.Dashboard.Services
{
    public interface ISimulationManagerDashboardService
    {
        /// <summary>
        /// Get simulation manager overview summary with total counts
        /// </summary>
        /// <param name="simulationManagerId">The ID of the simulation manager</param>
        /// <returns>Summary with counts for trainees, practices, tasks, and simulation sessions</returns>
        Task<SimulationManagerSummaryDto> GetManagerSummaryAsync(int simulationManagerId);

        /// <summary>
        /// Get monthly practice completion distribution (Completed vs NotCompleted) for a specific year
        /// </summary>
        /// <param name="simulationManagerId">The ID of the simulation manager</param>
        /// <param name="year">The year to filter practice attempts</param>
        /// <returns>Monthly distribution of practice attempts by completion status</returns>
        Task<IEnumerable<MonthlyPracticeCompletionDto>> GetPracticeCompletionDistributionAsync(int simulationManagerId, int year);

        /// <summary>
        /// Get practice duration distribution grouped by time ranges
        /// </summary>
        /// <param name="simulationManagerId">The ID of the simulation manager</param>
        /// <returns>Distribution of practice attempts by duration ranges</returns>
        Task<IEnumerable<PracticeDurationDistributionDto>> GetPracticeDurationDistributionAsync(int simulationManagerId);
    }
}
