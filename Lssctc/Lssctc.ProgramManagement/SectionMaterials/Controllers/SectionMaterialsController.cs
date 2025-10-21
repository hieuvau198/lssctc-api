using Lssctc.ProgramManagement.SectionMaterials.DTOs;
using Lssctc.ProgramManagement.SectionMaterials.Services;
using Lssctc.Share.Common;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace Lssctc.ProgramManagement.SectionMaterials.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SectionMaterialsController : ControllerBase
    {
        private readonly ISectionMaterialService _svc;

        public SectionMaterialsController(ISectionMaterialService svc)
        {
            _svc = svc;
        }

        // GET: /api/SectionMaterials?pageIndex=1&pageSize=20
        [HttpGet]
        public async Task<IActionResult> GetPaged(
            [FromQuery] int pageIndex = 1,
            [FromQuery] int pageSize = 20)
        {
            var paged = await _svc.GetSectionMaterialsPaged(pageIndex, pageSize);

            // Thêm header tổng số record
            Response.Headers["X-Total-Count"] = paged.TotalCount.ToString();
            Response.Headers["Access-Control-Expose-Headers"] = "X-Total-Count";

            return Ok(paged);
        }
        [HttpGet("all")]
        public async Task<IActionResult> GetSectionMaterialsNoPagination()
        {
            var items = await _svc.GetAllSectionMaterialsAsync();
            return Ok(items);
        }


        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetSectionMateriaById([FromRoute] int id)
        {
            var dto = await _svc.GetSectionMateriaById(id);
            return dto is null ? NotFound(new { error = $"SectionMaterial {id} not found." }) : Ok(dto);
        }

        [HttpPost]
        public async Task<IActionResult> CreateSectionMateria([FromBody] CreateSectionMaterialDto dto)
        {
            try
            {
                var id = await _svc.CreateSectionMateria(dto);
                return CreatedAtAction(nameof(GetSectionMateriaById), new { id }, new { id });
            }
            catch (ValidationException ex) { return BadRequest(new { error = ex.Message }); }
            catch (KeyNotFoundException ex) { return NotFound(new { error = ex.Message }); }
        }

        [HttpPost("upsert")]
        public async Task<IActionResult> UpsertSectionMaterial([FromBody] UpsertSectionMaterialDto dto)
        {
            try
            {
                var result = await _svc.UpsertSectionMaterial(dto);
                return Ok(result);
            }
            catch (ValidationException ex) { return BadRequest(new { error = ex.Message }); }
            catch (KeyNotFoundException ex) { return NotFound(new { error = ex.Message }); }
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> UpdateSectionMateria([FromRoute] int id, [FromBody] UpdateSectionMaterialDto dto)
        {
            try
            {
                var ok = await _svc.UpdateSectionMateria(id, dto);
                return ok ? NoContent() : NotFound(new { error = $"SectionMaterial {id} not found." });
            }
            catch (ValidationException ex) { return BadRequest(new { error = ex.Message }); }
            catch (KeyNotFoundException ex) { return NotFound(new { error = ex.Message }); }
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteSectionMateria([FromRoute] int id)
        {
            var ok = await _svc.DeleteSectionMateria(id);
            return ok ? NoContent() : NotFound(new { error = $"SectionMaterial {id} not found." });
        }
    }
}
