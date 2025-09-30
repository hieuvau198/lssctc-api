using Lssctc.LearningManagement.SectionMaterial.DTOs;
using Lssctc.LearningManagement.SectionMaterial.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace Lssctc.LearningManagement.SectionMaterial.Controllers
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

        [HttpGet]
        public async Task<IActionResult> GetSectionMaterias(
            [FromQuery] int pageIndex = 1,
            [FromQuery] int pageSize = 20,
            [FromQuery] int? sectionPartitionId = null)
        {
            var (items, total) = await _svc.GetSectionMaterialsPaged(pageIndex, pageSize);
            return Ok(new { pageIndex, pageSize, total, items });
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
