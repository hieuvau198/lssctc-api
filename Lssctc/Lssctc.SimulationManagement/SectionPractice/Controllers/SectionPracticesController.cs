using Lssctc.SimulationManagement.SectionPractice.Dtos;
using Lssctc.SimulationManagement.SectionPractice.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

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


        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateSectionPracticeDto dto)
        {
            try
            {
                var id = await _svc.CreateAsync(dto);
                return StatusCode(201, new ApiResponse<object>
                {
                    Success = true,
                    StatusCode = 201,
                    Message = "Create section practice successfully.",
                    Data = new { id },
                    Pagination = null
                });
            }
            catch (ValidationException ex)
            {
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    StatusCode = 400,
                    Message = $"Invalid input. {ex.Message}",
                    Data = null,
                    Pagination = null
                });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new ApiResponse<object>
                {
                    Success = false,
                    StatusCode = 404,
                    Message = ex.Message,   // nhận message chi tiết từ service
                    Data = null,
                    Pagination = null
                });
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new ApiResponse<object>
                {
                    Success = false,
                    StatusCode = 409,
                    Message = $"Conflict. {ex.Message}",
                    Data = null,
                    Pagination = null
                });
            }
        }


        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update([FromRoute] int id, [FromBody] UpdateSectionPracticeDto dto)
        {
            try
            {
                var ok = await _svc.UpdateAsync(id, dto);
                if (!ok)
                    return NotFound(new ApiResponse<object> { Success = false, StatusCode = 404, Message = "Not found." });

                return Ok(new ApiResponse<object> { Success = true, StatusCode = 200, Message = "Section Practice has been updated." });
            }
            catch (ValidationException)
            {
                return BadRequest(new ApiResponse<object> { Success = false, StatusCode = 400, Message = "Invalid input." });
            }
            catch (KeyNotFoundException)
            {
                return NotFound(new ApiResponse<object> { Success = false, StatusCode = 404, Message = "Not found." });
            }
            catch (InvalidOperationException)
            {
                return Conflict(new ApiResponse<object> { Success = false, StatusCode = 409, Message = "Conflict." });
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
                        Message = $"SectionPractice with id={id} not found.",
                        Data = null,
                        Pagination = null
                    });
                }

                return Ok(new ApiResponse<object>
                {
                    Success = true,
                    StatusCode = 200,
                    Message = $"Delete section practice with id={id} successfully.",
                    Data = null,
                    Pagination = null
                });
            }
            catch (InvalidOperationException ex)
            {
                // trường hợp đang có Timeslot hoặc Attempt liên kết
                return Conflict(new ApiResponse<object>
                {
                    Success = false,
                    StatusCode = 409,
                    Message = $"Cannot delete SectionPractice with id={id}: {ex.Message}",
                    Data = null,
                    Pagination = null
                });
            }
            catch (ValidationException ex)
            {
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    StatusCode = 400,
                    Message = $"Invalid input. {ex.Message}",
                    Data = null,
                    Pagination = null
                });
            }
        }


    }
}
