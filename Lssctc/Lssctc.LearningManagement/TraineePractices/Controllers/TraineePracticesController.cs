using Lssctc.LearningManagement.TraineePractices.Dtos;
using Lssctc.LearningManagement.TraineePractices.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Lssctc.LearningManagement.TraineePractices.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TraineePracticesController : ControllerBase
    {
        private readonly ITraineePracticeService _traineePracticeService;
        private readonly ITraineeStepService _traineeStepService;
        public TraineePracticesController(ITraineePracticeService traineePracticeService, ITraineeStepService traineeStepService)
        {
            _traineePracticeService = traineePracticeService;
            _traineeStepService = traineeStepService;
        }

        [HttpGet("practice/{practiceId}/trainee/{traineeId}")]
        public async Task<IActionResult> GetTraineePracticeByIdA(int practiceId, int traineeId)
        {
            try
            {
                var result = await _traineePracticeService.GetTraineePracticeByIdA(practiceId, traineeId);
                if (result == null)
                    return NotFound($"No practice found for Practice ID {practiceId} and Trainee ID {traineeId}.");

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

        [HttpGet("attempt/{attemptId}/steps")]
        public async Task<IActionResult> GetTraineeStepsByAttemptId(int attemptId)
        {
            try
            {
                var result = await _traineeStepService.GetTraineeStepsByAttemptId(attemptId);
                if (result == null || !result.Any())
                    return NotFound($"No trainee steps found for Attempt ID {attemptId}.");

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


        [HttpGet("trainee/{traineeId}/class/{classId}")]
        public async Task<IActionResult> GetTraineePracticesByTraineeIdAndClassId(int traineeId, int classId)
        {
            try
            {
                var result = await _traineePracticeService.GetTraineePracticesByTraineeIdAndClassId(traineeId, classId);
                if (result == null || !result.Any())
                    return NotFound($"No trainee practices found for Trainee ID {traineeId} in Class ID {classId}.");

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

        [HttpGet("step/{stepId}/trainee/{traineeId}")]
        public async Task<IActionResult> GetTraineeStepByIdAndTraineeId(int stepId, int traineeId)
        {
            try
            {
                var result = await _traineeStepService.GetTraineeStepByIdAndTraineeId(stepId, traineeId);
                if (result == null)
                    return NotFound($"No step found for Step ID {stepId} and Trainee ID {traineeId}.");
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

        [HttpGet("practice/{practiceId}/trainee/{traineeId}/steps")]
        public async Task<IActionResult> GetTraineeStepsByPracticeIdAndTraineeId(int practiceId, int traineeId)
        {
            try
            {
                var result = await _traineeStepService.GetTraineeStepsByPracticeIdAndTraineeId(practiceId, traineeId);
                if (result == null || !result.Any())
                    return NotFound($"No steps found for Practice ID {practiceId} and Trainee ID {traineeId}.");

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

        [HttpPost("attempt/{attemptId}/trainee/{traineeId}/submit")]
        public async Task<IActionResult> SubmitTraineeStepAttempt(
        int attemptId,
        int traineeId,
        [FromBody] UpdateTraineeStepAttemptDto input)
            {
                try
                {
                    var result = await _traineeStepService.SubmitTraineeStepAttempt(attemptId, traineeId, input);

                    if (!result)
                        return BadRequest("Failed to submit trainee step attempt.");

                    return Ok(new { message = "Trainee step attempt submitted successfully." });
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
    }
}
