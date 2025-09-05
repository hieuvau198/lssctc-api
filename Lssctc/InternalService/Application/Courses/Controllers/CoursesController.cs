using InternalService.Application.Courses.Dtos;
using InternalService.Application.Courses.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace InternalService.Application.Courses.Controllers;

[Route("api/[controller]")]
[ApiController]
public class CoursesController : ControllerBase
{
    private readonly ICoursesService _coursesService;

    public CoursesController(ICoursesService coursesService)
    {
        _coursesService = coursesService;
    }

    [HttpGet]
    public async Task<IActionResult> GetCourses([FromQuery] CourseQueryParameters parameters)
    {
        var result = await _coursesService.GetCoursesAsync(parameters);
        return Ok(result);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetCourse(int id)
    {
        var course = await _coursesService.GetCourseByIdAsync(id);
        if (course == null)
            return NotFound();
        return Ok(course);
    }

    [HttpPost]
    public async Task<IActionResult> CreateCourse([FromBody] CreateCourseDto dto)
    {
        var created = await _coursesService.CreateCourseAsync(dto);
        return CreatedAtAction(nameof(GetCourse), new { id = created.Id }, created);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> UpdateCourse(int id, [FromBody] UpdateCourseDto dto)
    {
        var updated = await _coursesService.UpdateCourseAsync(id, dto);
        if (updated == null)
            return NotFound();
        return Ok(updated);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteCourse(int id)
    {
        var deleted = await _coursesService.DeleteCourseAsync(id);
        if (!deleted)
            return NotFound();
        return NoContent();
    }
}
