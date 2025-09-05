using InternalService.Application.Courses.Common;
using InternalService.Application.Courses.Dtos;

namespace InternalService.Application.Courses.Interfaces;

public interface ICoursesService
{
    Task<PagedResult<CourseDto>> GetCoursesAsync(CourseQueryParameters parameters);
    Task<CourseDto?> GetCourseByIdAsync(int id);
    Task<CourseDto> CreateCourseAsync(CreateCourseDto createCourseDto);
    Task<CourseDto?> UpdateCourseAsync(int id, UpdateCourseDto updateCourseDto);
    Task<bool> DeleteCourseAsync(int id);
}
