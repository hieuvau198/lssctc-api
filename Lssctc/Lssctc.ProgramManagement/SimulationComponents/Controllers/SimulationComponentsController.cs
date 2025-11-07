using Lssctc.ProgramManagement.SimulationComponents.DTOs;
using Lssctc.ProgramManagement.SimulationComponents.Services;
using Lssctc.Share.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace Lssctc.ProgramManagement.SimulationComponents.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SimulationComponentsController : ControllerBase
    {
        private readonly ISimulationComponentService _simulationComponentService;

        public SimulationComponentsController(ISimulationComponentService simulationComponentService)
        {
            _simulationComponentService = simulationComponentService;
        }

        [HttpGet]
        public async Task<ActionResult<PagedResult<SimulationComponentDto>>> GetAllSimulationComponents(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var result = await _simulationComponentService.GetAllSimulationComponents(page, pageSize, cancellationToken);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<SimulationComponentDto>> GetSimulationComponentById(
            int id,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var result = await _simulationComponentService.GetSimulationComponentById(id, cancellationToken);
                if (result == null)
                    return NotFound("Simulation component not found.");

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }

        [HttpGet("by-brand-model/{brandModelId}")]
        public async Task<ActionResult<PagedResult<SimulationComponentDto>>> GetSimulationComponentsByBrandModelId(
            int brandModelId,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var result = await _simulationComponentService.GetSimulationComponentsByBrandModelId(
                    brandModelId, 
                    page, 
                    pageSize, 
                    cancellationToken);

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }

        [HttpPost]
        [Authorize(Roles = "Admin,SimulationManager")]
        public async Task<ActionResult<int>> CreateSimulationComponent(
            [FromBody] CreateSimulationComponentDto dto,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var id = await _simulationComponentService.CreateSimulationComponent(dto, cancellationToken);
                return CreatedAtAction(nameof(GetSimulationComponentById), new { id }, new { id });
            }
            catch (ValidationException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin,SimulationManager")]
        public async Task<IActionResult> UpdateSimulationComponent(
            int id,
            [FromBody] UpdateSimulationComponentDto dto,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var result = await _simulationComponentService.UpdateSimulationComponent(id, dto, cancellationToken);
                if (!result)
                    return NotFound("Simulation component not found.");

                return NoContent();
            }
            catch (ValidationException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin,SimulationManager")]
        public async Task<IActionResult> DeleteSimulationComponent(
            int id,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var result = await _simulationComponentService.DeleteSimulationComponent(id, cancellationToken);
                if (!result)
                    return NotFound("Simulation component not found.");

                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }
    }
}
