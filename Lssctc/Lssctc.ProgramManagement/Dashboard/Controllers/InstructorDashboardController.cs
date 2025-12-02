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
        /// Get average trainee scores per class for a specific instructor
        /// </summary>
        /// <param name="instructorId">The ID of the instructor</param>
        /// <returns>List of classes with their average trainee scores</returns>
        [HttpGet("{instructorId}/classes/average-scores")]
        public async Task<IActionResult> GetAverageScorePerClass(int instructorId)
        {
            if (instructorId <= 0)
                return BadRequest(new { status = 400, message = "Invalid instructor ID.", type = "ValidationException" });

            try
            {
                var averageScores = await _dashboardService.GetAverageScorePerClassAsync(instructorId);
                return Ok(new { status = 200, message = "Get average scores per class", data = averageScores });
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
