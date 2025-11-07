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
        private readonly IBrandModel _brandModelService;

        public BrandModelsController(IBrandModel brandModelService)
        {
            _brandModelService = brandModelService;
        }

        /// <summary>
        /// Get all BrandModels with pagination
        /// </summary>
        /// <param name="page">Page number (default: 1)</param>
        /// <param name="pageSize">Page size (default: 10)</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Paginated list of BrandModels</returns>
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

        /// <summary>
        /// Get BrandModel by ID
        /// </summary>
        /// <param name="id">BrandModel ID</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>BrandModel details</returns>
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

        /// <summary>
        /// Create a new BrandModel
        /// </summary>
        /// <param name="dto">BrandModel creation data</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Created BrandModel ID</returns>
        [HttpPost]
        [Authorize(Roles = "Admin")]
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

        /// <summary>
        /// Update an existing BrandModel
        /// </summary>
        /// <param name="id">BrandModel ID</param>
        /// <param name="dto">BrandModel update data</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Success status</returns>
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
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

        /// <summary>
        /// Delete a BrandModel (soft delete)
        /// </summary>
        /// <param name="id">BrandModel ID</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Success status</returns>
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
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
