using Lssctc.Share.Common;
using Lssctc.SimulationManagement.Practices.Dtos;

namespace Lssctc.SimulationManagement.Practices.Services
{
    public interface IPracticeService
    {
        Task<PagedResult<PracticeDto>> GetAllAsync(PracticeQueryDto query);
        Task<PracticeDto?> GetByIdAsync(int id);
        Task<PracticeDto> CreateAsync(CreatePracticeDto dto);
        Task<PracticeDto?> UpdateAsync(int id, UpdatePracticeDto dto);
        Task<bool> DeleteAsync(int id);
        Task<bool> ExistsAsync(int id);
        Task<bool> ExistsAsync(string practiceName, int? excludeId = null);
    }
}
