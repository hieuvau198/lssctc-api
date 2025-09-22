using Lssctc.ProgramManagement.Courses.DTOs;
using Lssctc.ProgramManagement.Courses.Services;
using Lssctc.ProgramManagement.HttpCustomResponse;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace Lssctc.ProgramManagement.Courses.Controllers
{
    /// <summary>
    /// API controller for managing courses, including retrieving,
    /// creating, updating, and deleting course records.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class CoursesController : ControllerBase
    {
        private readonly ICourseService _courseService;


        public CoursesController(ICourseService courseService)
        {
            _courseService = courseService;
        }

        /// <summary>
        /// Retrieves a list of courses with optional query parameters for filtering and pagination.
        /// </summary>
        /// <param name="parameters">The query parameters for filtering and pagination.</param>
        /// <returns>A list of courses.</returns>
        // GET: api/courses
        [HttpGet]
        public async Task<IActionResult> GetCourses([FromQuery] CourseQueryParameters parameters)
        {
            var result = await _courseService.GetCourses(parameters);
            return Ok(result);
        }

        /// <summary>
        /// Retrieves a course by its unique identifier.
        /// </summary>
        /// <param name="id">The unique identifier of the course.</param>
        /// <returns>The course details if found; otherwise, 404.</returns>
        // GET: api/courses/5
        [HttpGet("{id}")]
        public async Task<IActionResult> GetCourse(int id)
        {
            var course = await _courseService.GetCourseById(id);
            if (course == null)
                return NotFound();

            return Ok(course);
        }
        /// <summary>
        /// Retrieves courses by category with pagination.
        /// </summary>
        [HttpGet("by-category/{categoryId}")]
        public async Task<IActionResult> GetCoursesByCategory(int categoryId, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            var result = await _courseService.GetCoursesByCategoryId(categoryId, pageNumber, pageSize);
            return Ok(result);
        }

        /// <summary>
        /// Retrieves courses by level with pagination.
        /// </summary>
        [HttpGet("by-level/{levelId}")]
        public async Task<IActionResult> GetCoursesByLevel(int levelId, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            var result = await _courseService.GetCoursesByLevelId(levelId, pageNumber, pageSize);
            return Ok(result);
        }
        /// <summary>
        /// Retrieves courses by optional category and/or level with pagination.
        /// </summary>
        /// <param name="categoryId">Optional category filter.</param>
        /// <param name="levelId">Optional level filter.</param>
        /// <param name="pageNumber">Page number (default: 1).</param>
        /// <param name="pageSize">Page size (default: 10).</param>
        /// <returns>Paged list of filtered courses.</returns>
        [HttpGet("filter")]
        public async Task<IActionResult> GetCoursesByFilter(
            [FromQuery] int? categoryId,
            [FromQuery] int? levelId,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10)
        {
            var result = await _courseService.GetCoursesByFilter(categoryId, levelId, pageNumber, pageSize);
            return Ok(result);
        }

        /// <summary>
        /// Creates a new course.
        /// </summary>
        /// <param name="dto">The data transfer object containing course details.</param>
        /// <returns>The newly created course.</returns>
        // POST: api/courses
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
        }
        /// <summary>
        /// Get all syllabuses of a given course.
        /// </summary>
        [HttpGet("{courseId}/syllabuses")]
        public async Task<ActionResult<IEnumerable<CourseSyllabusDto>>> GetCourseSyllabuses(int courseId)
        {
            var result = await _courseService.GetSyllabusByCourseId(courseId);

            if (result == null )
                return NotFound(new { message = "No syllabuses found for this course." });

            return Ok(result);
        }

        /// <summary>
        /// Update a course syllabus (updates syllabus details).
        /// </summary>
        [HttpPut("course-syllabus/{courseSyllabusId}")]
        public async Task<ActionResult<CourseSyllabusDto>> UpdateCourseSyllabus(int courseSyllabusId, [FromBody] UpdateCourseSyllabusDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var updated = await _courseService.UpdateSyllabusById(courseSyllabusId, dto);

            if (updated == null)
                return NotFound(new { message = "Course syllabus not found." });

            return Ok(updated);
        }
        /// <summary>
        /// Updates an existing course by its ID.
        /// </summary>
        /// <param name="id">The unique identifier of the course.</param>
        /// <param name="dto">The data transfer object containing updated course details.</param>
        /// <returns>The updated course if successful; otherwise, 404 or 400.</returns>
        // PUT: api/courses/5
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateCourse(int id, [FromBody] UpdateCourseDto dto)
        {

            try
            {
                var updatedCourse = await _courseService.UpdateCourseById(id, dto);
                if (updatedCourse == null)
                    return NotFound();

                return Ok(updatedCourse);
            }
            catch (BadRequestException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Deletes a course by its ID.
        /// </summary>
        /// <param name="id">The unique identifier of the course to delete.</param>
        /// <returns>No content if deletion was successful; otherwise, 404.</returns>
        // DELETE: api/courses/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCourse(int id)
        {
            var deleted = await _courseService.DeleteCourseById(id);
            if (!deleted)
                return NotFound();

            return NoContent();
        }
    }
}
