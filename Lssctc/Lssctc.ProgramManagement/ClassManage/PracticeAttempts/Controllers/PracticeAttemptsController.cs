using Lssctc.ProgramManagement.ClassManage.PracticeAttempts.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Lssctc.ProgramManagement.ClassManage.PracticeAttempts.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class PracticeAttemptsController : ControllerBase
    {
        private readonly IPracticeAttemptsService _practiceAttemptsService;

        public PracticeAttemptsController(IPracticeAttemptsService practiceAttemptsService)
        {
            _practiceAttemptsService = practiceAttemptsService;
        }

        #region Trainee Endpoints

        /// <summary>
        /// Get all practice attempts for a trainee's activity record
        /// </summary>
        /// <param name="traineeId">Trainee ID</param>
        /// <param name="activityRecordId">The activity record ID</param>
        /// <returns>List of all practice attempts ordered by attempt date (newest first)</returns>
        [HttpGet("attempts")]
        [Authorize(Roles = "Trainee, Admin")]
        public async Task<IActionResult> GetPracticeAttempts([FromQuery] int traineeId, [FromQuery] int activityRecordId)
        {
            try
            {
                var result = await _practiceAttemptsService.GetPracticeAttempts(traineeId, activityRecordId);
                return Ok(result);
            }
            catch (UnauthorizedAccessException ex) { return Unauthorized(new { message = ex.Message }); }
            catch (KeyNotFoundException ex) { return NotFound(new { message = ex.Message }); }
            catch (Exception ex) 
            { 
                Console.WriteLine($"Error: {ex.Message}");
                Console.WriteLine($"StackTrace: {ex.StackTrace}");
                return StatusCode(500, new { message = "An unexpected error occurred.", details = ex.Message }); 
            }
        }

        /// <summary>
        /// Get the latest (current) practice attempt for a trainee's activity record
        /// </summary>
        /// <param name="traineeId">Trainee ID</param>
        /// <param name="activityRecordId">The activity record ID</param>
        /// <returns>The current practice attempt or 404 if none exists</returns>
        [HttpGet("attempts/latest")]
        [Authorize(Roles = "Trainee, Admin")]
        public async Task<IActionResult> GetLatestPracticeAttempt([FromQuery] int traineeId, [FromQuery] int activityRecordId)
        {
            try
            {
                var result = await _practiceAttemptsService.GetLatestPracticeAttempt(traineeId, activityRecordId);
                if (result == null)
                    return NotFound(new { message = "No practice attempt found." });
                return Ok(result);
            }
            catch (UnauthorizedAccessException ex) { return Unauthorized(new { message = ex.Message }); }
            catch (KeyNotFoundException ex) { return NotFound(new { message = ex.Message }); }
            catch (Exception ex) 
            { 
                Console.WriteLine($"Error: {ex.Message}");
                Console.WriteLine($"StackTrace: {ex.StackTrace}");
                return StatusCode(500, new { message = "An unexpected error occurred.", details = ex.Message }); 
            }
        }

        #endregion
    }
}
