using Lssctc.LearningManagement.PracticeAttempts.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Lssctc.LearningManagement.PracticeAttempts.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PracticeAttemptsController : ControllerBase
    {
        private readonly IPracticeAttemptService _practiceAttemptService;
        public PracticeAttemptsController(IPracticeAttemptService practiceAttemptService)
        {
            _practiceAttemptService = practiceAttemptService;
        }

        [HttpPost("section-practice/{sectionPracticeId}/trainee/{traineeId}")]
        public async Task<IActionResult> CreatePracticeAttempt(int sectionPracticeId, int traineeId)
        {
            try
            {
                var result = await _practiceAttemptService.CreatePracticeAttempt(sectionPracticeId, traineeId);
                return Ok(result);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = ex.Message });
            }
        }

        [HttpGet("section-practice/{sectionPracticeId}/trainee/{traineeId}")]
        public async Task<IActionResult> GetPracticeAttemptsByPracticeIdAndTraineeId(int sectionPracticeId, int traineeId)
        {
            try
            {
                var result = await _practiceAttemptService.GetPracticeAttemptsByPracticeIdAndTraineeId(sectionPracticeId, traineeId);
                if (result == null || !result.Any())
                    return NotFound($"No practice attempts found for Section Practice ID {sectionPracticeId} and Trainee ID {traineeId}.");
                return Ok(result);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = ex.Message });
            }
        }

        [HttpGet("attempt/{attemptId}")]
        public async Task<IActionResult> GetPracticeAttemptByIdAndTraineeId(int attemptId)
        {
            try
            {
                var result = await _practiceAttemptService.GetPracticeAttemptById(attemptId);
                if (result == null)
                    return NotFound($"No practice attempt found for Attempt ID {attemptId}");
                return Ok(result);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = ex.Message });
            }
        }

        [HttpDelete("attempt/{attemptId}")]
        public async Task<IActionResult> DeletePracticeAttempt(int attemptId)
        {
            try
            {
                var success = await _practiceAttemptService.DeletePracticeAttempt(attemptId);
                if (!success)
                    return NotFound($"No practice attempt found for Attempt ID {attemptId}.");
                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = ex.Message });
            }
        }

        [HttpPut("attempt/{attemptId}/complete")]
        public async Task<IActionResult> UpdatePracticeAttemptCompleteByAttemptId(int attemptId)
        {
            try
            {
                var result = await _practiceAttemptService.ConfirmPracticeAttemptComplete(attemptId);
                return Ok(result);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "An unexpected error occurred.", detail = ex.Message });
            }
        }
    }
}
