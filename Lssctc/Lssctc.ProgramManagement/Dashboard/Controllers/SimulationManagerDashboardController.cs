using Lssctc.ProgramManagement.Dashboard.Dtos;
using Lssctc.ProgramManagement.Dashboard.Services;
using Lssctc.ProgramManagement.HttpCustomResponse;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

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
        /// <returns>Summary with counts for trainees, practices, tasks, and simulation sessions</returns>
        [HttpGet("summary")]
        public async Task<IActionResult> GetManagerSummary()
        {
            try
            {
                var currentUserId = GetCurrentUserIdFromClaims();
                var summary = await _dashboardService.GetManagerSummaryAsync(currentUserId);
                return Ok(new { status = 200, message = "Get simulation manager summary", data = summary });
            }
            catch (BadRequestException ex)
            {
                return BadRequest(new { status = 400, message = ex.Message, type = "BadRequestException" });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { status = 401, message = ex.Message, type = "UnauthorizedAccessException" });
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
        /// <param name="year">The year to filter practice attempts (default: current year)</param>
        /// <returns>Monthly distribution of practice attempts by completion status (Dual Column Chart)</returns>
        [HttpGet("practices/completion-distribution/monthly")]
        public async Task<IActionResult> GetPracticeCompletionDistribution([FromQuery] int year = 0)
        {
            // Default to current year if not provided or invalid
            if (year <= 0)
                year = DateTime.UtcNow.Year;

            // Validate year is reasonable
            if (year < 2000 || year > DateTime.UtcNow.Year + 1)
                return BadRequest(new { status = 400, message = "Invalid year specified.", type = "ValidationException" });

            try
            {
                var currentUserId = GetCurrentUserIdFromClaims();
                var distribution = await _dashboardService.GetPracticeCompletionDistributionAsync(currentUserId, year);
                return Ok(new { status = 200, message = "Get monthly practice completion distribution", data = distribution });
            }
            catch (BadRequestException ex)
            {
                return BadRequest(new { status = 400, message = ex.Message, type = "BadRequestException" });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { status = 401, message = ex.Message, type = "UnauthorizedAccessException" });
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
        /// <returns>Distribution of practice attempts by duration ranges (Pie Chart)</returns>
        [HttpGet("practices/duration-distribution")]
        public async Task<IActionResult> GetPracticeDurationDistribution()
        {
            try
            {
                var currentUserId = GetCurrentUserIdFromClaims();
                var distribution = await _dashboardService.GetPracticeDurationDistributionAsync(currentUserId);
                return Ok(new { status = 200, message = "Get practice duration distribution", data = distribution });
            }
            catch (BadRequestException ex)
            {
                return BadRequest(new { status = 400, message = ex.Message, type = "BadRequestException" });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { status = 401, message = ex.Message, type = "UnauthorizedAccessException" });
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

        #region Private Helpers

        private int GetCurrentUserIdFromClaims()
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (int.TryParse(userIdClaim, out int userId))
            {
                return userId;
            }

            throw new UnauthorizedAccessException("User ID claim is missing or invalid.");
        }

        #endregion
    }
}
