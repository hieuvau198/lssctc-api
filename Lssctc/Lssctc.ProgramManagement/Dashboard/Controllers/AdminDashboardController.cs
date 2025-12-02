using Lssctc.ProgramManagement.Dashboard.Dtos;
using Lssctc.ProgramManagement.Dashboard.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Lssctc.ProgramManagement.Dashboard.Controllers
{
    [Route("api/admin/dashboard")]
    [ApiController]
    [Authorize(Roles = "Admin")]
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
        public async Task<IActionResult> GetSystemSummary()
        {
            try
            {
                var summary = await _dashboardService.GetSystemSummaryAsync();
                return Ok(new { status = 200, message = "Get system summary", data = summary });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { status = 500, message = ex.Message, type = ex.GetType().Name });
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
        public async Task<IActionResult> GetTopPopularCourses([FromQuery] int top = 5)
        {
            try
            {
                if (top <= 0) top = 5;
                if (top > 20) top = 20; // Limit to prevent excessive data

                var popularCourses = await _dashboardService.GetTopPopularCoursesAsync(top);
                return Ok(new { status = 200, message = "Get top popular courses", data = popularCourses });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { status = 500, message = ex.Message, type = ex.GetType().Name });
            }
        }

        /// <summary>
        /// Get top N most active trainees based on number of enrolled courses
        /// </summary>
        /// <param name="top">Number of top trainees to return (default: 5)</param>
        /// <returns>List of active trainees with enrolled course counts</returns>
        [HttpGet("trainees/active")]
        public async Task<IActionResult> GetTopActiveTrainees([FromQuery] int top = 5)
        {
            try
            {
                if (top <= 0) top = 5;
                if (top > 20) top = 20; // Limit to prevent excessive data

                var activeTrainees = await _dashboardService.GetTopActiveTraineesAsync(top);
                return Ok(new { status = 200, message = "Get top active trainees", data = activeTrainees });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { status = 500, message = ex.Message, type = ex.GetType().Name });
            }
        }

        /// <summary>
        /// Get class status distribution (count of classes by status)
        /// </summary>
        /// <returns>List of class statuses with their counts</returns>
        [HttpGet("classes/status-distribution")]
        public async Task<IActionResult> GetClassStatusDistribution()
        {
            try
            {
                var distribution = await _dashboardService.GetClassStatusDistributionAsync();
                return Ok(new { status = 200, message = "Get class status distribution", data = distribution });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { status = 500, message = ex.Message, type = ex.GetType().Name });
            }
        }

        /// <summary>
        /// Get course completion trends for a specific year (monthly breakdown)
        /// </summary>
        /// <param name="year">Year to get completion trends for (default: current year)</param>
        /// <returns>Monthly completion counts for the specified year</returns>
        [HttpGet("completions/trends")]
        public async Task<IActionResult> GetCourseCompletionTrends([FromQuery] int year = 0)
        {
            try
            {
                if (year <= 0)
                    year = DateTime.UtcNow.Year;

                // Validate year is reasonable
                if (year < 2000 || year > DateTime.UtcNow.Year + 1)
                    return BadRequest(new { status = 400, message = "Invalid year specified.", type = "ValidationException" });

                var trends = await _dashboardService.GetCourseCompletionTrendsAsync(year);
                return Ok(new { status = 200, message = "Get course completion trends", data = trends });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { status = 500, message = ex.Message, type = ex.GetType().Name });
            }
        }

        #endregion
    }
}
