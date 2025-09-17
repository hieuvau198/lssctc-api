using Lssctc.LearningManagement.Quizzes.DTOs;

namespace Lssctc.LearningManagement.Quizzes.Services
{
    public interface IQuizService
    {
        Task<(IReadOnlyList<QuizDto> Items, int Total)> GetPagedAsync(int pageIndex, int pageSize, string? search);
        Task<QuizDto?> GetByIdAsync(int id);
        Task<int> CreateAsync(CreateQuizDto dto);
        Task<bool> UpdateAsync(int id, UpdateQuizDto dto);
        Task<bool> DeleteAsync(int id);
        Task<int> CreateQuestionsAsync(int quizId, CreateQuizQuestionDto dto);

    }
}
