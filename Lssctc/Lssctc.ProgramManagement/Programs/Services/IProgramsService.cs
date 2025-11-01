using Lssctc.ProgramManagement.Programs.Dtos;
using Lssctc.Share.Common;

namespace Lssctc.ProgramManagement.Programs.Services
{
    public interface IProgramsService
    {
        Task<IEnumerable<ProgramDto>> GetAllProgramsAsync();
        Task<PagedResult<ProgramDto>> GetProgramsAsync(int pageNumber, int pageSize);
        Task<ProgramDto?> GetProgramByIdAsync(int id);
        Task<ProgramDto> CreateProgramAsync(CreateProgramDto createDto);
        Task<ProgramDto> UpdateProgramAsync(int id, UpdateProgramDto updateDto);
        Task DeleteProgramAsync(int id);
    }
}
