using Lssctc.SimulationManagement.StepActions.Dtos;
using Lssctc.SimulationManagement.StepActions.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace Lssctc.SimulationManagement.StepActions.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class StepActionsController : ControllerBase
    {
        private readonly IStepActionService _stepActionService;
        public StepActionsController(IStepActionService stepActionService)
        {
            _stepActionService = stepActionService;
        }
        [HttpGet]
        public async Task<IActionResult> GetAllStepActions()
        {
            try
            {
                var result = await _stepActionService.GetAllStepActionsAsync();
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode((int)HttpStatusCode.InternalServerError, new { Message = ex.Message });
            }
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetStepActionById(int id)
        {
            try
            {
                var result = await _stepActionService.GetStepActionByIdAsync(id);
                return Ok(result);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode((int)HttpStatusCode.InternalServerError, new { Message = ex.Message });
            }
        }

        [HttpGet("step/{stepId:int}")]
        public async Task<IActionResult> GetStepActionByStepId(int stepId)
        {
            try
            {
                var result = await _stepActionService.GetStepActionByStepId(stepId);
                return Ok(result);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { Message = ex.Message });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode((int)HttpStatusCode.InternalServerError, new { Message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> CreateStepAction([FromBody] CreateStepActionDto dto)
        {
            try
            {
                var created = await _stepActionService.CreateStepActionAsync(dto);
                return CreatedAtAction(nameof(GetStepActionById), new { id = created.StepActionId }, created);
            }
            catch (ArgumentNullException ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode((int)HttpStatusCode.InternalServerError, new { Message = ex.Message });
            }
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> UpdateStepAction(int id, [FromBody] UpdateStepActionDto dto)
        {
            try
            {
                var updated = await _stepActionService.UpdateStepActionAsync(id, dto);
                return Ok(updated);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { Message = ex.Message });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode((int)HttpStatusCode.InternalServerError, new { Message = ex.Message });
            }
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteStepAction(int id)
        {
            try
            {
                var deleted = await _stepActionService.DeleteStepActionAsync(id);
                return Ok(new { Message = $"StepAction with ID {id} deleted successfully." });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { Message = ex.Message });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode((int)HttpStatusCode.InternalServerError, new { Message = ex.Message });
            }
        }
    }
}
