using Lssctc.SimulationManagement.TraineePractices.Dtos;
using Lssctc.SimulationManagement.TraineePractices.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Lssctc.SimulationManagement.TraineePractices.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TraineePracticesController : ControllerBase
    {
        private readonly ITraineePracticeService _traineePracticeService;
        public TraineePracticesController(ITraineePracticeService traineePracticeService)
        {
            _traineePracticeService = traineePracticeService;
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
    }
}
