using Lssctc.ProgramManagement.Dashboard.Dtos;
using Lssctc.ProgramManagement.Dashboard.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Lssctc.ProgramManagement.Dashboard.Controllers
{
    [Route("api/simulation-manager/dashboard")]
    [ApiController]
    [Authorize(Roles = "Admin, SimulationManager")]
    public class SimulationManagerDashboardController : ControllerBase
    {
        private readonly ISimulationManagerDashboardService _dashboardService;

        public SimulationManagerDashboardController(ISimulationManagerDashboardService dashboardService)
        {
            _dashboardService = dashboardService;
        }

        #region Part 1: Overview (Summary Cards)

        /// <summary>
        /// Get simulation manager overview summary with total counts
        /// </summary>
        /// <param name="simulationManagerId">The ID of the simulation manager</param>
        /// <returns>Summary with counts for trainees, practices, tasks, and active classes with simulations</returns>
        [HttpGet("{simulationManagerId}/summary")]
        public async Task<IActionResult> GetManagerSummary(int simulationManagerId)
        {
            if (simulationManagerId <= 0)
                return BadRequest(new { status = 400, message = "Invalid simulation manager ID.", type = "ValidationException" });

            try
            {
                var summary = await _dashboardService.GetManagerSummaryAsync(simulationManagerId);
                return Ok(new { status = 200, message = "Get simulation manager summary", data = summary });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { status = 404, message = ex.Message, type = "NotFound" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { status = 500, message = ex.Message, type = ex.GetType().Name });
            }
        }

        #endregion

        #region Part 2: Charts & Analytics

        /// <summary>
        /// Get practice completion distribution (Completed vs NotCompleted) for a specific year
        /// </summary>
        /// <param name="simulationManagerId">The ID of the simulation manager</param>
        /// <param name="year">The year to filter practice attempts (default: current year)</param>
        /// <returns>Distribution of practice attempts by completion status</returns>
        [HttpGet("{simulationManagerId}/practices/completion-distribution")]
        public async Task<IActionResult> GetPracticeCompletionDistribution(int simulationManagerId, [FromQuery] int year = 0)
        {
            if (simulationManagerId <= 0)
                return BadRequest(new { status = 400, message = "Invalid simulation manager ID.", type = "ValidationException" });

            // Default to current year if not provided or invalid
            if (year <= 0 || year < 2000 || year > 2100)
            {
                year = DateTime.Now.Year;
            }

            try
            {
                var distribution = await _dashboardService.GetPracticeCompletionDistributionAsync(simulationManagerId, year);
                return Ok(new { status = 200, message = "Get practice completion distribution", data = distribution });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { status = 404, message = ex.Message, type = "NotFound" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { status = 500, message = ex.Message, type = ex.GetType().Name });
            }
        }

        /// <summary>
        /// Get average score per practice across all current attempts
        /// </summary>
        /// <param name="simulationManagerId">The ID of the simulation manager</param>
        /// <returns>List of practices with their average scores and attempt counts</returns>
        [HttpGet("{simulationManagerId}/practices/average-scores")]
        public async Task<IActionResult> GetAverageScorePerPractice(int simulationManagerId)
        {
            if (simulationManagerId <= 0)
                return BadRequest(new { status = 400, message = "Invalid simulation manager ID.", type = "ValidationException" });

            try
            {
                var averageScores = await _dashboardService.GetAverageScorePerPracticeAsync(simulationManagerId);
                return Ok(new { status = 200, message = "Get average scores per practice", data = averageScores });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { status = 404, message = ex.Message, type = "NotFound" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { status = 500, message = ex.Message, type = ex.GetType().Name });
            }
        }

        #endregion
    }
}
