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
    [Authorize]
    public class SimulationComponentsController : ControllerBase
    {
        private readonly ISimulationComponentService _simulationComponentService;

        public SimulationComponentsController(ISimulationComponentService simulationComponentService)
        {
            _simulationComponentService = simulationComponentService;
        }

        /// <summary>
        /// Get all SimulationComponents with pagination
        /// </summary>
        /// <param name="page">Page number (default: 1)</param>
        /// <param name="pageSize">Page size (default: 10)</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Paginated list of SimulationComponents</returns>
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

        /// <summary>
        /// Get SimulationComponent by ID
        /// </summary>
        /// <param name="id">SimulationComponent ID</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>SimulationComponent details</returns>
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

        /// <summary>
        /// Get all SimulationComponents by BrandModel ID with pagination
        /// </summary>
        /// <param name="brandModelId">The BrandModel ID</param>
        /// <param name="page">Page number (default: 1)</param>
        /// <param name="pageSize">Page size (default: 10)</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Paginated list of SimulationComponents</returns>
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

        /// <summary>
        /// Create a new SimulationComponent
        /// </summary>
        /// <param name="dto">SimulationComponent creation data</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Created SimulationComponent ID</returns>
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

        /// <summary>
        /// Update an existing SimulationComponent
        /// </summary>
        /// <param name="id">SimulationComponent ID</param>
        /// <param name="dto">SimulationComponent update data</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Success status</returns>
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

        /// <summary>
        /// Delete a SimulationComponent (soft delete)
        /// </summary>
        /// <param name="id">SimulationComponent ID</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Success status</returns>
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
