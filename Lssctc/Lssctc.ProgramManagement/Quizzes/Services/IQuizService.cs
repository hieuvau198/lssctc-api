using Lssctc.ProgramManagement.QuizQuestionOptions.DTOs;
using Lssctc.ProgramManagement.Quizzes.DTOs;
using Lssctc.Share.Common;

namespace Lssctc.ProgramManagement.Quizzes.Services
{
    public interface IQuizService
    {
        #region Gets
        Task<QuizDto?> GetQuizById(int id);
        Task<PagedResult<QuizOnlyDto>> GetQuizzes(int pageIndex, int pageSize, CancellationToken ct = default);
        Task<PagedResult<QuizDetailDto>> GetDetailQuizzes(int pageIndex, int pageSize, CancellationToken ct = default);
        Task<QuizDetailDto?> GetQuizDetail(int quizId, CancellationToken ct = default);
        Task<QuizTraineeDetailDto?> GetQuizDetailForTrainee(int quizId, CancellationToken ct = default);
        Task<QuizTraineeDetailDto?> GetQuizTraineeDetailBySectionQuizIdAsync(int sectionQuizId, CancellationToken ct = default);
        #endregion

        #region Manage
        Task<int> CreateQuiz(CreateQuizDto dto);
        Task<bool> UpdateQuizById(int id, UpdateQuizDto dto);
        Task<bool> DeleteQuizById(int id);
        Task<int> CreateQuestionWithOptionsByQuizId(int quizId, CreateQuizQuestionWithOptionsDto dto);
        #endregion
    }
}
