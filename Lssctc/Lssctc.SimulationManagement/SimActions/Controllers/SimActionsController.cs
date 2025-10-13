using Lssctc.SimulationManagement.SimActions.Dtos;
using Lssctc.SimulationManagement.SimActions.Services;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace Lssctc.SimulationManagement.SimActions.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SimActionsController : ControllerBase
    {
        private readonly ISimActionService _simActionService;

        public SimActionsController(ISimActionService simActionService)
        {
            _simActionService = simActionService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllSimActions()
        {
            try
            {
                var actions = await _simActionService.GetAllSimActionsAsync();
                return Ok(actions);
            }
            catch (Exception ex)
            {
                return StatusCode((int)HttpStatusCode.InternalServerError, new
                {
                    Message = "An error occurred while retrieving SimActions.",
                    Details = ex.Message
                });
            }
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetSimActionById(int id)
        {
            try
            {
                var action = await _simActionService.GetSimActionByIdAsync(id);
                if (action == null)
                    return NotFound(new { Message = $"SimAction with ID {id} not found." });

                return Ok(action);
            }
            catch (Exception ex)
            {
                return StatusCode((int)HttpStatusCode.InternalServerError, new
                {
                    Message = $"An error occurred while retrieving SimAction with ID {id}.",
                    Details = ex.Message
                });
            }
        }

        [HttpPost]
        public async Task<IActionResult> CreateSimAction([FromBody] CreateSimActionDto dto)
        {
            try
            {
                if (dto == null)
                    return BadRequest(new { Message = "Invalid SimAction data." });

                var createdAction = await _simActionService.CreateSimActionAsync(dto);
                return CreatedAtAction(nameof(GetSimActionById), new { id = createdAction.ActionId }, createdAction);
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
                return StatusCode((int)HttpStatusCode.InternalServerError, new
                {
                    Message = "An error occurred while creating the SimAction.",
                    Details = ex.Message
                });
            }
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> UpdateSimAction(int id, [FromBody] UpdateSimActionDto dto)
        {
            try
            {
                if (dto == null)
                    return BadRequest(new { Message = "Invalid SimAction data." });

                var updatedAction = await _simActionService.UpdateSimActionAsync(id, dto);
                return Ok(updatedAction);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode((int)HttpStatusCode.InternalServerError, new
                {
                    Message = $"An error occurred while updating SimAction with ID {id}.",
                    Details = ex.Message
                });
            }
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteSimAction(int id)
        {
            try
            {
                var deleted = await _simActionService.DeleteSimActionAsync(id);
                if (!deleted)
                    return NotFound(new { Message = $"SimAction with ID {id} not found." });

                return Ok(new { Message = $"SimAction with ID {id} was deleted successfully." });
            }
            catch (Exception ex)
            {
                return StatusCode((int)HttpStatusCode.InternalServerError, new
                {
                    Message = $"An error occurred while deleting SimAction with ID {id}.",
                    Details = ex.Message
                });
            }
        }
    }
}
