using Lssctc.ProgramManagement.Programs.DTOs;
using Lssctc.Share.Common;

namespace Lssctc.ProgramManagement.Programs.Services
{
    public interface IProgramService
    {
        Task<PagedResult<ProgramDto?>> GetAllProgramsAsync(ProgramQueryParameters parameters);
        Task<ProgramDto?> GetProgramByIdAsync(int id);
        Task<ProgramDto> CreateProgramAsync(CreateProgramDto dto);
        Task<ProgramDto?> AddCoursesToProgramAsync(int programId, List<CourseOrderDto> coursesToAdd);
        Task<ProgramDto?> AddPrerequisitesToProgramAsync(int programId, List<EntryRequirementDto> prerequisitesToAdd);
        Task<ProgramDto?> UpdateProgramBasicAsync(int id, UpdateProgramInfoDto dto);
        Task<ProgramDto?> UpdateProgramCoursesAsync(int id, ICollection<ProgramCourseOrderDto> courses);
        Task<ProgramDto?> UpdateProgramEntryRequirementsAsync(int id, ICollection<UpdateEntryRequirementDto> entryRequirements);
        Task<bool> DeleteProgramAsync(int id);
    }
}
