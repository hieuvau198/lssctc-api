using Lssctc.ProgramManagement.Materials.Dtos;
using Lssctc.ProgramManagement.Materials.Services;
using Lssctc.Share.Common;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Lssctc.ProgramManagement.Materials.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MaterialsController : ControllerBase
    {
        private readonly IMaterialsService _materialsService;

        public MaterialsController(IMaterialsService materialsService)
        {
            _materialsService = materialsService;
        }

        #region Learning Materials

        [HttpGet]
        public async Task<IActionResult> GetAllMaterials()
        {
            try
            {
                var materials = await _materialsService.GetAllMaterialsAsync();
                return Ok(materials);
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { Message = "An unexpected error occurred." });
            }
        }

        [HttpGet("paged")]
        public async Task<ActionResult<PagedResult<MaterialDto>>> GetMaterials([FromQuery] int pageNumber, [FromQuery] int pageSize)
        {
            try
            {
                var pagedResult = await _materialsService.GetMaterialsAsync(pageNumber, pageSize);
                return Ok(pagedResult);
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { Message = "An unexpected error occurred." });
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<MaterialDto>> GetMaterialById(int id)
        {
            try
            {
                var material = await _materialsService.GetMaterialByIdAsync(id);
                if (material == null)
                    return NotFound(new { Message = "Material not found." });

                return Ok(material);
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { Message = "An unexpected error occurred." });
            }
        }

        [HttpPost]
        public async Task<ActionResult<MaterialDto>> CreateMaterial([FromBody] CreateMaterialDto createDto)
        {
            if (createDto == null)
                return BadRequest(new { Message = "Invalid material data." });

            try
            {
                var newMaterial = await _materialsService.CreateMaterialAsync(createDto);
                return CreatedAtAction(nameof(GetMaterialById), new { id = newMaterial.Id }, newMaterial);
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { Message = "An unexpected error occurred." });
            }
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<MaterialDto>> UpdateMaterial(int id, [FromBody] UpdateMaterialDto updateDto)
        {
            if (updateDto == null)
                return BadRequest(new { Message = "Invalid material data." });

            try
            {
                var updated = await _materialsService.UpdateMaterialAsync(id, updateDto);
                return Ok(updated);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { Message = ex.Message });
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { Message = "An unexpected error occurred." });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteMaterial(int id)
        {
            try
            {
                await _materialsService.DeleteMaterialAsync(id);
                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { Message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { Message = "An unexpected error occurred." });
            }
        }

        #endregion

        #region Activity Materials

        [HttpPost("activities/{activityId}/materials/{materialId}")]
        public async Task<IActionResult> AddMaterialToActivity(int activityId, int materialId)
        {
            try
            {
                await _materialsService.AddMaterialToActivityAsync(activityId, materialId);
                return Ok(new { Message = "Material successfully added to activity." });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { Message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { Message = "An unexpected error occurred." });
            }
        }

        [HttpDelete("activities/{activityId}/materials/{materialId}")]
        public async Task<IActionResult> RemoveMaterialFromActivity(int activityId, int materialId)
        {
            try
            {
                await _materialsService.RemoveMaterialFromActivityAsync(activityId, materialId);
                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { Message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { Message = "An unexpected error occurred." });
            }
        }

        [HttpGet("activities/{activityId}/materials")]
        public async Task<IActionResult> GetMaterialsByActivity(int activityId)
        {
            try
            {
                var result = await _materialsService.GetMaterialsByActivityAsync(activityId);
                return Ok(result);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { Message = ex.Message });
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { Message = "An unexpected error occurred." });
            }
        }

        #endregion
    }
}