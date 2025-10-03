using Lssctc.ProgramManagement.SectionQuizzes.DTOs;
using Lssctc.Share.Common;

namespace Lssctc.ProgramManagement.SectionQuizzes.Services
{
    public interface ISectionQuizService
    {
        Task<PagedResult<SectionQuizDto>> GetSectionQuizzesPagination(int pageIndex, int pageSize);
        Task<IReadOnlyList<SectionQuizDto>> GetSectionQuizzesNoPagination();
        Task<SectionQuizDto?> GetSectionQuizById(int id);
        Task<int> CreateSectionQuiz(CreateSectionQuizDto dto);
        Task<bool> UpdateSectionQuiz(int id, UpdateSectionQuizDto dto);
        Task<bool> DeleteSectionQuiz(int id);
    }
}
