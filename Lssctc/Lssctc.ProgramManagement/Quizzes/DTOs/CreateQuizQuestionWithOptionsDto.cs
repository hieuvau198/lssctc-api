using Lssctc.ProgramManagement.QuizQuestionOptions.DTOs;

namespace Lssctc.ProgramManagement.Quizzes.DTOs
{
    public class CreateQuizQuestionWithOptionsDto
    {
        // Question
        public string Name { get; set; } = null!;
        public decimal? QuestionScore { get; set; }
        public string? Description { get; set; }
        public bool IsMultipleAnswers { get; set; }

        // Options
        public List<CreateQuizQuestionOptionDto> Options { get; set; } = new();
    }

}
