using Lssctc.LearningManagement.SectionPartition.DTOs;
using Lssctc.LearningManagement.SectionPartition.Services;
using Lssctc.Share.Common;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace Lssctc.LearningManagement.SectionPartition.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SectionPartitionsController : ControllerBase
    {
        private readonly ISectionPartitionService _svc;

        public SectionPartitionsController(ISectionPartitionService svc)
        {
            _svc = svc;
        }

        // GET: /api/SectionPartitions?pageIndex=1&pageSize=20&sectionId=1&partitionTypeId=2&search=abc
        [HttpGet]
        public async Task<IActionResult> GetPaged(
     [FromQuery] int pageIndex = 1,
     [FromQuery] int pageSize = 20,
     [FromQuery] int? sectionId = null,
     [FromQuery] int? partitionTypeId = null,
     [FromQuery] string? search = null)
        {
            var page = await _svc.GetPagedAsync(pageIndex, pageSize, sectionId, partitionTypeId, search);

            // (tuỳ chọn) expose tổng record cho FE
            Response.Headers["X-Total-Count"] = page.TotalCount.ToString();
            Response.Headers["Access-Control-Expose-Headers"] = "X-Total-Count";

            var resp = new ApiResponse<PagedResult<SectionPartitionDto>>
            {
                Success = true,
                StatusCode = 200,
                Message = "Get section partitions successfully.",
                Data = page,
               
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
                    Message = $"SectionPartition {id} not found."
                });
            }

            return Ok(new ApiResponse<SectionPartitionDto>
            {
                Success = true,
                StatusCode = 200,
                Message = "Get section partition successfully.",
                Data = dto
            });
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateSectionPartitionDto dto)
        {
            try
            {
                var id = await _svc.CreateAsync(dto);
                var resp = new ApiResponse<object>
                {
                    Success = true,
                    StatusCode = 201,
                    Message = "Create section partition successfully.",
                    Data = new { id }
                };
                return StatusCode(201, resp);
            }
            catch (ValidationException ex)
            {
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    StatusCode = 400,
                    Message = ex.Message
                });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new ApiResponse<object>
                {
                    Success = false,
                    StatusCode = 404,
                    Message = ex.Message
                });
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new ApiResponse<object>
                {
                    Success = false,
                    StatusCode = 409,
                    Message = ex.Message
                });
            }
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update([FromRoute] int id, [FromBody] UpdateSectionPartitionDto dto)
        {
            try
            {
                var ok = await _svc.UpdateAsync(id, dto);
                if (!ok)
                {
                    return NotFound(new ApiResponse<object>
                    {
                        Success = false,
                        StatusCode = 404,
                        Message = $"SectionPartition {id} not found."
                    });
                }

                return Ok(new ApiResponse<object>
                {
                    Success = true,
                    StatusCode = 200,
                    Message = "Update section partition successfully."
                });
            }
            catch (ValidationException ex)
            {
                return BadRequest(new ApiResponse<object> { Success = false, StatusCode = 400, Message = ex.Message });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new ApiResponse<object> { Success = false, StatusCode = 404, Message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new ApiResponse<object> { Success = false, StatusCode = 409, Message = ex.Message });
            }
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete([FromRoute] int id)
        {
            try
            {
                var ok = await _svc.DeleteAsync(id);
                if (!ok)
                {
                    return NotFound(new ApiResponse<object>
                    {
                        Success = false,
                        StatusCode = 404,
                        Message = $"SectionPartition {id} not found."
                    });
                }

                return Ok(new ApiResponse<object>
                {
                    Success = true,
                    StatusCode = 200,
                    Message = "Delete section partition successfully."
                });
            }
            catch (ValidationException ex)
            {
                return BadRequest(new ApiResponse<object> { Success = false, StatusCode = 400, Message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new ApiResponse<object> { Success = false, StatusCode = 409, Message = ex.Message });
            }
        }
    }
}
