using Lssctc.LearningManagement.Practices.Dtos;
using Lssctc.Share.Common;

namespace Lssctc.LearningManagement.Practices.Services
{
    public interface IPracticeService
    {
        Task<PagedResult<PracticeDto>> GetPractices(PracticeQueryDto query);
        Task<PracticeDto?> GetPracticeById(int id);
        Task<PracticeDto> CreatePractice(CreatePracticeDto dto);
        Task<PracticeDto?> UpdatePractice(int id, UpdatePracticeDto dto);
        Task<bool> DeletePractice(int id);
        Task<bool> ExistPractice(int id);
        Task<bool> ExistsPracticeQuery(string practiceName, int? excludeId = null);        


    }
}
