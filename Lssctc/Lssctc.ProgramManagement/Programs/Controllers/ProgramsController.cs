using Lssctc.ProgramManagement.Programs.Dtos;
using Lssctc.ProgramManagement.Programs.Services;
using Lssctc.Share.Common;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Lssctc.ProgramManagement.Programs.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProgramsController : ControllerBase
    {
        private readonly IProgramsService _programsService;
        private readonly IProgramCoursesService _programCoursesService;
        public ProgramsController(IProgramsService programsService, IProgramCoursesService programCoursesService)
        {
            _programsService = programsService;
            _programCoursesService = programCoursesService;
        }
        #region Programs

        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<ProgramDto>), 200)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> GetAllPrograms()
        {
            try
            {
                var programs = await _programsService.GetAllProgramsAsync();
                return Ok(programs);
            }
            catch (Exception ex)
            {
                // In a real app, you would log this exception
                return StatusCode(500, $"An internal server error occurred: {ex.Message}");
            }
        }

        [HttpGet("paged")]
        [ProducesResponseType(typeof(PagedResult<ProgramDto>), 200)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> GetPrograms([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            try
            {
                var pagedResult = await _programsService.GetProgramsAsync(pageNumber, pageSize);
                return Ok(pagedResult);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An internal server error occurred: {ex.Message}");
            }
        }

        [HttpGet("{id}")]
        [ProducesResponseType(typeof(ProgramDto), 200)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> GetProgramById(int id)
        {
            try
            {
                var program = await _programsService.GetProgramByIdAsync(id);
                if (program == null)
                {
                    return NotFound();
                }
                return Ok(program);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An internal server error occurred: {ex.Message}");
            }
        }

        [HttpPost]
        [ProducesResponseType(typeof(ProgramDto), 201)]
        [ProducesResponseType(400)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> CreateProgram([FromBody] CreateProgramDto createDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var createdProgram = await _programsService.CreateProgramAsync(createDto);
                return Ok(createdProgram);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An internal server error occurred: {ex.Message}");
            }
        }

        [HttpPut("{id}")]
        [ProducesResponseType(204)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> UpdateProgram(int id, [FromBody] UpdateProgramDto updateDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var updatedProgram = await _programsService.UpdateProgramAsync(id, updateDto);
                return Ok(updatedProgram);
            }
            catch (Exception ex)
            {
                // Check for the specific "not found" exception from the service
                if (ex.Message.Contains("not found"))
                {
                    return NotFound(ex.Message);
                }
                return StatusCode(500, $"An internal server error occurred: {ex.Message}");
            }
        }

        [HttpDelete("{id}")]
        [ProducesResponseType(204)]
        [ProducesResponseType(404)]
        [ProducesResponseType(400)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> DeleteProgram(int id)
        {
            try
            {
                await _programsService.DeleteProgramAsync(id);
                return NoContent();
            }
            catch (Exception ex)
            {
                // Handle specific exceptions from the service layer
                if (ex.Message.Contains("not found"))
                {
                    return NotFound(ex.Message);
                }
                if (ex.Message.Contains("Cannot delete program"))
                {
                    return BadRequest(ex.Message);
                }
                return StatusCode(500, $"An internal server error occurred: {ex.Message}");
            }
        }

        #endregion

        #region Program Courses

        [HttpPost("{programId}/courses/{courseId}")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> AddCourseToProgram(int programId, int courseId)
        {
            try
            {
                await _programCoursesService.AddCourseToProgramAsync(programId, courseId);
                return Ok();
            }
            catch (Exception ex)
            {
                if (ex is ArgumentException)
                    return BadRequest(ex.Message);
                if (ex is KeyNotFoundException)
                    return NotFound(ex.Message);
                if (ex is InvalidOperationException)
                    return BadRequest(ex.Message);
                return StatusCode(500, $"An internal server error occurred: {ex.Message}");
            }
        }

        [HttpDelete("{programId}/courses/{courseId}")]
        [ProducesResponseType(204)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> RemoveCourseFromProgram(int programId, int courseId)
        {
            try
            {
                await _programCoursesService.RemoveCourseFromProgramAsync(programId, courseId);
                return NoContent();
            }
            catch (Exception ex)
            {
                if (ex is ArgumentException)
                    return BadRequest(ex.Message);
                if (ex is KeyNotFoundException)
                    return NotFound(ex.Message);
                return StatusCode(500, $"An internal server error occurred: {ex.Message}");
            }
        }

        [HttpPut("{programId}/courses/{courseId}/order/{newOrder}")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> UpdateProgramCourseOrder(int programId, int courseId, int newOrder)
        {
            try
            {
                await _programCoursesService.UpdateProgramCourseAsync(programId, courseId, newOrder);
                return Ok();
            }
            catch (Exception ex)
            {
                if (ex is ArgumentException)
                    return BadRequest(ex.Message);
                if (ex is KeyNotFoundException)
                    return NotFound(ex.Message);
                return StatusCode(500, $"An internal server error occurred: {ex.Message}");
            }
        }

        #endregion
    }
}
