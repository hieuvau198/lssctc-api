using Lssctc.ProgramManagement.Sections.Dtos;
using Lssctc.ProgramManagement.Sections.Services;
using Lssctc.Share.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Lssctc.ProgramManagement.Sections.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SectionsController : ControllerBase
    {
        private readonly ISectionsService _sectionsService;

        public SectionsController(ISectionsService sectionsService)
        {
            _sectionsService = sectionsService;
        }

        #region Sections

        [HttpGet]
        public async Task<ActionResult<IEnumerable<SectionDto>>> GetAllSections()
        {
            var sections = await _sectionsService.GetAllSectionsAsync();
            return Ok(sections);
        }

        [HttpGet("paged")]
        public async Task<ActionResult<PagedResult<SectionDto>>> GetSections([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            var pagedSections = await _sectionsService.GetSectionsAsync(pageNumber, pageSize);
            return Ok(pagedSections);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<SectionDto>> GetSectionById(int id)
        {
            var section = await _sectionsService.GetSectionByIdAsync(id);
            if (section == null) return NotFound(new { Message = "Section not found." });
            return Ok(section);
        }

        [HttpPost]
        [Authorize(Roles = "Admin, Instructor")]
        public async Task<ActionResult<SectionDto>> CreateSection([FromBody] CreateSectionDto createDto)
        {
            try
            {
                var section = await _sectionsService.CreateSectionAsync(createDto);
                return CreatedAtAction(nameof(GetSectionById), new { id = section.Id }, section);
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { Message = "An unexpected error occurred." });
            }
        }

        [HttpPost("course/{courseId}")]
        [Authorize(Roles = "Admin, Instructor")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        public async Task<ActionResult<SectionDto>> CreateSectionForCourse(int courseId, [FromBody] CreateSectionDto createDto)
        {
            try
            {
                var section = await _sectionsService.CreateSectionForCourseAsync(courseId, createDto);
                return CreatedAtAction(nameof(GetSectionById), new { id = section.Id }, section);
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

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin, Instructor")]
        public async Task<ActionResult<SectionDto>> UpdateSection(int id, [FromBody] UpdateSectionDto updateDto)
        {
            try
            {
                var updatedSection = await _sectionsService.UpdateSectionAsync(id, updateDto);
                return Ok(updatedSection);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { Message = ex.Message });
            }
            catch (InvalidOperationException ex) // Added for lock check
            {
                return BadRequest(new { Message = ex.Message });
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { Message = "An unexpected error occurred." });
            }
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin, Instructor")]
        public async Task<IActionResult> DeleteSection(int id)
        {
            try
            {
                await _sectionsService.DeleteSectionAsync(id);
                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { Message = ex.Message });
            }
            catch (InvalidOperationException ex) // Catches "Cannot delete section..."
            {
                return BadRequest(new { Message = ex.Message }); // 400 Bad Request
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { Message = "An unexpected error occurred." });
            }
        }

        #endregion

        #region Course-Section

        [HttpGet("course/{courseId}")]
        public async Task<ActionResult<IEnumerable<SectionDto>>> GetSectionsByCourseId(int courseId)
        {
            var sections = await _sectionsService.GetSectionsByCourseIdAsync(courseId);
            return Ok(sections);
        }

        [HttpPost("course/{courseId}/section/{sectionId}")]
        [Authorize(Roles = "Admin, Instructor")]
        public async Task<IActionResult> AddSectionToCourse(int courseId, int sectionId)
        {
            try
            {
                await _sectionsService.AddSectionToCourseAsync(courseId, sectionId);
                return Ok(new { Message = "Section successfully added to course." }); // Added success message
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { Message = ex.Message }); // Course or Section not found
            }
            catch (InvalidOperationException ex) // Catches "already added" or "course locked"
            {
                return BadRequest(new { Message = ex.Message }); // 400 Bad Request
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { Message = "An unexpected error occurred." });
            }
        }

        [HttpDelete("course/{courseId}/section/{sectionId}")]
        [Authorize(Roles = "Admin, Instructor")]
        public async Task<IActionResult> RemoveSectionFromCourse(int courseId, int sectionId)
        {
            try
            {
                await _sectionsService.RemoveSectionFromCourseAsync(courseId, sectionId);
                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { Message = ex.Message });
            }
            catch (InvalidOperationException ex) // Added for lock check
            {
                return BadRequest(new { Message = ex.Message });
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { Message = "An unexpected error occurred." });
            }
        }

        [HttpPut("course/{courseId}/section/{sectionId}/order/{newOrder}")]
        [Authorize(Roles = "Admin, Instructor")]
        public async Task<IActionResult> UpdateCourseSectionOrder(int courseId, int sectionId, int newOrder)
        {
            try
            {
                await _sectionsService.UpdateCourseSectionOrderAsync(courseId, sectionId, newOrder);
                return Ok(new { Message = "Section order updated successfully." }); // Added success message
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { Message = ex.Message });
            }
            catch (ArgumentException ex) // Catches "Section order must be greater than 0."
            {
                return BadRequest(new { Message = ex.Message }); // 400 Bad Request
            }
            catch (InvalidOperationException ex) // Added for lock check
            {
                return BadRequest(new { Message = ex.Message });
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { Message = "An unexpected error occurred." });
            }
        }

        #endregion

        #region Import Sections
        [HttpPost("course/{courseId}/import")]
        [Authorize(Roles = "Admin, Instructor")]
        public async Task<ActionResult<IEnumerable<SectionDto>>> ImportSections(int courseId, IFormFile file)
        {
            // Basic file validation
            if (file == null || file.Length == 0)
                return BadRequest(new { Message = "No file uploaded." });

            var extension = Path.GetExtension(file.FileName).ToLower();
            if (extension != ".xlsx" && extension != ".xls")
                return BadRequest(new { Message = "Invalid file format. Please upload an Excel file (.xlsx or .xls)." });

            try
            {
                var createdSections = await _sectionsService.ImportSectionsFromExcelAsync(courseId, file);
                return Ok(createdSections);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { Message = ex.Message });
            }
            catch (InvalidOperationException ex) // For locked courses
            {
                return BadRequest(new { Message = ex.Message });
            }
            catch (ArgumentException ex) // For empty/invalid files
            {
                return BadRequest(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                // Log the error details in a real app
                return StatusCode(StatusCodes.Status500InternalServerError, new { Message = "An error occurred while processing the file: " + ex.Message });
            }
        }

        [HttpPost("course/{courseId}/import-full")]
        [Authorize(Roles = "Admin, Instructor")]
        public async Task<ActionResult<IEnumerable<SectionDto>>> ImportSectionsWithActivities(int courseId, IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest(new { Message = "No file uploaded." });

            var extension = Path.GetExtension(file.FileName).ToLower();
            if (extension != ".xlsx" && extension != ".xls")
                return BadRequest(new { Message = "Invalid file format. Please upload an Excel file (.xlsx or .xls)." });

            try
            {
                // Call the new service method
                var createdSections = await _sectionsService.ImportSectionsWithActivitiesFromExcelAsync(courseId, file);
                return Ok(createdSections);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { Message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { Message = "Error processing file: " + ex.Message });
            }
        }

        #endregion

    }
}