using Lssctc.ProgramManagement.Dashboard.Dtos;
using Lssctc.ProgramManagement.Dashboard.Services;
using Microsoft.AspNetCore.Mvc;

namespace Lssctc.ProgramManagement.Dashboard.Controllers
{
    [Route("api/admin/dashboard")]
    [ApiController]
    public class AdminDashboardController : ControllerBase
    {
        private readonly IAdminDashboardService _dashboardService;

        public AdminDashboardController(IAdminDashboardService dashboardService)
        {
            _dashboardService = dashboardService;
        }

        #region Part 1: System Overview

        /// <summary>
        /// Get system overview summary with total counts
        /// </summary>
        /// <returns>System summary with counts for programs, courses, trainees, instructors, classes, and practices</returns>
        [HttpGet("summary")]
        [ProducesResponseType(typeof(SystemSummaryDto), 200)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> GetSystemSummary()
        {
            try
            {
                var summary = await _dashboardService.GetSystemSummaryAsync();
                return Ok(summary);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An internal server error occurred: {ex.Message}");
            }
        }

        #endregion

        #region Part 2: Advanced Statistics

        /// <summary>
        /// Get top N most popular courses based on enrollment count
        /// </summary>
        /// <param name="top">Number of top courses to return (default: 5)</param>
        /// <returns>List of popular courses with enrollment counts</returns>
        [HttpGet("courses/popular")]
        [ProducesResponseType(typeof(IEnumerable<PopularCourseDto>), 200)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> GetTopPopularCourses([FromQuery] int top = 5)
        {
            try
            {
                if (top <= 0) top = 5;
                if (top > 20) top = 20; // Limit to prevent excessive data

                var popularCourses = await _dashboardService.GetTopPopularCoursesAsync(top);
                return Ok(popularCourses);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An internal server error occurred: {ex.Message}");
            }
        }

        /// <summary>
        /// Get top N most active trainees based on number of enrolled courses
        /// </summary>
        /// <param name="top">Number of top trainees to return (default: 5)</param>
        /// <returns>List of active trainees with enrolled course counts</returns>
        [HttpGet("trainees/active")]
        [ProducesResponseType(typeof(IEnumerable<ActiveTraineeDto>), 200)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> GetTopActiveTrainees([FromQuery] int top = 5)
        {
            try
            {
                if (top <= 0) top = 5;
                if (top > 20) top = 20; // Limit to prevent excessive data

                var activeTrainees = await _dashboardService.GetTopActiveTraineesAsync(top);
                return Ok(activeTrainees);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An internal server error occurred: {ex.Message}");
            }
        }

        /// <summary>
        /// Get class status distribution (count of classes by status)
        /// </summary>
        /// <returns>List of class statuses with their counts</returns>
        [HttpGet("classes/status-distribution")]
        [ProducesResponseType(typeof(IEnumerable<ClassStatusDistributionDto>), 200)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> GetClassStatusDistribution()
        {
            try
            {
                var distribution = await _dashboardService.GetClassStatusDistributionAsync();
                return Ok(distribution);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An internal server error occurred: {ex.Message}");
            }
        }

        /// <summary>
        /// Get course completion trends for a specific year (monthly breakdown)
        /// </summary>
        /// <param name="year">Year to get completion trends for (default: current year)</param>
        /// <returns>Monthly completion counts for the specified year</returns>
        [HttpGet("completions/trends")]
        [ProducesResponseType(typeof(IEnumerable<CourseCompletionTrendDto>), 200)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> GetCourseCompletionTrends([FromQuery] int year = 0)
        {
            try
            {
                if (year <= 0)
                    year = DateTime.UtcNow.Year;

                // Validate year is reasonable
                if (year < 2000 || year > DateTime.UtcNow.Year + 1)
                    return BadRequest("Invalid year specified.");

                var trends = await _dashboardService.GetCourseCompletionTrendsAsync(year);
                return Ok(trends);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An internal server error occurred: {ex.Message}");
            }
        }

        #endregion
    }
}
