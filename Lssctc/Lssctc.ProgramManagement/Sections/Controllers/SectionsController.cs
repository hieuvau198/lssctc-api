using Lssctc.ProgramManagement.Sections.Dtos;
using Lssctc.ProgramManagement.Sections.Services;
using Lssctc.Share.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http; // Required for StatusCodes
using Microsoft.AspNetCore.Mvc;

namespace Lssctc.ProgramManagement.Sections.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class SectionsController : ControllerBase
    {
        private readonly ISectionsService _sectionsService;

        public SectionsController(ISectionsService sectionsService)
        {
            _sectionsService = sectionsService;
        }

        #region Sections

        [HttpGet]
        [Authorize(Roles = "1,4")]
        public async Task<ActionResult<IEnumerable<SectionDto>>> GetAllSections()
        {
            var sections = await _sectionsService.GetAllSectionsAsync();
            return Ok(sections);
        }

        [HttpGet("paged")]
        [Authorize(Roles = "1,4")]
        public async Task<ActionResult<PagedResult<SectionDto>>> GetSections([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            var pagedSections = await _sectionsService.GetSectionsAsync(pageNumber, pageSize);
            return Ok(pagedSections);
        }

        [HttpGet("{id}")]
        [Authorize(Roles = "1,4")]
        public async Task<ActionResult<SectionDto>> GetSectionById(int id)
        {
            var section = await _sectionsService.GetSectionByIdAsync(id);
            if (section == null) return NotFound();
            return Ok(section);
        }

        [HttpPost]
        [Authorize(Roles = "1,4")]
        public async Task<ActionResult<SectionDto>> CreateSection([FromBody] CreateSectionDto createDto)
        {
            try
            {
                var section = await _sectionsService.CreateSectionAsync(createDto);
                return CreatedAtAction(nameof(GetSectionById), new { id = section.Id }, section);
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "An unexpected error occurred." });
            }
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "1,4")]
        public async Task<ActionResult<SectionDto>> UpdateSection(int id, [FromBody] UpdateSectionDto updateDto)
        {
            try
            {
                var updatedSection = await _sectionsService.UpdateSectionAsync(id, updateDto);
                return Ok(updatedSection);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "An unexpected error occurred." });
            }
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "1,4")]
        public async Task<IActionResult> DeleteSection(int id)
        {
            try
            {
                await _sectionsService.DeleteSectionAsync(id);
                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (InvalidOperationException ex) // Catches "Cannot delete section associated with courses."
            {
                return BadRequest(new { message = ex.Message }); // 400 Bad Request
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "An unexpected error occurred." });
            }
        }

        #endregion

        #region Course-Section

        [HttpGet("course/{courseId}")]
        [Authorize(Roles = "1,4")]
        public async Task<ActionResult<IEnumerable<SectionDto>>> GetSectionsByCourseId(int courseId)
        {
            var sections = await _sectionsService.GetSectionsByCourseIdAsync(courseId);
            return Ok(sections);
        }

        [HttpPost("course/{courseId}/section/{sectionId}")]
        [Authorize(Roles = "1,4")]
        public async Task<IActionResult> AddSectionToCourse(int courseId, int sectionId)
        {
            try
            {
                await _sectionsService.AddSectionToCourseAsync(courseId, sectionId);
                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message }); // Course or Section not found
            }
            catch (InvalidOperationException ex) // Catches "already added to course"
            {
                return Conflict(new { message = ex.Message }); // 409 Conflict
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "An unexpected error occurred." });
            }
        }

        [HttpDelete("course/{courseId}/section/{sectionId}")]
        [Authorize(Roles = "1,4")]
        public async Task<IActionResult> RemoveSectionFromCourse(int courseId, int sectionId)
        {
            try
            {
                await _sectionsService.RemoveSectionFromCourseAsync(courseId, sectionId);
                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "An unexpected error occurred." });
            }
        }

        [HttpPut("course/{courseId}/section/{sectionId}/order/{newOrder}")]
        [Authorize(Roles = "1,4")]
        public async Task<IActionResult> UpdateCourseSectionOrder(int courseId, int sectionId, int newOrder)
        {
            try
            {
                await _sectionsService.UpdateCourseSectionOrderAsync(courseId, sectionId, newOrder);
                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (ArgumentException ex) // Catches "Section order must be greater than 0."
            {
                return BadRequest(new { message = ex.Message }); // 400 Bad Request
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "An unexpected error occurred." });
            }
        }

        #endregion
    }
}