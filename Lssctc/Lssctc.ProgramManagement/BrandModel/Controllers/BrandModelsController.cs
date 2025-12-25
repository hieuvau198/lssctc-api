using Lssctc.ProgramManagement.BrandModel.DTOs;
using Lssctc.ProgramManagement.BrandModel.Services;
using Lssctc.Share.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace Lssctc.ProgramManagement.BrandModel.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class BrandModelsController : ControllerBase
    {
        private readonly IBrandModelService _brandModelService;

        public BrandModelsController(IBrandModelService brandModelService)
        {
            _brandModelService = brandModelService;
        }

        [HttpGet]
        public async Task<ActionResult<PagedResult<BrandModelDto>>> GetAllBrandModels(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var result = await _brandModelService.GetAllBrandModels(page, pageSize, cancellationToken);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<BrandModelDto>> GetBrandModelById(
            int id,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var result = await _brandModelService.GetBrandModelById(id, cancellationToken);
                if (result == null)
                    return NotFound("Brand model not found.");

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }

        [HttpPost]
        [Authorize(Roles = "Admin, SimulationManager")]
        public async Task<ActionResult<int>> CreateBrandModel(
            [FromBody] CreateBrandModelDto dto,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var id = await _brandModelService.CreateBrandModel(dto, cancellationToken);
                return CreatedAtAction(nameof(GetBrandModelById), new { id }, new { id });
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
        [Authorize(Roles = "Admin, SimulationManager")]
        public async Task<IActionResult> UpdateBrandModel(
            int id,
            [FromBody] UpdateBrandModelDto dto,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var result = await _brandModelService.UpdateBrandModel(id, dto, cancellationToken);
                if (!result)
                    return NotFound("Brand model not found.");

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
        [Authorize(Roles = "Admin, SimulationManager")]
        public async Task<IActionResult> DeleteBrandModel(
            int id,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var result = await _brandModelService.DeleteBrandModel(id, cancellationToken);
                if (!result)
                    return NotFound("Brand model not found.");

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
    }
}
