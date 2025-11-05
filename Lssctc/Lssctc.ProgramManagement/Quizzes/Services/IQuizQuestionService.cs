using Lssctc.ProgramManagement.Quizzes.DTOs;
using Lssctc.Share.Common;

namespace Lssctc.ProgramManagement.Quizzes.Services
{
    public interface IQuizQuestionService
    {
        Task<int> CreateQuestionByQuizId(int quizId, CreateQuizQuestionDto dto);
        Task<PagedResult<QuizQuestionNoOptionsDto>> GetQuestionsByQuizIdPaged(int quizId, int page, int pageSize);
        Task<QuizQuestionNoOptionsDto?> GetQuestionById(int questionId);
        Task<bool> UpdateQuestionById(int questionId, UpdateQuizQuestionDto dto);
        Task<bool> DeleteQuestionById(int questionId);
    }
}
