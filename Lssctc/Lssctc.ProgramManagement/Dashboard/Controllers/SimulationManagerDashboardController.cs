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
        /// <returns>Summary with counts for trainees, practices, tasks, and simulation sessions</returns>
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
        /// Get monthly practice completion distribution (Completed vs NotCompleted) for a specific year
        /// </summary>
        /// <param name="simulationManagerId">The ID of the simulation manager</param>
        /// <param name="year">The year to filter practice attempts (default: current year)</param>
        /// <returns>Monthly distribution of practice attempts by completion status (Dual Column Chart)</returns>
        [HttpGet("{simulationManagerId}/practices/completion-distribution/monthly")]
        public async Task<IActionResult> GetPracticeCompletionDistribution(int simulationManagerId, [FromQuery] int year = 0)
        {
            if (simulationManagerId <= 0)
                return BadRequest(new { status = 400, message = "Invalid simulation manager ID.", type = "ValidationException" });

            // Default to current year if not provided or invalid
            if (year <= 0)
                year = DateTime.UtcNow.Year;

            // Validate year is reasonable
            if (year < 2000 || year > DateTime.UtcNow.Year + 1)
                return BadRequest(new { status = 400, message = "Invalid year specified.", type = "ValidationException" });

            try
            {
                var distribution = await _dashboardService.GetPracticeCompletionDistributionAsync(simulationManagerId, year);
                return Ok(new { status = 200, message = "Get monthly practice completion distribution", data = distribution });
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
        /// Get practice duration distribution grouped by time ranges
        /// </summary>
        /// <param name="simulationManagerId">The ID of the simulation manager</param>
        /// <returns>Distribution of practice attempts by duration ranges (Pie Chart)</returns>
        [HttpGet("{simulationManagerId}/practices/duration-distribution")]
        public async Task<IActionResult> GetPracticeDurationDistribution(int simulationManagerId)
        {
            if (simulationManagerId <= 0)
                return BadRequest(new { status = 400, message = "Invalid simulation manager ID.", type = "ValidationException" });

            try
            {
                var distribution = await _dashboardService.GetPracticeDurationDistributionAsync(simulationManagerId);
                return Ok(new { status = 200, message = "Get practice duration distribution", data = distribution });
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
