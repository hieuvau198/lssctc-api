using Lssctc.ProgramManagement.Courses.DTOs;
using Lssctc.Share.Common;

namespace Lssctc.ProgramManagement.Courses.Services
{
    public interface ICourseService
    {
        Task<PagedResult<CourseDto>> GetAllCoursesAsync(CourseQueryParameters parameters);
        Task<CourseDto?> GetCourseByIdAsync(int id);
        Task<PagedResult<CourseDto>> GetCoursesByLevelAsync(int levelId, int pageNumber, int pageSize);
        Task<PagedResult<CourseDto>> GetCoursesByCategoryAsync(int categoryId, int pageNumber, int pageSize);
        Task<CourseDto> CreateCourseAsync(CreateCourseDto dto);
        Task<CourseDto?> UpdateCourseAsync(int id,UpdateCourseDto dto);
        Task<bool> DeleteCourseAsync(int id);
    }
}
