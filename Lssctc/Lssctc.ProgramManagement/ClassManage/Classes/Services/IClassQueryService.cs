using Lssctc.ProgramManagement.ClassManage.Classes.Dtos;
using Lssctc.Share.Common;

namespace Lssctc.ProgramManagement.ClassManage.Classes.Services
{
    public interface IClassQueryService
    {
        Task<IEnumerable<ClassDto>> GetAllClassesAsync();
        Task<PagedResult<ClassDto>> GetClassesAsync(int pageNumber, int pageSize, string? searchTerm = null, string? sortBy = null, string? sortDirection = null, string? status = null);
        Task<ClassDto?> GetClassByIdAsync(int id);

        // Filter Queries
        Task<IEnumerable<ClassDto>> GetClassesByProgramAndCourseAsync(int programId, int courseId);
        Task<IEnumerable<ClassDto>> GetClassesByCourseAsync(int courseId);
        Task<IEnumerable<ClassDto>> GetClassesByCourseIdForTrainee(int courseId);
        Task<IEnumerable<ClassDto>> GetClassesByInstructorAsync(int instructorId);

        // Trainee Queries
        Task<IEnumerable<ClassDto>> GetAllClassesByTraineeAsync(int traineeId);
        Task<PagedResult<ClassDto>> GetPagedClassesByTraineeAsync(int traineeId, int pageNumber, int pageSize);
        Task<ClassDto?> GetClassByIdAndTraineeAsync(int classId, int traineeId);

        // New Method
        Task<IEnumerable<ClassWithEnrollmentDto>> GetAvailableClassesByProgramCourseForTraineeAsync(int programId, int courseId, int? traineeId);
    }
}