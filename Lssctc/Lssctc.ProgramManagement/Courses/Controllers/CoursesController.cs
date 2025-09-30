using Lssctc.ProgramManagement.Courses.DTOs;
using Lssctc.ProgramManagement.Courses.Services;
using Lssctc.ProgramManagement.HttpCustomResponse;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Lssctc.ProgramManagement.Courses.Controllers
{
    
    [Route("api/[controller]")]
    [ApiController]
    public class CoursesController : ControllerBase
    {
        private readonly ICourseService _courseService;

        public CoursesController(ICourseService courseService)
        {
            _courseService = courseService;
        }

        [HttpGet("all")]
        public async Task<IActionResult> GetAllCourses()
        {
            try
            {
                var courses = await _courseService.GetAllCourses();
                return Ok(courses);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = ex.Message });
            }
        }

        /// <summary>
        /// Retrieves a list of courses with optional query parameters for filtering and pagination.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetCourses([FromQuery] CourseQueryParameters parameters)
        {
            try
            {
                var result = await _courseService.GetCourses(parameters);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = ex.Message });
            }
        }

        /// <summary>
        /// Retrieves a course by its unique identifier.
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetCourse(int id)
        {
            try
            {
                var course = await _courseService.GetCourseById(id);
                if (course == null)
                    return NotFound(new { message = "Course not found." });

                return Ok(course);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = ex.Message });
            }
        }

        /// <summary>
        /// Retrieves courses by category with pagination.
        /// </summary>
        [HttpGet("by-category/{categoryId}")]
        public async Task<IActionResult> GetCoursesByCategory(int categoryId, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            try
            {
                var result = await _courseService.GetCoursesByCategoryId(categoryId, pageNumber, pageSize);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = ex.Message });
            }
        }

        /// <summary>
        /// Retrieves courses by level with pagination.
        /// </summary>
        [HttpGet("by-level/{levelId}")]
        public async Task<IActionResult> GetCoursesByLevel(int levelId, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            try
            {
                var result = await _courseService.GetCoursesByLevelId(levelId, pageNumber, pageSize);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = ex.Message });
            }
        }

        /// <summary>
        /// Retrieves courses by optional category and/or level with pagination.
        /// </summary>
        [HttpGet("filter")]
        public async Task<IActionResult> GetCoursesByFilter(
            [FromQuery] int? categoryId,
            [FromQuery] int? levelId,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10)
        {
            try
            {
                var result = await _courseService.GetCoursesByFilter(categoryId, levelId, pageNumber, pageSize);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = ex.Message });
            }
        }

        /// <summary>
        /// Creates a new course.
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> CreateCourse([FromBody] CreateCourseDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var createdCourse = await _courseService.CreateCourse(dto);
                return CreatedAtAction(nameof(GetCourse), new { id = createdCourse.Id }, createdCourse);
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
        /// Create a new syllabus and attach it to a course.
        /// </summary>
        [HttpPost("course-syllabus")]
        public async Task<ActionResult<CourseSyllabusDto>> CreateCourseSyllabus([FromBody] CourseSyllabusCreateDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var result = await _courseService.CreateSyllabus(dto);
                return Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = ex.Message });
            }
        }

        /// <summary>
        /// Get all syllabuses of a given course.
        /// </summary>
        [HttpGet("{courseId}/syllabuses")]
        public async Task<ActionResult<IEnumerable<CourseSyllabusDto>>> GetCourseSyllabuses(int courseId)
        {
            try
            {
                var result = await _courseService.GetSyllabusByCourseId(courseId);
                if (result == null)
                    return NotFound(new { message = "No syllabuses found for this course." });

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = ex.Message });
            }
        }

        /// <summary>
        /// Update a course syllabus (updates syllabus details).
        /// </summary>
        [HttpPut("course-syllabus/{courseSyllabusId}")]
        public async Task<ActionResult<CourseSyllabusDto>> UpdateCourseSyllabus(int courseSyllabusId, [FromBody] UpdateCourseSyllabusDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var updated = await _courseService.UpdateSyllabusById(courseSyllabusId, dto);
                if (updated == null)
                    return NotFound(new { message = "Course syllabus not found." });

                return Ok(updated);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = ex.Message });
            }
        }

        /// <summary>
        /// Updates an existing course by its ID.
        /// </summary>
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateCourse(int id, [FromBody] UpdateCourseDto dto)
        {
            try
            {
                var updatedCourse = await _courseService.UpdateCourseById(id, dto);
                if (updatedCourse == null)
                    return NotFound(new { message = "Course not found." });

                return Ok(updatedCourse);
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
        /// Deletes a course by its ID.
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCourse(int id)
        {
            try
            {
                var deleted = await _courseService.DeleteCourseById(id);
                if (!deleted)
                    return NotFound(new { message = "Course not found." });

                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = ex.Message });
            }
        }

        /// <summary>
        /// Get all course categories
        /// </summary>
        [HttpGet("categories")]
        public async Task<IActionResult> GetCourseCategories()
        {
            try
            {
                var categories = await _courseService.GetAllCourseCategoriesAsync();
                if (categories == null || !categories.Any())
                    return NotFound(new { message = "No course categories found." });

                return Ok(categories);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = ex.Message });
            }
        }

        /// <summary>
        /// Get all course levels
        /// </summary>
        [HttpGet("levels")]
        public async Task<IActionResult> GetCourseLevels()
        {
            try
            {
                var levels = await _courseService.GetAllCourseLevelsAsync();
                if (levels == null || !levels.Any())
                    return NotFound(new { message = "No course levels found." });

                return Ok(levels);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = ex.Message });
            }
        }
    }
}
