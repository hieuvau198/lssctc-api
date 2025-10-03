using Lssctc.ProgramManagement.Classes.DTOs;
using Lssctc.ProgramManagement.TrainingResults.Services;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace Lssctc.ProgramManagement.TrainingResults.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TrainingResultsController : ControllerBase
    {
        private readonly ITrainingResultService _svc;

        public TrainingResultsController(ITrainingResultService svc)
        {
            _svc = svc;
        }

        // GET /api/TrainingResults (paged, no filter)
        [HttpGet]
        public async Task<IActionResult> GetPaged(
            [FromQuery] int pageIndex = 1,
            [FromQuery] int pageSize = 20)
        {
            var result = await _svc.GetTrainingResults(pageIndex, pageSize);
            return Ok(result);
        }

        // GET /api/TrainingResults/all (non-paged, no filter)
        [HttpGet("all")]
        public async Task<IActionResult> GetAll()
        {
            var items = await _svc.GetTrainingResultsNoPagination();
            return Ok(items);
        }

        // GET /api/TrainingResults/{id}
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById([FromRoute] int id)
        {
            var dto = await _svc.GetTrainingResultById(id);
            if (dto is null)
            {
                return NotFound(new
                {
                    success = false,
                    statusCode = 404,
                    message = $"TrainingResult {id} not found.",
                    data = (object?)null
                });
            }

            return Ok(new
            {
                success = true,
                statusCode = 200,
                message = "Get training result successfully.",
                data = dto
            });
        }

        // POST /api/TrainingResults
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateTrainingResultDto dto)
        {
            try
            {
                var id = await _svc.CreateTrainingResult(dto);
                return StatusCode(201, new
                {
                    success = true,
                    statusCode = 201,
                    message = "Create training result successfully.",
                    data = new { id }
                });
            }
            catch (ValidationException ex)
            {
                return BadRequest(new { success = false, statusCode = 400, message = ex.Message });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { success = false, statusCode = 404, message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { success = false, statusCode = 409, message = ex.Message });
            }
        }

        // PUT /api/TrainingResults/{id}
        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update([FromRoute] int id, [FromBody] UpdateTrainingResultDto dto)
        {
            try
            {
                var ok = await _svc.UpdateTrainingResult(id, dto);
                if (!ok)
                    return NotFound(new { success = false, statusCode = 404, message = $"TrainingResult {id} not found." });

                return Ok(new { success = true, statusCode = 200, message = "Update training result successfully." });
            }
            catch (ValidationException ex)
            {
                return BadRequest(new { success = false, statusCode = 400, message = ex.Message });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { success = false, statusCode = 404, message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { success = false, statusCode = 409, message = ex.Message });
            }
        }

        // DELETE /api/TrainingResults/{id}
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete([FromRoute] int id)
        {
            try
            {
                var ok = await _svc.DeleteTrainingResultById(id);
                if (!ok)
                    return NotFound(new { success = false, statusCode = 404, message = $"TrainingResult {id} not found." });

                return Ok(new { success = true, statusCode = 200, message = "Delete training result successfully." });
            }
            catch (InvalidOperationException ex)
            {
                // ví dụ: đang bị ràng buộc bởi bảng khác
                return Conflict(new { success = false, statusCode = 409, message = ex.Message });
            }
        }
    }
}
