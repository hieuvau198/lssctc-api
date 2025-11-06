using Lssctc.ProgramManagement.BrandModel.DTOs;
using Lssctc.ProgramManagement.BrandModel.Services;
using Lssctc.Share.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

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
        /// Get all SimulationComponents by BrandModel ID with pagination
        /// </summary>
        /// <param name="brandModelId">The BrandModel ID</param>
        /// <param name="page">Page number (default: 1)</param>
        /// <param name="pageSize">Page size (default: 10)</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Paginated list of SimulationComponents</returns>
        [HttpGet("{brandModelId}/simulation-components")]
       
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<PagedResult<SimulationComponentDto>>> GetSimulationComponentsByBrandModelId(
            int brandModelId,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var result = await _brandModelService.GetSimulationComponentsByBrandModelIdAsync(
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
    }
}
