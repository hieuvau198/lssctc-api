using System.ComponentModel.DataAnnotations;

namespace Lssctc.ProgramManagement.Quizzes.DTOs
{
    public class CreateQuizQuestionWithOptionsDto
    {
        // Question
        [Required(ErrorMessage = "Question name is required.")]
        [StringLength(100, MinimumLength = 1, ErrorMessage = "Question name must be between 1 and 100 characters.")]
        public string Name { get; set; } = null!;

        [StringLength(2000, ErrorMessage = "Description must be at most 2000 characters.")]
        public string? Description { get; set; }

        public bool IsMultipleAnswers { get; set; }

        [StringLength(500, ErrorMessage = "ImageUrl must be at most 500 characters.")]
        public string? ImageUrl { get; set; }

        // Options
        [Required(ErrorMessage = "Options are required.")]
        [MinLength(2, ErrorMessage = "At least 2 options are required.")]
        [MaxLength(20, ErrorMessage = "Maximum 20 options allowed.")]
        public List<CreateQuizQuestionOptionDto> Options { get; set; } = new();
    }

    public class CreateQuizQuestionOptionDto
    {
        [Required(ErrorMessage = "Option name is required.")]
        [StringLength(100, MinimumLength = 1, ErrorMessage = "Option name must be between 1 and 100 characters.")]
        public string Name { get; set; } = null!;

        [StringLength(1000, ErrorMessage = "Description must be at most 1000 characters.")]
        public string? Description { get; set; }

        public bool IsCorrect { get; set; } = false;

        [StringLength(2000, ErrorMessage = "Explanation must be at most 2000 characters.")]
        public string? Explanation { get; set; }

        // DisplayOrder removed - will be auto-generated in service to avoid conflicts

        [Range(0, 999.99, ErrorMessage = "Option score must be between 0 and 999.99.")]
        public decimal? OptionScore { get; set; }
    }

    //create and update quiz question 
    public class CreateQuizQuestionDto
    {
        [Required(ErrorMessage = "Question name is required.")]
        [StringLength(100, MinimumLength = 1, ErrorMessage = "Question name must be between 1 and 100 characters.")]
        public string Name { get; set; } = null!;

        [Required(ErrorMessage = "Question score is required.")]
        [Range(0.01, 9.99, ErrorMessage = "Question score must be between 0.01 and 9.99.")]
        public decimal? QuestionScore { get; set; }

        [StringLength(2000, ErrorMessage = "Description must be at most 2000 characters.")]
        public string? Description { get; set; }
        
        public bool IsMultipleAnswers { get; set; }
        
        [StringLength(500, ErrorMessage = "ImageUrl must be at most 500 characters.")]
        public string? ImageUrl { get; set; }
    }

    // DTO to create Quiz with Questions and Options
    public class CreateQuizWithQuestionsDto
    {
        [Required(ErrorMessage = "Quiz name is required.")]
        [StringLength(100, MinimumLength = 1, ErrorMessage = "Quiz name must be between 1 and 100 characters.")]
        public string Name { get; set; } = null!;

        [Range(0.01, 10, ErrorMessage = "Pass score criteria must be greater than 0 and less than or equal to 10.")]
        public decimal? PassScoreCriteria { get; set; }

        [Range(1, 600, ErrorMessage = "Time limit must be between 1 and 600 minutes.")]
        public int? TimelimitMinute { get; set; }

        [StringLength(999, ErrorMessage = "Description must be at most 999 characters.")]
        public string? Description { get; set; }

        [Required(ErrorMessage = "Questions are required.")]
        [MinLength(1, ErrorMessage = "At least 1 question is required.")]
        [MaxLength(100, ErrorMessage = "Maximum 100 questions allowed.")]
        public List<CreateQuizQuestionWithOptionsDto> Questions { get; set; } = new();
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

        [StringLength(100, ErrorMessage = "Question name must be at most 100 characters.")]
        public string? Name { get; set; }

        [Range(0.01, 999.99, ErrorMessage = "Question score must be between 0.01 and 999.99.")]
        public decimal? QuestionScore { get; set; }

        [StringLength(2000, ErrorMessage = "Description must be at most 2000 characters.")]
        public string? Description { get; set; }

        public bool? IsMultipleAnswers { get; set; }

        [StringLength(500, ErrorMessage = "ImageUrl must be at most 500 characters.")]
        public string? ImageUrl { get; set; }
    }

    public class UpdateQuizQuestionOptionDto
    {
        [StringLength(100, ErrorMessage = "Option name must be at most 100 characters.")]
        public string? Name { get; set; }

        [StringLength(2000, ErrorMessage = "Description must be at most 2000 characters.")]
        public string? Description { get; set; }

        public bool? IsCorrect { get; set; }

        // DisplayOrder kept for update operations
        public int? DisplayOrder { get; set; }

        [StringLength(2000, ErrorMessage = "Explanation must be at most 2000 characters.")]
        public string? Explanation { get; set; }

        [Range(0, 999.99, ErrorMessage = "Option score must be between 0 and 999.99.")]
        public decimal? OptionScore { get; set; }
    }
}
