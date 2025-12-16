using Lssctc.ProgramManagement.Programs.Dtos;
using Lssctc.Share.Common;
using Microsoft.AspNetCore.Http;

namespace Lssctc.ProgramManagement.Programs.Services
{
    public interface IProgramsService
    {
        Task<IEnumerable<ProgramDto>> GetAllProgramsAsync();
        Task<PagedResult<ProgramDto>> GetProgramsAsync(int pageNumber, int pageSize, string? searchTerm = null);
        Task<IEnumerable<ProgramDto>> GetAvailableProgramsAsync();
        Task<ProgramDto?> GetProgramByIdAsync(int id);
        Task<ProgramDto> CreateProgramAsync(CreateProgramDto createDto);
        Task<ProgramDto> UpdateProgramAsync(int id, UpdateProgramDto updateDto);
        Task DeleteProgramAsync(int id);
        Task<int> CreateProgramWithHierarchyAsync(CreateProgramWithHierarchyDto dto);
        Task<object> ImportProgramFromExcelAsync(IFormFile file);
        Task CleanupProgramDataAsync(int id);
    }
}
