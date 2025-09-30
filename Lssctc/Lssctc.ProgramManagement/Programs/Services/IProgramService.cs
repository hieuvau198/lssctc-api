using Lssctc.ProgramManagement.Programs.DTOs;
using Lssctc.Share.Common;

namespace Lssctc.ProgramManagement.Programs.Services
{
    public interface IProgramService
    {
        Task<List<ProgramDto>> GetAllPrograms();
        Task<PagedResult<ProgramDto?>> GetPrograms(ProgramQueryParameters parameters);
        Task<ProgramDto?> GetProgramById(int id);
        Task<ProgramDto> CreateProgram(CreateProgramDto dto);
        Task<ProgramDto?> AddCoursesToProgram(int programId, List<CourseOrderDto> coursesToAdd);
        Task<ProgramDto?> AddPrerequisitesToProgram(int programId, List<EntryRequirementDto> prerequisitesToAdd);
        Task<ProgramDto?> UpdateProgram(int id, UpdateProgramInfoDto dto);
        Task<ProgramDto?> UpdateProgramCourses(int id, ICollection<ProgramCourseOrderDto> courses);
        Task<ProgramDto?> UpdateProgramEntryRequirements(int id, ICollection<UpdateEntryRequirementDto> entryRequirements);
        Task<bool> DeleteProgram(int id);
    }
}
