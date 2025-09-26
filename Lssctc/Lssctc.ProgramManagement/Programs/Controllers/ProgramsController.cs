using Lssctc.ProgramManagement.HttpCustomResponse;
using Lssctc.ProgramManagement.Programs.DTOs;
using Lssctc.ProgramManagement.Programs.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Lssctc.ProgramManagement.Programs.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProgramsController : ControllerBase
    {
        private readonly IProgramService _programService;

        public ProgramsController(IProgramService programService)
        {
            _programService = programService;
        }

        /// <summary>
        /// Retrieves all programs with filtering and pagination.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetPrograms([FromQuery] ProgramQueryParameters parameters)
        {
            try
            {
                var result = await _programService.GetAllProgramsAsync(parameters);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = ex.Message });
            }
        }

        /// <summary>
        /// Retrieves a program by ID.
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetProgram(int id)
        {
            try
            {
                var program = await _programService.GetProgramByIdAsync(id);
                if (program == null)
                    return NotFound(new { message = "Program not found." });

                return Ok(program);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = ex.Message });
            }
        }

        /// <summary>
        /// Creates a new empty program (no courses inside yet).
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> CreateProgram([FromBody] CreateProgramDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var createdProgram = await _programService.CreateProgramAsync(dto);
                return CreatedAtAction(nameof(GetProgram), new { id = createdProgram.Id }, createdProgram);
            }
            catch (BadRequestException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = ex.Message });
            }
        }

        /// <summary>
        /// Adds courses into an existing program with order.
        /// </summary>
        [HttpPost("{programId}/courses")]
        public async Task<IActionResult> AddCoursesToProgram(int programId, [FromBody] List<CourseOrderDto> courses)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var updatedProgram = await _programService.AddCoursesToProgramAsync(programId, courses);
                if (updatedProgram == null)
                    return NotFound(new { message = "Program not found." });

                return Ok(updatedProgram);
            }
            catch (BadRequestException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = ex.Message });
            }
        }

        /// <summary>
        /// Adds entry requirements to an existing program.
        /// </summary>
        [HttpPost("{programId}/entry-requirements")]
        public async Task<IActionResult> AddPrerequisitesToProgram(
            int programId,
            [FromBody] List<EntryRequirementDto> prerequisites)
        {
            if (prerequisites == null || prerequisites.Count == 0)
                return BadRequest(new { message = "At least one prerequisite must be provided." });

            try
            {
                var result = await _programService.AddPrerequisitesToProgramAsync(programId, prerequisites);
                if (result == null)
                    return NotFound(new { message = $"Program with ID {programId} not found or deleted." });

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = ex.Message });
            }
        }

        // ✅ UPDATE basic info
        [HttpPut("{id}/basic")]
        public async Task<IActionResult> UpdateProgramBasic(int id, [FromBody] UpdateProgramInfoDto dto)
        {
            try
            {
                var result = await _programService.UpdateProgramBasicAsync(id, dto);
                if (result == null)
                    return NotFound(new { message = "Program not found." });

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        // ✅ UPDATE program courses
        [HttpPut("{id}/courses")]
        public async Task<IActionResult> UpdateProgramCourses(int id, [FromBody] ICollection<ProgramCourseOrderDto> courses)
        {
            try
            {
                var result = await _programService.UpdateProgramCoursesAsync(id, courses);
                if (result == null)
                    return NotFound(new { message = "Program not found." });

                return Ok(result);
            }
            catch (BadRequestException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        // ✅ UPDATE entry requirements
        [HttpPut("{id}/entry-requirements")]
        public async Task<IActionResult> UpdateProgramEntryRequirements(int id, [FromBody] ICollection<UpdateEntryRequirementDto> entryRequirements)
        {
            try
            {
                var result = await _programService.UpdateProgramEntryRequirementsAsync(id, entryRequirements);
                if (result == null)
                    return NotFound(new { message = "Program not found." });

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        /// <summary>
        /// Soft deletes a program.
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProgram(int id)
        {
            try
            {
                var deleted = await _programService.DeleteProgramAsync(id);
                if (!deleted)
                    return NotFound(new { message = "Program not found." });

                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = ex.Message });
            }
        }
    }
}
