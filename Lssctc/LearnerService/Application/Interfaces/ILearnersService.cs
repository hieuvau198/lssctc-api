using LearnerService.Application.Common;
using LearnerService.Application.Dtos;
using System.Threading.Tasks;

namespace LearnerService.Application.Interfaces;

public interface ILearnersService
{
    Task<PagedResult<LearnerDto>> GetLearnersAsync(LearnerQueryParameters parameters);
    Task<LearnerDto?> GetLearnerByIdAsync(int userId);
    Task<LearnerDto> CreateLearnerAsync(CreateLearnerDto createLearnerDto);
    Task<LearnerDto?> UpdateLearnerAsync(int userId, UpdateLearnerDto updateLearnerDto);
    Task<bool> DeleteLearnerAsync(int userId);
}