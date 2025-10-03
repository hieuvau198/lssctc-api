using Lssctc.ProgramManagement.SectionQuizzes.DTOs;
using Lssctc.ProgramManagement.SectionQuizzes.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace Lssctc.ProgramManagement.SectionQuizzes.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SectionQuizzesController : ControllerBase
    {
        private readonly ISectionQuizService _svc;
        public SectionQuizzesController(ISectionQuizService svc) { _svc = svc; }

        //  GET /api/SectionQuizzes (paged, no filter)
        [HttpGet]
        public async Task<IActionResult> GetAllPaged([FromQuery] int pageIndex = 1, [FromQuery] int pageSize = 20)
        {
            var result = await _svc.GetSectionQuizzesPagination(pageIndex, pageSize);
            return Ok(result);
        }

        // GET /api/SectionQuizzes/all (non paged, no filter)
        [HttpGet("all")]
        public async Task<IActionResult> GetAll()
        {
            var items = await _svc.GetSectionQuizzesNoPagination();
            return Ok(items);
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById([FromRoute] int id)
        {
            var dto = await _svc.GetSectionQuizById(id);
            if (dto is null)
            {
                return NotFound(new
                {
                    success = false,
                    statusCode = 404,
                    message = $"SectionQuiz {id} not found.",
                    data = (object?)null
                });
            }

            return Ok(new
            {
                success = true,
                statusCode = 200,
                message = "Get section quiz successfully.",
                data = dto
            });
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateSectionQuizDto dto)
        {
            try
            {
                var id = await _svc.CreateSectionQuiz(dto);
                return StatusCode(201, new
                {
                    success = true,
                    statusCode = 201,
                    message = "Create section quiz successfully.",
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

        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update([FromRoute] int id, [FromBody] UpdateSectionQuizDto dto)
        {
            try
            {
                var ok = await _svc.UpdateSectionQuiz(id, dto);
                if (!ok)
                    return NotFound(new { success = false, statusCode = 404, message = $"SectionQuiz {id} not found." });

                return Ok(new { success = true, statusCode = 200, message = "Update section quiz successfully." });
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

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete([FromRoute] int id)
        {
            try
            {
                var ok = await _svc.DeleteSectionQuiz(id);
                if (!ok)
                    return NotFound(new { success = false, statusCode = 404, message = $"SectionQuiz {id} not found." });

                return Ok(new { success = true, statusCode = 200, message = "Delete section quiz successfully." });
            }
            catch (InvalidOperationException ex)
            {
                // vd: còn attempts
                return Conflict(new { success = false, statusCode = 409, message = ex.Message });
            }
        }


    }
}
