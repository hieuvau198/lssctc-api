using Lssctc.ProgramManagement.Courses.Dtos;
using Lssctc.Share.Common;

namespace Lssctc.ProgramManagement.Courses.Services
{
    public interface ICoursesService
    {
        #region Courses
        Task<IEnumerable<CourseDto>> GetAllCoursesAsync();
        Task<PagedResult<CourseDto>> GetCoursesAsync(int pageNumber, int pageSize);
        Task<CourseDto?> GetCourseByIdAsync(int id);
        Task<CourseDto> CreateCourseAsync(CreateCourseDto createDto);
        Task<CourseDto> UpdateCourseAsync(int id, UpdateCourseDto updateDto);
        Task DeleteCourseAsync(int id);
        #endregion

        #region Program Courses

        Task<IEnumerable<CourseDto>> GetCoursesByProgramIdAsync(int programId);
        #endregion

    }
}
