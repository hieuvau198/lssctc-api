using Lssctc.ProgramManagement.HttpCustomResponse;
using Lssctc.ProgramManagement.Programs.DTOs;
using Lssctc.ProgramManagement.Programs.Services;
using Microsoft.AspNetCore.Authorization;
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
            var result = await _programService.GetAllProgramsAsync(parameters);
            return Ok(result);
        }

        /// <summary>
        /// Retrieves a program by ID.
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetProgram(int id)
        {
            var program = await _programService.GetProgramByIdAsync(id);
            if (program == null)
                return NotFound();

            return Ok(program);
        }

        /// <summary>
        /// Creates a new empty program (no courses inside yet).
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> CreateProgram([FromBody] CreateProgramDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var createdProgram = await _programService.CreateProgramAsync(dto);
            return CreatedAtAction(nameof(GetProgram), new { id = createdProgram.Id }, createdProgram);
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
                    return NotFound();

                return Ok(updatedProgram);
            }
            catch (BadRequestException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
        /// <summary>
        /// adds entry requirements to an existing program.
        /// </summary>
        [HttpPost("{programId}/entry-requirements")]
        public async Task<IActionResult> AddPrerequisitesToProgram(
            int programId,
            [FromBody] List<EntryRequirementDto> prerequisites)
        {
            if (prerequisites == null || prerequisites.Count == 0)
                return BadRequest("At least one prerequisite must be provided.");

            var result = await _programService.AddPrerequisitesToProgramAsync(programId, prerequisites);

            if (result == null)
                return NotFound($"Program with ID {programId} not found or deleted.");

            return Ok(result);
        }
        /// <summary>
        /// Updates a program (can replace courses entirely).
        /// </summary>
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateProgram(int id, [FromBody] UpdateProgramDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var updatedProgram = await _programService.UpdateProgramAsync(id, dto);
                if (updatedProgram == null)
                    return NotFound();

                return Ok(updatedProgram);
            }
            catch (BadRequestException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Soft deletes a program.
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProgram(int id)
        {
            var deleted = await _programService.DeleteProgramAsync(id);
            if (!deleted)
                return NotFound();

            return NoContent();
        }
    }
}
