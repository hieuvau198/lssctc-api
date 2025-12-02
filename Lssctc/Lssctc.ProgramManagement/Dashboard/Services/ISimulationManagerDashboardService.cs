using Lssctc.ProgramManagement.Dashboard.Dtos;

namespace Lssctc.ProgramManagement.Dashboard.Services
{
    public interface ISimulationManagerDashboardService
    {
        /// <summary>
        /// Get simulation manager overview summary with total counts
        /// </summary>
        /// <param name="simulationManagerId">The ID of the simulation manager</param>
        /// <returns>Summary with counts for trainees, practices, tasks, and active classes with simulations</returns>
        Task<SimulationManagerSummaryDto> GetManagerSummaryAsync(int simulationManagerId);

        /// <summary>
        /// Get practice completion distribution (Completed vs NotCompleted) for a specific year
        /// </summary>
        /// <param name="simulationManagerId">The ID of the simulation manager</param>
        /// <param name="year">The year to filter practice attempts</param>
        /// <returns>Distribution of practice attempts by completion status</returns>
        Task<IEnumerable<PracticeCompletionDistributionDto>> GetPracticeCompletionDistributionAsync(int simulationManagerId, int year);

        /// <summary>
        /// Get average score per practice across all current attempts
        /// </summary>
        /// <param name="simulationManagerId">The ID of the simulation manager</param>
        /// <returns>List of practices with their average scores and attempt counts</returns>
        Task<IEnumerable<PracticeAverageScoreDto>> GetAverageScorePerPracticeAsync(int simulationManagerId);
    }
}
