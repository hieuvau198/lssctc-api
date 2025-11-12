using Lssctc.ProgramManagement.ClassManage.QuizAttempts.Dtos;
using Lssctc.ProgramManagement.ClassManage.QuizAttempts.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Lssctc.ProgramManagement.ClassManage.QuizAttempts.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class QuizAttemptsController : ControllerBase
    {
        private readonly IQuizAttemptsService _quizAttemptsService;

        public QuizAttemptsController(IQuizAttemptsService quizAttemptsService)
        {
            _quizAttemptsService = quizAttemptsService;
        }

        #region Trainee Endpoints

        [HttpGet("my-attempts/activity-record/{activityRecordId}")]
        [Authorize(Roles = "Trainee")]
        public async Task<IActionResult> GetMyQuizAttempts(int activityRecordId)
        {
            try
            {
                var traineeId = GetUserIdFromClaims();
                var result = await _quizAttemptsService.GetQuizAttemptsAsync(traineeId, activityRecordId);
                return Ok(result);
            }
            catch (UnauthorizedAccessException ex) { return Unauthorized(new { message = ex.Message }); }
            catch (KeyNotFoundException ex) { return NotFound(new { message = ex.Message }); }
            catch (Exception) { return StatusCode(500, new { message = "An unexpected error occurred." }); }
        }

        [HttpGet("my-attempts/activity-record/{activityRecordId}/latest")]
        [Authorize(Roles = "Trainee")]
        public async Task<IActionResult> GetMyLatestQuizAttempt(int activityRecordId)
        {
            try
            {
                var traineeId = GetUserIdFromClaims();
                var result = await _quizAttemptsService.GetLatestQuizAttemptAsync(traineeId, activityRecordId);
                if (result == null)
                    return NotFound();
                return Ok(result);
            }
            catch (UnauthorizedAccessException ex) { return Unauthorized(new { message = ex.Message }); }
            catch (KeyNotFoundException ex) { return NotFound(new { message = ex.Message }); }
            catch (Exception) { return StatusCode(500, new { message = "An unexpected error occurred." }); }
        }

        [HttpPost("my-attempts/submit")]
        [Authorize(Roles = "Trainee")]
        // --- ADDED/MODIFIED THESE LINES ---
        [ProducesResponseType(typeof(QuizAttemptDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        // --- END OF ADDITIONS ---
        public async Task<IActionResult> SubmitQuizAttempt([FromBody] SubmitQuizDto dto)
        {
            try
            {
                var traineeId = GetUserIdFromClaims();
                var result = await _quizAttemptsService.SubmitQuizAttemptAsync(traineeId, dto);
                return Ok(result);
            }
            catch (UnauthorizedAccessException ex) { return Unauthorized(new { message = ex.Message }); }
            catch (KeyNotFoundException ex) { return NotFound(new { message = ex.Message }); }
            catch (InvalidOperationException ex) { return BadRequest(new { message = ex.Message }); }
            catch (Exception) { return StatusCode(500, new { message = "An unexpected error occurred." }); }
        }

        #endregion

        #region Admin/Instructor Endpoints

        [HttpGet("activity/{activityId}/latest-attempts")]
        [Authorize(Roles = "Admin, Instructor")]
        public async Task<IActionResult> GetLatestAttemptsForActivity(int activityId)
        {
            try
            {
                var result = await _quizAttemptsService.GetLatestAttemptsForActivityAsync(activityId);
                return Ok(result);
            }
            catch (Exception) { return StatusCode(500, new { message = "An unexpected error occurred." }); }
        }

        #endregion

        private int GetUserIdFromClaims()
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (int.TryParse(userIdClaim, out int userId))
            {
                return userId;
            }

            throw new UnauthorizedAccessException("User ID claim is missing or invalid.");
        }
    }
}
