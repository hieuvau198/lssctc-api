using Lssctc.ProgramManagement.Programs.DTOs;
using Lssctc.Share.Common;

namespace Lssctc.ProgramManagement.Programs.Services
{
    public interface IProgramService
    {
        Task<PagedResult<ProgramDto?>> GetAllProgramsAsync(ProgramQueryParameters parameters);
        Task<ProgramDto?> GetProgramByIdAsync(int id);
        Task<ProgramDto> CreateProgramAsync(CreateProgramDto dto);
        Task<ProgramDto?> UpdateProgramAsync(int id, UpdateProgramDto dto);
        Task<bool> DeleteProgramAsync(int id);
    }
}
