using Lssctc.LearningManagement.Components.Dtos;
using Lssctc.LearningManagement.Components.Services;
using Lssctc.Share.Common;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Lssctc.LearningManagement.Components.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ComponentsController : ControllerBase
    {
        private readonly IComponentService _componentService;

        public ComponentsController(IComponentService componentService)
        {
            _componentService = componentService;
        }

        /// <summary>
        /// Get all simulation components with filtering, sorting, and pagination
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<PagedResult<SimulationComponentDto>>> GetAll(
            [FromQuery] SimulationComponentQueryDto query)
        {
            try
            {
                var result = await _componentService.GetAllAsync(query);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving components.", detail = ex.Message });
            }
        }

        /// <summary>
        /// Get a simulation component by ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<SimulationComponentDto>> GetById(int id)
        {
            try
            {
                if (id <= 0)
                    return BadRequest(new { message = "Invalid component ID." });

                var component = await _componentService.GetByIdAsync(id);

                if (component == null)
                    return NotFound(new { message = $"Component with ID {id} not found." });

                return Ok(component);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving the component.", detail = ex.Message });
            }
        }

        /// <summary>
        /// Create a new simulation component
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<SimulationComponentDto>> Create(
            [FromBody] CreateSimulationComponentDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var component = await _componentService.CreateAsync(dto);

                return CreatedAtAction(
                    nameof(GetById),
                    new { id = component.Id },
                    component);
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while creating the component.", detail = ex.Message });
            }
        }

        /// <summary>
        /// Update an existing simulation component
        /// </summary>
        [HttpPut("{id}")]
        public async Task<ActionResult<SimulationComponentDto>> Update(
            int id,
            [FromBody] UpdateSimulationComponentDto dto)
        {
            try
            {
                if (id <= 0)
                    return BadRequest(new { message = "Invalid component ID." });

                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var component = await _componentService.UpdateAsync(id, dto);

                if (component == null)
                    return NotFound(new { message = $"Component with ID {id} not found." });

                return Ok(component);
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while updating the component.", detail = ex.Message });
            }
        }

        /// <summary>
        /// Delete a simulation component (soft delete)
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(int id)
        {
            try
            {
                if (id <= 0)
                    return BadRequest(new { message = "Invalid component ID." });

                var success = await _componentService.DeleteAsync(id);

                if (!success)
                    return NotFound(new { message = $"Component with ID {id} not found." });

                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while deleting the component.", detail = ex.Message });
            }
        }

        /// <summary>
        /// Check if a simulation component exists by ID
        /// </summary>
        [HttpHead("{id}")]
        public async Task<ActionResult> Exists(int id)
        {
            try
            {
                if (id <= 0)
                    return BadRequest();

                var exists = await _componentService.ExistsAsync(id);

                return exists ? Ok() : NotFound();
            }
            catch (Exception)
            {
                return StatusCode(500);
            }
        }
    }
}
