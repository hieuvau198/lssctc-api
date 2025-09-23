using Lssctc.ProgramManagement.Courses.DTOs;
using Lssctc.Share.Common;

namespace Lssctc.ProgramManagement.Courses.Services
{
    public interface ICourseService
    {
        // get syllabus sections by syllabusId
        // create syllabus section by syllabusId
        // update, delete syllabus sections
        Task<PagedResult<CourseDto>> GetCourses(CourseQueryParameters parameters);
        Task<CourseDto?> GetCourseById(int id);
        Task<PagedResult<CourseDto>> GetCoursesByLevelId(int levelId, int pageNumber, int pageSize);
        Task<PagedResult<CourseDto>> GetCoursesByCategoryId(int categoryId, int pageNumber, int pageSize);
        Task<PagedResult<CourseDto>> GetCoursesByFilter(int? categoryId, int? levelId, int pageNumber, int pageSize);
        Task<CourseDto> CreateCourse(CreateCourseDto dto);
        Task<CourseSyllabusDto> CreateSyllabus(CourseSyllabusCreateDto dto);
        Task<CourseSyllabusDto> GetSyllabusByCourseId(int courseId);
        Task<CourseSyllabusDto?> UpdateSyllabusById(int courseSyllabusId, UpdateCourseSyllabusDto dto);
        Task<CourseDto?> UpdateCourseById(int id,UpdateCourseDto dto);
        Task<bool> DeleteCourseById(int id);

        Task<IEnumerable<CourseLevelDto>> GetAllCourseLevelsAsync();
        Task<IEnumerable<CourseCategoryDto>> GetAllCourseCategoriesAsync();
    }
}
