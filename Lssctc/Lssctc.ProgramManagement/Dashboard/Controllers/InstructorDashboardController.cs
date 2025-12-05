using Lssctc.ProgramManagement.Dashboard.Dtos;
using Lssctc.ProgramManagement.Dashboard.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Lssctc.ProgramManagement.Dashboard.Controllers
{
    [Route("api/instructor/dashboard")]
    [ApiController]
    [Authorize(Roles = "Admin, Instructor")]
    public class InstructorDashboardController : ControllerBase
    {
        private readonly IInstructorDashboardService _dashboardService;

        public InstructorDashboardController(IInstructorDashboardService dashboardService)
        {
            _dashboardService = dashboardService;
        }

        #region Part 1: Overview (Summary Cards)

        /// <summary>
        /// Get instructor overview summary with total counts
        /// </summary>
        /// <returns>Instructor summary with counts for assigned trainees, classes, materials, and quizzes</returns>
        [HttpGet("summary")]
        public async Task<IActionResult> GetInstructorSummary()
        {
            try
            {
                var userId = GetUserIdFromClaims();
                var summary = await _dashboardService.GetInstructorSummaryAsync(userId);
                return Ok(new { status = 200, message = "Get instructor summary", data = summary });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { status = 401, message = ex.Message, type = "UnauthorizedAccess" });
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
        /// Get top N classes by trainee count for the current instructor
        /// </summary>
        /// <param name="top">Number of top classes to return (default: 5)</param>
        /// <returns>List of classes with their trainee counts</returns>
        [HttpGet("classes/top-by-trainees")]
        public async Task<IActionResult> GetTopClassesByTraineeCount([FromQuery] int top = 5)
        {
            if (top <= 0) top = 5;
            if (top > 20) top = 20; // Limit to prevent excessive data

            try
            {
                var userId = GetUserIdFromClaims();
                var topClasses = await _dashboardService.GetTopClassesByTraineeCountAsync(userId, top);
                return Ok(new { status = 200, message = "Get top classes by trainee count", data = topClasses });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { status = 401, message = ex.Message, type = "UnauthorizedAccess" });
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
        /// Get class status distribution for the current instructor
        /// </summary>
        /// <returns>Distribution of class statuses for the instructor's classes</returns>
        [HttpGet("classes/status-distribution")]
        public async Task<IActionResult> GetInstructorClassStatusDistribution()
        {
            try
            {
                var userId = GetUserIdFromClaims();
                var distribution = await _dashboardService.GetInstructorClassStatusDistributionAsync(userId);
                return Ok(new { status = 200, message = "Get instructor class status distribution", data = distribution });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { status = 401, message = ex.Message, type = "UnauthorizedAccess" });
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
        /// Get yearly course completion trends for the current instructor
        /// </summary>
        /// <param name="year">Year to get completion trends for (default: current year)</param>
        /// <returns>Monthly completion counts for the specified year in instructor's classes</returns>
        [HttpGet("completions/trends/yearly")]
        public async Task<IActionResult> GetInstructorYearlyCompletionTrends([FromQuery] int year = 0)
        {
            // Default to current year if not specified
            if (year <= 0)
                year = DateTime.UtcNow.Year;

            // Validate year is reasonable
            if (year < 2000 || year > DateTime.UtcNow.Year + 1)
                return BadRequest(new { status = 400, message = "Invalid year specified.", type = "ValidationException" });

            try
            {
                var userId = GetUserIdFromClaims();
                var trends = await _dashboardService.GetInstructorYearlyCompletionTrendsAsync(userId, year);
                return Ok(new { status = 200, message = "Get instructor yearly completion trends", data = trends });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { status = 401, message = ex.Message, type = "UnauthorizedAccess" });
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
        /// Get class grade distribution for the current instructor (Pie Chart)
        /// </summary>
        /// <returns>Distribution of classes by performance ranges (Excellent, Good, Average, Poor)</returns>
        [HttpGet("classes/grade-distribution")]
        public async Task<IActionResult> GetClassGradeDistribution()
        {
            try
            {
                var userId = GetUserIdFromClaims();
                var distribution = await _dashboardService.GetClassGradeDistributionAsync(userId);
                return Ok(new { status = 200, message = "Get class grade distribution", data = distribution });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { status = 401, message = ex.Message, type = "UnauthorizedAccess" });
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

        #region Helpers

        private int GetUserIdFromClaims()
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
