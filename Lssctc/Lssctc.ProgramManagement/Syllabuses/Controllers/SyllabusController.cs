using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using Lssctc.ProgramManagement.Syllabuses.Dtos;
using Lssctc.ProgramManagement.Syllabuses.Services;

namespace Lssctc.ProgramManagement.Syllabuses.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SyllabusController : ControllerBase
    {
        private readonly ISyllabusService _syllabusService;

        public SyllabusController(ISyllabusService syllabusService)
        {
            _syllabusService = syllabusService;
        }


        /// <summary>
        /// Get all syllabuses
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(List<SyllabusDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<List<SyllabusDto>>> GetAllSyllabuses()
        {
            var syllabuses = await _syllabusService.GetAllSyllabusesAsync();
            return Ok(syllabuses);
        }

        /// <summary>
        /// Get syllabus by ID
        /// </summary>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(SyllabusDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<SyllabusDto>> GetSyllabusById(int id)
        {
            var syllabus = await _syllabusService.GetSyllabusByIdAsync(id);

            if (syllabus == null)
                return NotFound(new { message = $"Syllabus with ID {id} not found" });

            return Ok(syllabus);
        }

        /// <summary>
        /// Create a new syllabus
        /// </summary>
        [HttpPost]
        [ProducesResponseType(typeof(SyllabusDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<SyllabusDto>> CreateSyllabus([FromBody] CreateSyllabusDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var syllabus = await _syllabusService.CreateSyllabusAsync(dto);
            return CreatedAtAction(nameof(GetSyllabusById), new { id = syllabus.Id }, syllabus);
        }

        /// <summary>
        /// Update an existing syllabus
        /// </summary>
        [HttpPut("{id}")]
        [ProducesResponseType(typeof(SyllabusDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<SyllabusDto>> UpdateSyllabus(int id, [FromBody] UpdateSyllabusDto dto)
        {
            var syllabus = await _syllabusService.UpdateSyllabusAsync(id, dto);

            if (syllabus == null)
                return NotFound(new { message = $"Syllabus with ID {id} not found" });

            return Ok(syllabus);
        }

        /// <summary>
        /// Delete a syllabus (soft delete)
        /// </summary>
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult> DeleteSyllabus(int id)
        {
            var result = await _syllabusService.DeleteSyllabusAsync(id);

            if (!result)
                return NotFound(new { message = $"Syllabus with ID {id} not found" });

            return NoContent();
        }

        // ==================== Syllabus Section Endpoints ====================

        /// <summary>
        /// Get all sections for a specific syllabus
        /// </summary>
        [HttpGet("{syllabusId}/sections")]
        [ProducesResponseType(typeof(List<SyllabusSectionDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<List<SyllabusSectionDto>>> GetSyllabusSections(int syllabusId)
        {
            var sections = await _syllabusService.GetSyllabusSectionsBySyllabusIdAsync(syllabusId);
            return Ok(sections);
        }

        /// <summary>
        /// Get section by ID
        /// </summary>
        [HttpGet("sections/{id}")]
        [ProducesResponseType(typeof(SyllabusSectionDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<SyllabusSectionDto>> GetSyllabusSectionById(int id)
        {
            var section = await _syllabusService.GetSyllabusSectionByIdAsync(id);

            if (section == null)
                return NotFound(new { message = $"Syllabus section with ID {id} not found" });

            return Ok(section);
        }

        /// <summary>
        /// Create a new syllabus section
        /// </summary>
        [HttpPost("sections")]
        [ProducesResponseType(typeof(SyllabusSectionDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<SyllabusSectionDto>> CreateSyllabusSection([FromBody] CreateSyllabusSectionDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var section = await _syllabusService.CreateSyllabusSectionAsync(dto);
            return CreatedAtAction(nameof(GetSyllabusSectionById), new { id = section.Id }, section);
        }

        /// <summary>
        /// Update an existing syllabus section
        /// </summary>
        [HttpPut("sections/{id}")]
        [ProducesResponseType(typeof(SyllabusSectionDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<SyllabusSectionDto>> UpdateSyllabusSection(int id, [FromBody] UpdateSyllabusSectionDto dto)
        {
            var section = await _syllabusService.UpdateSyllabusSectionAsync(id, dto);

            if (section == null)
                return NotFound(new { message = $"Syllabus section with ID {id} not found" });

            return Ok(section);
        }

        /// <summary>
        /// Delete a syllabus section (soft delete)
        /// </summary>
        [HttpDelete("sections/{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult> DeleteSyllabusSection(int id)
        {
            var result = await _syllabusService.DeleteSyllabusSectionAsync(id);

            if (!result)
                return NotFound(new { message = $"Syllabus section with ID {id} not found" });

            return NoContent();
        }
    }
}