using Lssctc.ProgramManagement.Courses.DTOs;
using Lssctc.Share.Common;

namespace Lssctc.ProgramManagement.Courses.Services
{
    public interface ICourseService
    {
        Task<PagedResult<CourseDto>> GetAllCoursesAsync(CourseQueryParameters parameters);
        Task<CourseDto?> GetCourseByIdAsync(int id);
        Task<CourseDto> CreateCourseAsync(CreateCourseDto dto);
        Task<CourseDto?> UpdateCourseAsync(int id,UpdateCourseDto dto);
        Task<bool> DeleteCourseAsync(int id);
    }
}
