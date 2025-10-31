using System.ComponentModel.DataAnnotations;

namespace Lssctc.ProgramManagement.Quizzes.DTOs
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

    public class CreateQuizQuestionOptionDto
    {
        [Required(ErrorMessage = "Name is required.")]
        [StringLength(100, ErrorMessage = "Name must be at most 100 characters.")]
        public string Name { get; set; } = null!;

        [StringLength(1000, ErrorMessage = "Description must be at most 1000 characters.")]
        public string? Description { get; set; }

        public bool IsCorrect { get; set; } = true;

        public string? Explanation { get; set; }
        public int? DisplayOrder { get; set; }

        [Range(0, 999.99, ErrorMessage = "OptionScore must be between 0 and 999.99.")]
        public decimal? OptionScore { get; set; }
    }
}
