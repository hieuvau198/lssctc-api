using Lssctc.SimulationManagement.StepComponents.Dtos;
using Lssctc.SimulationManagement.StepComponents.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Lssctc.SimulationManagement.StepComponents.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PracticeStepComponentsController : ControllerBase
    {
        private readonly IPracticeStepComponentService _service;

        public PracticeStepComponentsController(IPracticeStepComponentService service)
        {
            _service = service;
        }

        [HttpGet("{stepId}")]
        public async Task<ActionResult<List<PracticeStepComponentDto>>> Get(int stepId)
        {
            try
            {
                var result = await _service.GetByPracticeStepIdAsync(stepId);
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
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpPost]
        public async Task<ActionResult<PracticeStepComponentDto>> Create([FromBody] CreatePracticeStepComponentDto dto)
        {
            try
            {
                var result = await _service.AssignSimulationComponentAsync(dto);
                return Ok(result);
            }
            catch (ArgumentNullException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<PracticeStepComponentDto>> UpdateOrder(int id, [FromBody] UpdatePracticeStepComponentDto dto)
        {
            try
            {
                var result = await _service.UpdateOrderAsync(id, dto);
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
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(int id)
        {
            try
            {
                var success = await _service.RemoveAsync(id);
                if (!success)
                    return NotFound($"PracticeStepComponent with ID {id} not found.");
                return NoContent();
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
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
    }
}
