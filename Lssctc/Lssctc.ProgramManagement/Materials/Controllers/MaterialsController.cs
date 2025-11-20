using Lssctc.ProgramManagement.Materials.Dtos;
using Lssctc.ProgramManagement.Materials.Services;
using Lssctc.Share.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Lssctc.ProgramManagement.Materials.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class MaterialsController : ControllerBase
    {
        private readonly IMaterialsService _materialsService;

        public MaterialsController(IMaterialsService materialsService)
        {
            _materialsService = materialsService;
        }

        #region Learning Materials

        [HttpGet]
        [Authorize(Roles = "Admin, Instructor")]
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
        [Authorize(Roles = "Admin, Instructor")]
        public async Task<ActionResult<PagedResult<MaterialDto>>> GetMaterials([FromQuery] int pageNumber, [FromQuery] int pageSize)
        {
            try
            {
                // Extract instructor ID from JWT claims if user is Instructor (not Admin)
                int? instructorId = null;
                if (User.IsInRole("Instructor"))
                {
                    instructorId = GetInstructorIdFromClaims();
                }

                var pagedResult = await _materialsService.GetMaterialsAsync(pageNumber, pageSize, instructorId);
                return Ok(pagedResult);
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { Message = "An unexpected error occurred." });
            }
        }

        [HttpGet("{id}")]
        [Authorize(Roles = "Admin, Instructor")]
        public async Task<ActionResult<MaterialDto>> GetMaterialById(int id)
        {
            try
            {
                // Extract instructor ID from JWT claims if user is Instructor (not Admin)
                int? instructorId = null;
                if (User.IsInRole("Instructor"))
                {
                    instructorId = GetInstructorIdFromClaims();
                }

                var material = await _materialsService.GetMaterialByIdAsync(id, instructorId);
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
        [Authorize(Roles = "Admin, Instructor")]
        public async Task<ActionResult<MaterialDto>> CreateMaterial([FromBody] CreateMaterialDto createDto)
        {
            if (createDto == null)
                return BadRequest(new { Message = "Invalid material data." });

            try
            {
                // Extract instructor ID from JWT claims
                var instructorId = GetInstructorIdFromClaims();

                var newMaterial = await _materialsService.CreateMaterialAsync(createDto, instructorId);
                return CreatedAtAction(nameof(GetMaterialById), new { id = newMaterial.Id }, newMaterial);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { Message = "An unexpected error occurred." });
            }
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin, Instructor")]
        public async Task<ActionResult<MaterialDto>> UpdateMaterial(int id, [FromBody] UpdateMaterialDto updateDto)
        {
            if (updateDto == null)
                return BadRequest(new { Message = "Invalid material data." });

            try
            {
                // Extract instructor ID from JWT claims if user is Instructor (not Admin)
                int? instructorId = null;
                if (User.IsInRole("Instructor"))
                {
                    instructorId = GetInstructorIdFromClaims();
                }

                var updated = await _materialsService.UpdateMaterialAsync(id, updateDto, instructorId);
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
        [Authorize(Roles = "Admin, Instructor")]
        public async Task<IActionResult> DeleteMaterial(int id)
        {
            try
            {
                // Extract instructor ID from JWT claims if user is Instructor (not Admin)
                int? instructorId = null;
                if (User.IsInRole("Instructor"))
                {
                    instructorId = GetInstructorIdFromClaims();
                }

                await _materialsService.DeleteMaterialAsync(id, instructorId);
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
        [Authorize(Roles = "Admin, Instructor")]
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
        [Authorize(Roles = "Admin, Instructor")]
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
        [AllowAnonymous]
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

        private int GetInstructorIdFromClaims()
        {
            // Try to get instructorId claim first, otherwise use NameIdentifier
            var instructorIdClaim = User.FindFirstValue("instructorId") ?? User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (int.TryParse(instructorIdClaim, out int instructorId))
            {
                return instructorId;
            }

            throw new UnauthorizedAccessException("Instructor ID claim is missing or invalid.");
        }
    }
}