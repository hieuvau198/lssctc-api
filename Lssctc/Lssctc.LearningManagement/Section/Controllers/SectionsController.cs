using Lssctc.LearningManagement.Section.DTOs;
using Lssctc.LearningManagement.Section.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace Lssctc.LearningManagement.Section.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SectionsController : ControllerBase
    {
        private readonly ISectionService _service;

        public SectionsController(ISectionService service)
        {
            _service = service;
        }

        // GET: /api/sections?pageIndex=1&pageSize=20&classesId=1&syllabusSectionId=2&status=1&search=crane
        [HttpGet]
        public async Task<IActionResult> GetSections(
            [FromQuery] int pageIndex = 1,
            [FromQuery] int pageSize = 20,
            [FromQuery] int? classesId = null,
            [FromQuery] int? syllabusSectionId = null,
            [FromQuery] int? status = null,
            [FromQuery] string? search = null)
        {
            var (items, total) = await _service.GetSections(pageIndex, pageSize, classesId, syllabusSectionId, status, search);

            Response.Headers["X-Total-Count"] = total.ToString();
            Response.Headers["Access-Control-Expose-Headers"] = "X-Total-Count";

            return Ok(new { pageIndex, pageSize, total, items });
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById([FromRoute] int id)
        {
            var dto = await _service.GetById(id);
            if (dto is null)
                return NotFound(new { success = false, message = "Section not found." });

            return Ok(new
            {
                success = true,
                message = "Lấy chi tiết section thành công.",
                data = dto
            });
        }


        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateSectionDto dto)
        {
            try
            {
                var id = await _service.Create(dto);
                return CreatedAtAction(nameof(GetById), new { id }, new
                {
                    success = true,
                    message = "Section created successfully.",
                    data = new { id }
                });
            }
            catch (ValidationException ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update([FromRoute] int id, [FromBody] UpdateSectionDto dto)
        {
            try
            {
                var ok = await _service.Update(id, dto);
                if (!ok) return NotFound(new { success = false, message = "Section not found." });

                return Ok(new
                {
                    success = true,
                    message = "Section update successful."
                });
            }
            catch (ValidationException ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete([FromRoute] int id)
        {
            try
            {
                var ok = await _service.Delete(id);
                if (!ok) return NotFound(new { success = false, message = "Section not found." });

                return Ok(new
                {
                    success = true,
                    message = "Section deleted successfully."
                });
            }
            catch (ValidationException ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }
    }
}
