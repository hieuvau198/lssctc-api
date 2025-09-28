using Lssctc.SimulationManagement.SectionPractice.Dtos;
using Lssctc.SimulationManagement.SectionPractice.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Lssctc.SimulationManagement.SectionPractice.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SectionPracticesController : ControllerBase
    {
        private readonly ISectionPracticeService _svc;

        public SectionPracticesController(ISectionPracticeService svc)
        {
            _svc = svc;
        }

        // GET: /api/SectionPractices?pageIndex=1&pageSize=20&sectionPartitionId=1&practiceId=2&status=1&search=abc
        [HttpGet]
        public async Task<IActionResult> GetPaged(
            [FromQuery] int pageIndex = 1,
            [FromQuery] int pageSize = 20,
            [FromQuery] int? sectionPartitionId = null,
            [FromQuery] int? practiceId = null,
            [FromQuery] int? status = null,
            [FromQuery] string? search = null)
        {
            var (items, total) = await _svc.GetPagedAsync(pageIndex, pageSize, sectionPartitionId, practiceId, status, search);

            var pagination = new Pagination
            {
                PageIndex = pageIndex,
                PageSize = pageSize,
                TotalItems = total,
                TotalPages = (int)Math.Ceiling(total / (double)pageSize)
            };

            var resp = new ApiResponse<IEnumerable<SectionPracticeDto>>
            {
                Success = true,
                StatusCode = 200,
                Message = "Get section practices successfully.",
                Data = items,
                Pagination = pagination
            };

            return Ok(resp);
        }


        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById([FromRoute] int id)
        {
            var dto = await _svc.GetByIdAsync(id);
            if (dto is null)
            {
                return NotFound(new ApiResponse<object>
                {
                    Success = false,
                    StatusCode = 404,
                    Message = $"SectionPractice {id} not found."
                });
            }

            return Ok(new ApiResponse<SectionPracticeDto>
            {
                Success = true,
                StatusCode = 200,
                Message = "Get section practice successfully.",
                Data = dto
            });
        }
    }
}
