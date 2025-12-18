using Lssctc.ProgramManagement.ClassManage.Classes.Dtos;
using Lssctc.Share.Common;
using Microsoft.AspNetCore.Http;

namespace Lssctc.ProgramManagement.ClassManage.Classes.Services
{
    public interface IClassesService
    {
        #region Classes
        Task<IEnumerable<ClassDto>> GetAllClassesAsync();
        Task<PagedResult<ClassDto>> GetClassesAsync(int pageNumber, int pageSize, string? searchTerm = null, string? sortBy = null, string? sortDirection = null, string? status = null);
        Task<ClassDto?> GetClassByIdAsync(int id);
        Task<ClassDto> CreateClassAsync(CreateClassDto dto);
        Task<ClassDto> UpdateClassAsync(int id, UpdateClassDto dto);
        Task OpenClassAsync(int id);
        Task StartClassAsync(int id);
        Task CompleteClassAsync(int id);
        Task CancelClassAsync(int id);
        Task DeleteClassDataRecursiveAsync(int classId);
        Task<string> ImportTraineesToClassAsync(int classId, IFormFile file);
        #endregion

        #region Classes By other Filters
        Task<IEnumerable<ClassDto>> GetClassesByProgramAndCourseAsync(int programId, int courseId);
        Task<IEnumerable<ClassDto>> GetClassesByCourseAsync(int courseId);
        Task<IEnumerable<ClassDto>> GetClassesByCourseIdForTrainee(int courseId);
        Task<IEnumerable<ClassDto>> GetClassesByInstructorAsync(int instructorId);
        Task<IEnumerable<ClassDto>> GetAllClassesByTraineeAsync(int traineeId);
        Task<PagedResult<ClassDto>> GetPagedClassesByTraineeAsync(int traineeId, int pageNumber, int pageSize);
        Task<ClassDto?> GetClassByIdAndTraineeAsync(int classId, int traineeId);
        #endregion


    }
}
