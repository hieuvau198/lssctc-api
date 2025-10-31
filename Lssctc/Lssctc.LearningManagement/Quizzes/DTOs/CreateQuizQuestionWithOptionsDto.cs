using Lssctc.LearningManagement.QuizQuestionOptions.DTOs;

namespace Lssctc.LearningManagement.Quizzes.DTOs
{
    public class CreateQuizQuestionWithOptionsDto
    {
        // Question
        public string Name { get; set; } = null!;
        public string? Description { get; set; }
        public bool IsMultipleAnswers { get; set; }

        // Options
        public List<CreateQuizQuestionOptionDto> Options { get; set; } = new();
    }

}
