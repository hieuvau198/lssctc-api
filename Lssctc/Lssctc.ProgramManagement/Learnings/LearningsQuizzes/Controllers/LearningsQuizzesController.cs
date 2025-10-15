using Lssctc.ProgramManagement.Learnings.LearningsQuizzes.Dtos;
using Lssctc.ProgramManagement.Learnings.LearningsQuizzes.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Lssctc.ProgramManagement.Learnings.LearningsQuizzes.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LearningsQuizzesController : ControllerBase
    {
        private readonly ILearningsSectionQuizService _lsqService;
        public LearningsQuizzesController(ILearningsSectionQuizService learningsSectionQuizService)
        {
            _lsqService = learningsSectionQuizService;
        }
        #region Section Quizzes
        [HttpGet("sectionquizzes/partition/{partitionId:int}/trainee/{traineeId:int}")]
        public async Task<IActionResult> GetSectionQuizByPartitionIdAndTraineeId(int partitionId, int traineeId)
        {
            if (partitionId <= 0 || traineeId <= 0)
                return BadRequest("Invalid partition or trainee ID.");
            var result = await _lsqService.GetSectionQuizByPartitionIdAndTraineeId(partitionId, traineeId);
            if (result == null)
                return NotFound($"Section quiz with partitionId {partitionId} for traineeId {traineeId} not found.");
            return Ok(result);
        }

        [HttpPost("sectionquizzes/{partitionId:int}/trainee/{traineeId:int}/submit")]
        public async Task<IActionResult> SubmitSectionQuizAttempt(
            int partitionId,
            int traineeId,
            [FromBody] CreateLearningsSectionQuizAttemptDto attempt)
        {
            if (partitionId <= 0 || traineeId <= 0)
                return BadRequest("Invalid partitionId or traineeId.");

            if (attempt == null)
                return BadRequest("Attempt data cannot be null.");

            try
            {
                var result = await _lsqService.SubmitSectionQuizAttempt(partitionId, traineeId, attempt);
                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                // Optional: log the exception (e.g., using ILogger)
                return StatusCode(500, $"An unexpected error occurred: {ex.Message}");
            }
        }

        #endregion
    }
}
