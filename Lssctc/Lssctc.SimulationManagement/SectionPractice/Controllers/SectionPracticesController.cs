using Lssctc.Share.Common;
using Lssctc.SimulationManagement.SectionPractice.Dtos;
using Lssctc.SimulationManagement.SectionPractice.Services;
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

        // GET: /api/SectionPractices?pageIndex=1&pageSize=20
        [HttpGet]
        public async Task<IActionResult> GetSectionPracticesPaged(
            [FromQuery] int pageIndex = 1,
            [FromQuery] int pageSize = 20)
        {
            if (pageIndex < 1) return BadRequest(new { message = "pageIndex must be >= 1." });
            if (pageSize < 1 || pageSize > 200) return BadRequest(new { message = "pageSize must be between 1 and 200." });

            var page = await _svc.GetSectionPracticesPaged(pageIndex, pageSize);

            // expose total qua header cho FE
            Response.Headers["X-Total-Count"] = page.TotalCount.ToString();
            Response.Headers["Access-Control-Expose-Headers"] = "X-Total-Count";

            return Ok(page); 
        }

        // GET: /api/SectionPractices/5
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetSectionPracticeById([FromRoute] int id)
        {
            var dto = await _svc.GetSectionPracticeById(id);
            if (dto is null) return NotFound(new { message = $"SectionPractice {id} not found." });

            return Ok(dto); 
        }

        // POST: /api/SectionPractices
        [HttpPost]
        public async Task<IActionResult> CreateSectionPractice([FromBody] CreateSectionPracticeDto dto)
        {
            try
            {
                var id = await _svc.CreateSectionPractice(dto);
                // trả về địa chỉ GET-by-id theo chuẩn REST
                return CreatedAtAction(nameof(GetSectionPracticeById), new { id }, new { id });
            }
            catch (ValidationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { message = ex.Message });
            }
        }

        // PUT: /api/SectionPractices/5
        [HttpPut("{id:int}")]
        public async Task<IActionResult> UpdateSectionPractice([FromRoute] int id, [FromBody] UpdateSectionPracticeDto dto)
        {
            try
            {
                var ok = await _svc.UpdateSectionPractice(id, dto);
                if (!ok) return NotFound(new { message = $"SectionPractice {id} not found." });

                return Ok(new { message = "Section practice updated successfully." });
            }
            catch (ValidationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { message = ex.Message });
            }
        }

        // DELETE: /api/SectionPractices/5
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteSectionPractice([FromRoute] int id)
        {
            try
            {
                var ok = await _svc.DeleteSectionPractice(id);
                if (!ok) return NotFound(new { message = $"SectionPractice {id} not found." });

                return Ok(new { message = $"Delete section practice with id={id} successfully." });
            }
            catch (InvalidOperationException ex)
            {
                // ví dụ: đang có Timeslot/Attempt liên kết…
                return Conflict(new { message = $"Cannot delete SectionPractice {id}: {ex.Message}" });
            }
            catch (ValidationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}
