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
        public async Task<IActionResult> GetCourses(
            [FromQuery] int pageNumber = 1, 
            [FromQuery] int pageSize = 10,
            [FromQuery] string? searchTerm = null,
            [FromQuery] string? sortBy = null,
            [FromQuery] string? sortDirection = null)
        {
            try
            {
                var pagedResult = await _coursesService.GetCoursesAsync(pageNumber, pageSize, searchTerm, sortBy, sortDirection);
                return Ok(pagedResult);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An internal server error occurred: {ex.Message}");
            }
        }

        [HttpGet("available")]
        [ProducesResponseType(typeof(IEnumerable<CourseDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAvailableCourses()
        {
            try
            {
                var courses = await _coursesService.GetAvailableCoursesAsync();
                return Ok(courses);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An internal server error occurred: {ex.Message}");
            }
        }

        [HttpGet("{id}")]
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

        [HttpGet("program/{programId}/available")]
        [ProducesResponseType(typeof(IEnumerable<CourseDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAvailableCoursesByProgramId(int programId)
        {
            try
            {
                var courses = await _coursesService.GetAvailableCoursesByProgramIdAsync(programId);

                if (courses == null || !courses.Any())
                {
                    return NotFound($"No available courses found for program ID {programId}.");
                }
                return Ok(courses);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An internal server error occurred: {ex.Message}");
            }
        }

        #endregion

        #region Class Courses

        /// <summary>
        /// Gets the course associated with a specific class ID.
        /// </summary>
        /// <param name="classId">The ID of the class.</param>
        /// <returns>The course details.</returns>
        [HttpGet("class/{classId}")]
        [ProducesResponseType(typeof(CourseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetCourseByClassId(int classId)
        {
            try
            {
                var course = await _coursesService.GetCourseByClassIdAsync(classId);
                if (course == null)
                {
                    return NotFound(new { message = $"No course found for class ID {classId}." });
                }
                return Ok(course);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An internal server error occurred: {ex.Message}");
            }
        }

        #endregion

        #region Course Categories and Levels

        [HttpGet("categories")]
        public async Task<IActionResult> GetAllCourseCategories()
        {
            try
            {
                var categories = await _coursesService.GetAllCourseCategoriesAsync();
                return Ok(categories);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An internal server error occurred: {ex.Message}");
            }
        }

        [HttpPost("categories")]
        public async Task<IActionResult> CreateCourseCategory([FromBody] CreateCourseCategoryDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var createdCategory = await _coursesService.CreateCourseCategoryAsync(dto);
                return Ok(createdCategory);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An internal server error occurred: {ex.Message}");
            }
        }

        [HttpPut("categories/{id}")]
        public async Task<IActionResult> UpdateCourseCategory(int id, [FromBody] UpdateCourseCategoryDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var updatedCategory = await _coursesService.UpdateCourseCategoryAsync(id, dto);
                return Ok(updatedCategory);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An internal server error occurred: {ex.Message}");
            }
        }

        [HttpGet("levels")]
        public async Task<IActionResult> GetAllCourseLevels()
        {
            try
            {
                var levels = await _coursesService.GetAllCourseLevelsAsync();
                return Ok(levels);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An internal server error occurred: {ex.Message}");
            }
        }

        [HttpPost("levels")]
        public async Task<IActionResult> CreateCourseLevel([FromBody] CreateCourseLevelDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var createdLevel = await _coursesService.CreateCourseLevelAsync(dto);
                return Ok(createdLevel);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An internal server error occurred: {ex.Message}");
            }
        }

        [HttpPut("levels/{id}")]
        public async Task<IActionResult> UpdateCourseLevel(int id, [FromBody] UpdateCourseLevelDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var updatedLevel = await _coursesService.UpdateCourseLevelAsync(id, dto);
                return Ok(updatedLevel);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An internal server error occurred: {ex.Message}");
            }
        }

        #endregion
    }
}
