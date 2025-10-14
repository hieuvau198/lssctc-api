using System.ComponentModel.DataAnnotations;

namespace Lssctc.LearningManagement.Quizzes.DTOs
{
    public class QuizQuestionDto
    {
        public int Id { get; set; }
        public int QuizId { get; set; }
        public string Name { get; set; } = null!;
        public decimal? QuestionScore { get; set; }
        public string? Description { get; set; }
        public bool IsMultipleAnswers { get; set; }

        public List<QuizQuestionOptionDto> Options { get; set; } = new();
    }

    public class QuizQuestionNoOptionsDto
    {
        public int Id { get; set; }
        public int QuizId { get; set; }
        public string Name { get; set; } = null!;
        public decimal? QuestionScore { get; set; }
        public string? Description { get; set; }
        public bool IsMultipleAnswers { get; set; }
    }


    public class QuizDetailQuestionDto
    {
        public int Id { get; set; }
        public int QuizId { get; set; }
        public string Name { get; set; } = null!;
        public decimal? QuestionScore { get; set; }
        public string? Description { get; set; }
        public bool IsMultipleAnswers { get; set; }

        public List<QuizDetailQuestionOptionDto> Options { get; set; } = new();
    }


    public class QuizTraineeQuestionDto
    {
        public int Id { get; set; }
        public int QuizId { get; set; }
        public string Name { get; set; } = null!;
        public decimal? QuestionScore { get; set; }
        public string? Description { get; set; }
        public bool IsMultipleAnswers { get; set; }
        public List<QuizTraineeQuestionOptionDto> Options { get; set; } = new();
    }

    public class CreateQuizQuestionDto
    {
        [Required(ErrorMessage = "Name is required.")]
        [StringLength(100, ErrorMessage = "Name must be at most 100 characters.")]
        public string Name { get; set; } = null!;

        [Required(ErrorMessage = "QuestionScore is required.")]
        [Range(0.01, 999.99, ErrorMessage = "QuestionScore must be between 0.01 and 999.99.")]
        public decimal? QuestionScore { get; set; }

        [StringLength(2000, ErrorMessage = "Description must be at most 2000 characters.")]
        public string? Description { get; set; }
    }

    public class UpdateQuizQuestionDto
    {
        public string? Name { get; set; }
        public decimal? QuestionScore { get; set; }
        public string? Description { get; set; }
    }
}
