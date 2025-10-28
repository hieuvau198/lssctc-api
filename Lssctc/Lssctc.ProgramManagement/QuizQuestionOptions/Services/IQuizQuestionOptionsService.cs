using Lssctc.ProgramManagement.QuizQuestionOptions.DTOs;

namespace Lssctc.ProgramManagement.QuizQuestionOptions.Services
{
    public interface IQuizQuestionOptionsService
    {
        Task<int> CreateOption(int questionId, CreateQuizQuestionOptionDto dto);

        Task<QuizQuestionOptionDto?> GetOptionById(int optionId);

        Task<IReadOnlyList<QuizDetailQuestionOptionDto>> GetOptionsByQuestionId(
       int questionId, CancellationToken ct = default);

        Task UpdateOption(int optionId, UpdateQuizQuestionOptionDto dto);
    }
}
