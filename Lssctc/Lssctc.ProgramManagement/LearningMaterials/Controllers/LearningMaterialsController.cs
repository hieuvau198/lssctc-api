using Lssctc.ProgramManagement.LearningMaterials.DTOs;
using Lssctc.ProgramManagement.LearningMaterials.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace Lssctc.ProgramManagement.LearningMaterials.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LearningMaterialsController : ControllerBase
    {
        private readonly ILearningMaterialService _svc;

        public LearningMaterialsController(ILearningMaterialService svc)
        {
            _svc = svc;
        }

        // GET: /api/LearningMaterials/all
        [HttpGet("all")]
        public async Task<IActionResult> GetAllNoPagination()
        {
            var items = await _svc.GetLearningMaterialsNoPagination();
            return Ok(items);
        }

        // GET: /api/LearningMaterials?pageIndex=1&pageSize=20
        [HttpGet]
        public async Task<IActionResult> GetAllPagination(
            [FromQuery] int pageIndex = 1,
            [FromQuery] int pageSize = 20)
        {
            var result = await _svc.GetLearningMaterialsPagination(pageIndex, pageSize);
            return Ok(result);
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetLearningMaterialById([FromRoute] int id)
        {
            var dto = await _svc.GetLearningMaterialById(id);
            return dto is null ? NotFound() : Ok(dto);
        }

        [HttpPost]
        public async Task<IActionResult> CreateLearningMaterial([FromBody] CreateLearningMaterialDto dto)
        {
            try
            {
                var id = await _svc.CreateLearningMaterial(dto);
                return CreatedAtAction(nameof(GetLearningMaterialById), new { id }, new { id });
            }
            catch (ValidationException ex) { return BadRequest(new { success = false, statusCode = 400, message = ex.Message }); }
            catch (KeyNotFoundException ex) { return NotFound(new { success = false, statusCode = 404, message = ex.Message }); }
            catch (InvalidOperationException ex) { return Conflict(new { success = false, statusCode = 409, message = ex.Message }); }
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> UpdateLearningMaterial([FromRoute] int id, [FromBody] UpdateLearningMaterialDto dto)
        {
            try
            {
                var ok = await _svc.UpdateLearningMaterial(id, dto);
                return ok ? NoContent() : NotFound();
            }
            catch (ValidationException ex) { return BadRequest(new { error = ex.Message }); }
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete([FromRoute] int id)
        {
            var ok = await _svc.DeleteLearningMaterial(id);
            return ok ? NoContent() : NotFound();
        }
    }
}
