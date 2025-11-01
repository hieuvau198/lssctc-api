using Lssctc.ProgramManagement.Programs.Dtos;
using Lssctc.Share.Common;

namespace Lssctc.ProgramManagement.Programs.Services
{
    public interface IProgramsService
    {
        // get all program
        Task<IEnumerable<ProgramDto>> GetAllProgramsAsync();
        // get all program with paging
        Task<PagedResult<ProgramDto>> GetProgramsAsync(int pageNumber, int pageSize);
        // get program by id
        Task<ProgramDto?> GetProgramByIdAsync(int id);
        // create program
        Task<ProgramDto> CreateProgramAsync(CreateProgramDto createDto);
        // update program
        Task<ProgramDto> UpdateProgramAsync(int id, UpdateProgramDto updateDto);
        // delete program
        Task DeleteProgramAsync(int id);
    }
}
