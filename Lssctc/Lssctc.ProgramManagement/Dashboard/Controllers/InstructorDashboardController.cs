using Lssctc.ProgramManagement.Dashboard.Dtos;
using Lssctc.ProgramManagement.Dashboard.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

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
        /// <param name="instructorId">The ID of the instructor</param>
        /// <returns>Instructor summary with counts for assigned trainees, classes, materials, and quizzes</returns>
        [HttpGet("{instructorId}/summary")]
        public async Task<IActionResult> GetInstructorSummary(int instructorId)
        {
            if (instructorId <= 0)
                return BadRequest(new { status = 400, message = "Invalid instructor ID.", type = "ValidationException" });

            try
            {
                var summary = await _dashboardService.GetInstructorSummaryAsync(instructorId);
                return Ok(new { status = 200, message = "Get instructor summary", data = summary });
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
        /// Get top N classes by trainee count for a specific instructor
        /// </summary>
        /// <param name="instructorId">The ID of the instructor</param>
        /// <param name="top">Number of top classes to return (default: 5)</param>
        /// <returns>List of classes with their trainee counts</returns>
        [HttpGet("{instructorId}/classes/top-by-trainees")]
        public async Task<IActionResult> GetTopClassesByTraineeCount(int instructorId, [FromQuery] int top = 5)
        {
            if (instructorId <= 0)
                return BadRequest(new { status = 400, message = "Invalid instructor ID.", type = "ValidationException" });

            if (top <= 0) top = 5;
            if (top > 20) top = 20; // Limit to prevent excessive data

            try
            {
                var topClasses = await _dashboardService.GetTopClassesByTraineeCountAsync(instructorId, top);
                return Ok(new { status = 200, message = "Get top classes by trainee count", data = topClasses });
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
        /// Get class status distribution for a specific instructor
        /// </summary>
        /// <param name="instructorId">The ID of the instructor</param>
        /// <returns>Distribution of class statuses for the instructor's classes</returns>
        [HttpGet("{instructorId}/classes/status-distribution")]
        public async Task<IActionResult> GetInstructorClassStatusDistribution(int instructorId)
        {
            if (instructorId <= 0)
                return BadRequest(new { status = 400, message = "Invalid instructor ID.", type = "ValidationException" });

            try
            {
                var distribution = await _dashboardService.GetInstructorClassStatusDistributionAsync(instructorId);
                return Ok(new { status = 200, message = "Get instructor class status distribution", data = distribution });
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
        /// Get yearly course completion trends for a specific instructor
        /// </summary>
        /// <param name="instructorId">The ID of the instructor</param>
        /// <param name="year">Year to get completion trends for (default: current year)</param>
        /// <returns>Monthly completion counts for the specified year in instructor's classes</returns>
        [HttpGet("{instructorId}/completions/trends/yearly")]
        public async Task<IActionResult> GetInstructorYearlyCompletionTrends(int instructorId, [FromQuery] int year = 0)
        {
            if (instructorId <= 0)
                return BadRequest(new { status = 400, message = "Invalid instructor ID.", type = "ValidationException" });

            // Default to current year if not specified
            if (year <= 0)
                year = DateTime.UtcNow.Year;

            // Validate year is reasonable
            if (year < 2000 || year > DateTime.UtcNow.Year + 1)
                return BadRequest(new { status = 400, message = "Invalid year specified.", type = "ValidationException" });

            try
            {
                var trends = await _dashboardService.GetInstructorYearlyCompletionTrendsAsync(instructorId, year);
                return Ok(new { status = 200, message = "Get instructor yearly completion trends", data = trends });
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
        /// Get class grade distribution for a specific instructor (Pie Chart)
        /// </summary>
        /// <param name="instructorId">The ID of the instructor</param>
        /// <returns>Distribution of classes by performance ranges (Excellent, Good, Average, Poor)</returns>
        [HttpGet("{instructorId}/classes/grade-distribution")]
        public async Task<IActionResult> GetClassGradeDistribution(int instructorId)
        {
            if (instructorId <= 0)
                return BadRequest(new { status = 400, message = "Invalid instructor ID.", type = "ValidationException" });

            try
            {
                var distribution = await _dashboardService.GetClassGradeDistributionAsync(instructorId);
                return Ok(new { status = 200, message = "Get class grade distribution", data = distribution });
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
