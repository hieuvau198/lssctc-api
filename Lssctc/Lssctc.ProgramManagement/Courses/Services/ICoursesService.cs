using Lssctc.ProgramManagement.Courses.Dtos;
using Lssctc.Share.Common;

namespace Lssctc.ProgramManagement.Courses.Services
{
    public interface ICoursesService
    {
        #region Courses
        Task<IEnumerable<CourseDto>> GetAllCoursesAsync();
        Task<PagedResult<CourseDto>> GetCoursesAsync(int pageNumber, int pageSize, string? searchTerm = null, string? sortBy = null, string? sortDirection = null);
        Task<CourseDto?> GetCourseByIdAsync(int id);
        Task<CourseDto> CreateCourseAsync(CreateCourseDto createDto);
        Task<CourseDto> UpdateCourseAsync(int id, UpdateCourseDto updateDto);
        Task DeleteCourseAsync(int id);
        #endregion

        #region Program Courses
        Task<IEnumerable<CourseDto>> GetCoursesByProgramIdAsync(int programId);
        #endregion

        #region Class Courses
        Task<CourseDto?> GetCourseByClassIdAsync(int classId);
        #endregion

        #region Course Categories and Levels
        Task<IEnumerable<CourseCategoryDto>> GetAllCourseCategoriesAsync();
        Task<CourseCategoryDto> CreateCourseCategoryAsync(CreateCourseCategoryDto dto);
        Task<CourseCategoryDto> UpdateCourseCategoryAsync(int id, UpdateCourseCategoryDto dto);
        Task<IEnumerable<CourseLevelDto>> GetAllCourseLevelsAsync();
        Task<CourseLevelDto> CreateCourseLevelAsync(CreateCourseLevelDto dto);
        Task<CourseLevelDto> UpdateCourseLevelAsync(int id, UpdateCourseLevelDto dto);
        #endregion

    }
}
