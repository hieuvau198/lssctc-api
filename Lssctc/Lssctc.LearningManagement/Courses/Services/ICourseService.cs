using Lssctc.LearningManagement.Courses.DTOs;
using Lssctc.Share.Common;

namespace Lssctc.LearningManagement.Courses.Services
{
    public interface ICourseService
    {
        // Course - get info
        Task<List<CourseDto>> GetAllCourses();
        Task<PagedResult<CourseDto>> GetCourses(CourseQueryParameters parameters);
        Task<CourseDto?> GetCourseById(int id);
        Task<PagedResult<CourseDto>> GetCoursesByLevelId(int levelId, int pageNumber, int pageSize);
        Task<PagedResult<CourseDto>> GetCoursesByCategoryId(int categoryId, int pageNumber, int pageSize);
        Task<PagedResult<CourseDto>> GetCoursesByFilter(int? categoryId, int? levelId, int pageNumber, int pageSize);
        Task<IEnumerable<CourseLevelDto>> GetAllCourseLevelsAsync();
        Task<IEnumerable<CourseCategoryDto>> GetAllCourseCategoriesAsync();
        // course - manage
        Task<CourseDto> CreateCourse(CreateCourseDto dto);
        Task<CourseDto?> UpdateCourseById(int id,UpdateCourseDto dto);
        Task<bool> DeleteCourseById(int id);
        // syllabus
        Task<CourseSyllabusDto> GetSyllabusByCourseId(int courseId);
        Task<CourseSyllabusDto> CreateSyllabus(CourseSyllabusCreateDto dto);
        Task<CourseSyllabusDto?> UpdateSyllabusById(int courseSyllabusId, UpdateCourseSyllabusDto dto);

    }
}
