using Lssctc.LearningManagement.Quizzes.DTOs;

namespace Lssctc.LearningManagement.Quizzes.Services
{
    public interface IQuizService
    {
        Task<(IReadOnlyList<QuizDetailDto> Items, int Total)> GetDetailQuizzes(
      int pageIndex, int pageSize, string? search);
        Task<QuizDto?> GetQuizById(int id);
        Task<int> CreateQuiz(CreateQuizDto dto);
        Task<bool> UpdateQuizById(int id, UpdateQuizDto dto);
        Task<bool> DeleteQuizById(int id);
        // Add method: Get questions by quizId
        Task<QuizDetailDto?> GetQuizDetail(int quizId, CancellationToken ct = default);
        Task<QuizTraineeDetailDto?> GetQuizDetailForTrainee(
    int quizId, CancellationToken ct = default);

        Task<int> CreateQuestionByQuizId(int quizId, CreateQuizQuestionDto dto);
        // no need for quizId here, option is linked to question which is linked to quiz
        Task<int> CreateOption(int quizId, int questionId, CreateQuizQuestionOptionDto dto);
        // Add method: Get options by questionId
        Task<IReadOnlyList<QuizDetailQuestionOptionDto>> GetOptionsByQuestionId(
        int questionId, CancellationToken ct = default);
        Task<QuizQuestionOptionDto?> GetOptionById(int optionId);
        // Get quiz by quizId with questions and options
        // Create quiz with questions and options
        // Get options for trainee without correct value
    }
}
