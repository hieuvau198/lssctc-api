using Lssctc.SimulationManagement.PracticeSteps.Dtos;
using Lssctc.SimulationManagement.PracticeSteps.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Lssctc.SimulationManagement.PracticeSteps.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PracticeStepsController : ControllerBase
    {
        private readonly IPracticeStepService _practiceStepService;

        public PracticeStepsController(IPracticeStepService practiceStepService)
        {
            _practiceStepService = practiceStepService;
        }

        [HttpGet("practice/{practiceId}")]
        public async Task<ActionResult<List<PracticeStepDto>>> GetPracticeStepsByPracticeId(int practiceId)
        {
            if (practiceId <= 0)
                return BadRequest(new { message = "Invalid practice ID." });

            var steps = await _practiceStepService.GetPracticeStepsByPracticeIdAsync(practiceId);
            return Ok(steps);
        }

        [HttpGet("{stepId}")]
        public async Task<ActionResult<PracticeStepDto>> GetPracticeStepById(int stepId)
        {
            if (stepId <= 0)
                return BadRequest(new { message = "Invalid step ID." });

            var step = await _practiceStepService.GetPracticeStepByIdAsync(stepId);
            if (step == null)
                return NotFound(new { message = $"PracticeStep with ID {stepId} not found." });

            return Ok(step);
        }

        [HttpPost]
        public async Task<ActionResult<PracticeStepDto>> CreatePracticeStep([FromBody] CreatePracticeStepDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var step = await _practiceStepService.CreatePracticeStepAsync(dto);

            return CreatedAtAction(nameof(GetPracticeStepById), new { stepId = step.Id }, step);
        }

        [HttpPut("{stepId}")]
        public async Task<ActionResult<PracticeStepDto>> UpdatePracticeStep(int stepId, [FromBody] UpdatePracticeStepDto dto)
        {
            if (stepId <= 0)
                return BadRequest(new { message = "Invalid step ID." });


            var step = await _practiceStepService.UpdatePracticeStepAsync(stepId, dto);
            if (step == null)
                return NotFound(new { message = $"PracticeStep with ID {stepId} not found." });

            return Ok(step);
        }

        [HttpDelete("{stepId}")]
        public async Task<ActionResult> DeletePracticeStep(int stepId)
        {
            if (stepId <= 0)
                return BadRequest(new { message = "Invalid step ID." });

            var success = await _practiceStepService.DeletePracticeStepAsync(stepId);
            if (!success)
                return NotFound(new { message = $"PracticeStep with ID {stepId} not found." });

            return NoContent();
        }
    }
}
