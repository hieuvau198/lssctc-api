using Lssctc.ProgramManagement.Quizzes.DTOs;
using Lssctc.Share.Common;

namespace Lssctc.ProgramManagement.Quizzes.Services
{
    public interface IQuizService
    {
        // Issue todos:
        // remove cancellation token
        // BR for quiz management: only admin and instructor can create, update, delete quiz
        // trainee can only view quiz with no iscorrect info in options
        // can only delete quiz if not assigned to any activity yet
        #region Gets
        Task<QuizDto?> GetQuizById(int id);
        Task<QuizDto?> GetQuizById(int id, int? instructorId);
        Task<PagedResult<QuizOnlyDto>> GetQuizzes(int pageIndex, int pageSize, CancellationToken ct = default);
        Task<PagedResult<QuizOnlyDto>> GetQuizzes(int pageIndex, int pageSize, int? instructorId, CancellationToken ct = default);
        Task<PagedResult<QuizOnlyDto>> GetQuizzes(int pageIndex, int pageSize, int? instructorId, string? searchTerm, string? sortBy, string? sortDirection, CancellationToken ct = default);
        Task<QuizTraineeDetailDto?> GetQuizDetailForTrainee(int quizId, CancellationToken ct = default);
        Task<QuizTraineeDetailDto?> GetQuizDetailForTraineeByActivityIdAsync(int activityId, int? traineeId = null, CancellationToken ct = default); 
        Task<QuizTraineeDetailDto?> GetQuizTraineeDetailBySectionQuizIdAsync(int sectionQuizId, CancellationToken ct = default);
        Task<TraineeQuizResponseDto> GetQuizForTraineeByRecordIdAsync(int activityRecordId, CancellationToken ct = default);
        #endregion

        #region Manage
        Task<bool> DeleteQuizById(int id);
        Task<bool> DeleteQuizById(int id, int? instructorId);
        Task<int> CreateQuizWithQuestions(CreateQuizWithQuestionsDto dto);
        Task<int> CreateQuizWithQuestions(CreateQuizWithQuestionsDto dto, int instructorId);
        Task<int> CreateQuizFromExcel(ImportQuizExcelDto dto, int instructorId);
        Task<bool> UpdateQuizWithQuestionsAsync(int quizId, UpdateQuizWithQuestionsDto dto);
        Task<bool> UpdateQuizWithQuestionsAsync(int quizId, UpdateQuizWithQuestionsDto dto, int? instructorId);
        Task<int> AddQuizToActivity(CreateActivityQuizDto dto);
        Task<List<QuizOnlyDto>> GetQuizzesByActivityId(int activityId);

        Task<bool> RemoveQuizFromActivityAsync(int activityId, int quizId);
        Task<bool> UpdateQuizInActivityAsync(int activityId, UpdateActivityQuizDto dto);
        #endregion
    }
}
