using Lssctc.ProgramManagement.Courses.Dtos;
using Lssctc.ProgramManagement.Courses.Services;
using Lssctc.Share.Common;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Lssctc.ProgramManagement.Courses.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CoursesController : ControllerBase
    {
        private readonly ICoursesService _coursesService;
        public CoursesController(ICoursesService coursesService)
        {
            _coursesService = coursesService;
        }

        #region Courses

        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<CourseDto>), 200)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> GetAllCourses()
        {
            try
            {
                var courses = await _coursesService.GetAllCoursesAsync();
                return Ok(courses);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An internal server error occurred: {ex.Message}");
            }
        }

        [HttpGet("paged")]
        [ProducesResponseType(typeof(PagedResult<CourseDto>), 200)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> GetCourses([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            try
            {
                var pagedResult = await _coursesService.GetCoursesAsync(pageNumber, pageSize);
                return Ok(pagedResult);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An internal server error occurred: {ex.Message}");
            }
        }

        [HttpGet("{id}")]
        [ProducesResponseType(typeof(CourseDto), 200)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> GetCourseById(int id)
        {
            try
            {
                var course = await _coursesService.GetCourseByIdAsync(id);
                if (course == null)
                {
                    return NotFound();
                }
                return Ok(course);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An internal server error occurred: {ex.Message}");
            }
        }

        [HttpPost]
        [ProducesResponseType(typeof(CourseDto), 201)]
        [ProducesResponseType(400)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> CreateCourse([FromBody] CreateCourseDto createDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var createdCourse = await _coursesService.CreateCourseAsync(createDto);
                return Ok(createdCourse);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An internal server error occurred: {ex.Message}");
            }
        }

        [HttpPut("{id}")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> UpdateCourse(int id, [FromBody] UpdateCourseDto updateDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var updatedCourse = await _coursesService.UpdateCourseAsync(id, updateDto);
                return Ok(updatedCourse);
            }
            catch (Exception ex)
            {
                if (ex is KeyNotFoundException)
                    return NotFound(ex.Message);
                return StatusCode(500, $"An internal server error occurred: {ex.Message}");
            }
        }

        [HttpDelete("{id}")]
        [ProducesResponseType(204)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> DeleteCourse(int id)
        {
            try
            {
                await _coursesService.DeleteCourseAsync(id);
                return NoContent();
            }
            catch (Exception ex)
            {
                if (ex is KeyNotFoundException)
                    return NotFound(ex.Message);
                if (ex is InvalidOperationException)
                    return BadRequest(ex.Message);
                return StatusCode(500, $"An internal server error occurred: {ex.Message}");
            }
        }
        #endregion

        #region Program Courses
        [HttpGet("program/{programId}")]
        [ProducesResponseType(typeof(IEnumerable<CourseDto>), 200)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> GetCoursesByProgramId(int programId)
        {
            try
            {
                var courses = await _coursesService.GetCoursesByProgramIdAsync(programId);
                if (courses == null || !courses.Any())
                {
                    return NotFound($"No courses found for program ID {programId}.");
                }
                return Ok(courses);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An internal server error occurred: {ex.Message}");
            }
        }

        #endregion
    }
}
