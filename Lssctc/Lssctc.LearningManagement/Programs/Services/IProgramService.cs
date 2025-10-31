using Lssctc.LearningManagement.Programs.DTOs;
using Lssctc.Share.Common;

namespace Lssctc.LearningManagement.Programs.Services
{
    public interface IProgramService
    {
        Task<List<ProgramDto>> GetAllPrograms();
        Task<PagedResult<ProgramDto?>> GetPrograms(ProgramQueryParameters parameters);
        Task<ProgramDto?> GetProgramById(int id);
        Task<ProgramDto> CreateProgram(CreateProgramDto dto);
        // New method for add only one course each time, keep origin data
        Task<ProgramDto?> AddCourseToProgram(int programId, int courseId);
        Task<ProgramDto?> AddCoursesToProgram(int programId, List<CourseOrderDto> coursesToAdd);
        Task<ProgramDto?> AddPrerequisitesToProgram(int programId, List<EntryRequirementDto> prerequisitesToAdd);
        Task<ProgramDto?> UpdateProgram(int id, UpdateProgramInfoDto dto);
        Task<ProgramDto?> UpdateProgramCourses(int id, ICollection<ProgramCourseOrderDto> courses);
        // New method for update only entry requirement by entry requirement id
        Task<EntryRequirementDto?> UpdateProgramEntryRequirement(int id, UpdateEntryRequirementDto entryRequirement);
        Task<ProgramDto?> UpdateProgramEntryRequirements(int id, ICollection<UpdateEntryRequirementDto> entryRequirements);
        Task<bool> DeleteProgram(int id);
    }
}
