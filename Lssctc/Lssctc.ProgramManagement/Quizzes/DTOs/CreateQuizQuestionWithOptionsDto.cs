using System.ComponentModel.DataAnnotations;

namespace Lssctc.ProgramManagement.Quizzes.DTOs
{
    public class CreateQuizQuestionWithOptionsDto
    {
        // Question
        public string Name { get; set; } = null!;
        public string? Description { get; set; }
        public bool IsMultipleAnswers { get; set; }
        public string? ImageUrl { get; set; }

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

    //create and update quiz question 
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
        
        public bool IsMultipleAnswers { get; set; }
        
        [StringLength(500, ErrorMessage = "ImageUrl must be at most 500 characters.")]
        public string? ImageUrl { get; set; }
    }

   

    // New DTO for bulk updating all questions in a quiz
    public class BulkUpdateQuizQuestionsDto
    {
        [Required(ErrorMessage = "Questions list is required.")]
        public List<UpdateQuizQuestionItemDto> Questions { get; set; } = new();
    }

    public class UpdateQuizQuestionItemDto
    {
        [Required(ErrorMessage = "Question ID is required.")]
        public int Id { get; set; }

        public string? Name { get; set; }

        [Range(0.01, 999.99, ErrorMessage = "QuestionScore must be between 0.01 and 999.99.")]
        public decimal? QuestionScore { get; set; }

        [StringLength(2000, ErrorMessage = "Description must be at most 2000 characters.")]
        public string? Description { get; set; }

        public bool? IsMultipleAnswers { get; set; }

        [StringLength(500, ErrorMessage = "ImageUrl must be at most 500 characters.")]
        public string? ImageUrl { get; set; }
    }
}
