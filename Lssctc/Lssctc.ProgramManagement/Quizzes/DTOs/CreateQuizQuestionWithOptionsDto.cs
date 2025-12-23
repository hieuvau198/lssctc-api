using System.ComponentModel.DataAnnotations;

namespace Lssctc.ProgramManagement.Quizzes.DTOs
{
    public class CreateQuizQuestionWithOptionsDto
    {
        [Required(ErrorMessage = "Question name is required.")]
        [StringLength(100, MinimumLength = 1, ErrorMessage = "Question name must be between 1 and 100 characters.")]
        public string Name { get; set; } = null!;

        [StringLength(2000, ErrorMessage = "Description must be at most 2000 characters.")]
        public string? Description { get; set; }

        public bool IsMultipleAnswers { get; set; }

        [StringLength(500, ErrorMessage = "ImageUrl must be at most 500 characters.")]
        public string? ImageUrl { get; set; }

        [Required(ErrorMessage = "Question score is required.")]
        [Range(0.01, 10, ErrorMessage = "Question score must be between 0.01 and 10.")]
        public decimal? QuestionScore { get; set; }

        // Options
        [Required(ErrorMessage = "Options are required.")]
        [MinLength(2, ErrorMessage = "At least 2 options are required.")]
        [MaxLength(20, ErrorMessage = "Maximum 20 options allowed.")]
        public List<CreateQuizQuestionOptionDto> Options { get; set; } = new();
    }

    public class CreateQuizQuestionOptionDto
    {
        [Required(ErrorMessage = "Option name is required.")]
        [StringLength(500, MinimumLength = 1, ErrorMessage = "Option name must be between 1 and 500 characters.")]
        public string Name { get; set; } = null!;
        [StringLength(1000, ErrorMessage = "Description must be at most 1000 characters.")]
        public string? Description { get; set; }
        public bool IsCorrect { get; set; } = false;
        [StringLength(2000, ErrorMessage = "Explanation must be at most 2000 characters.")]
        public string? Explanation { get; set; }
        [Range(0, 999.99, ErrorMessage = "Option score must be between 0 and 999.99.")]
        public decimal? OptionScore { get; set; }
    }

    
    public class CreateQuizWithQuestionsDto
    {
        [Required(ErrorMessage = "Quiz name is required.")]
        [StringLength(100, MinimumLength = 1, ErrorMessage = "Quiz name must be between 1 and 100 characters.")]
        public string Name { get; set; } = null!;
        [Range(0.01, 10, ErrorMessage = "Pass score criteria must be greater than 0 and less than or equal to 10.")]
        public decimal? PassScoreCriteria { get; set; }
        [Range(1, 600, ErrorMessage = "Time limit must be between 1 and 600 minutes.")]
        public int? TimelimitMinute { get; set; }
        [StringLength(2000, ErrorMessage = "Description must be at most 2000 characters.")]
        public string? Description { get; set; }
        public int? MaxAttempts { get; set; }
        [Required(ErrorMessage = "Questions are required.")]
        [MinLength(1, ErrorMessage = "At least 1 question is required.")]
        [MaxLength(100, ErrorMessage = "Maximum 100 questions allowed.")]
        public List<CreateQuizQuestionWithOptionsDto> Questions { get; set; } = new();
    }

    public class UpdateQuizQuestionItemDto
    {
        [Required(ErrorMessage = "Question ID is required.")]
        public int Id { get; set; }
        [StringLength(100, ErrorMessage = "Question name must be at most 100 characters.")]
        public string? Name { get; set; }
        [Range(0.01, 10, ErrorMessage = "Question score must be between 0.01 and 10.")]
        public decimal? QuestionScore { get; set; }
        [StringLength(2000, ErrorMessage = "Description must be at most 2000 characters.")]
        public string? Description { get; set; }
        public bool? IsMultipleAnswers { get; set; }
        [StringLength(500, ErrorMessage = "ImageUrl must be at most 500 characters.")]
        public string? ImageUrl { get; set; }
    }

    public class UpdateQuizWithQuestionsDto
    {
        [StringLength(100, ErrorMessage = "Quiz name must be at most 100 characters.")]
        public string? Name { get; set; }
        [Range(0.01, 10, ErrorMessage = "Pass score criteria must be greater than 0 and less than or equal to 10.")]
        public decimal? PassScoreCriteria { get; set; }
        [Range(1, 600, ErrorMessage = "Time limit must be between 1 and 600 minutes.")]
        public int? TimelimitMinute { get; set; }
        [StringLength(2000, ErrorMessage = "Description must be at most 2000 characters.")]
        public string? Description { get; set; }
        public int? MaxAttempts { get; set; }
        [Required(ErrorMessage = "Questions are required.")]
        [MinLength(1, ErrorMessage = "At least 1 question is required.")]
        [MaxLength(100, ErrorMessage = "Maximum 100 questions allowed.")]
        public List<UpdateQuizQuestionWithOptionsDto> Questions { get; set; } = new();
    }

    public class UpdateQuizQuestionWithOptionsDto
    {
        [Required(ErrorMessage = "Question name is required.")]
        [StringLength(500, MinimumLength = 1, ErrorMessage = "Question name must be between 1 and 500 characters.")]
        public string Name { get; set; } = null!;
        [StringLength(2000, ErrorMessage = "Description must be at most 2000 characters.")]
        public string? Description { get; set; }
        public bool IsMultipleAnswers { get; set; }
        [StringLength(500, ErrorMessage = "ImageUrl must be at most 500 characters.")]
        public string? ImageUrl { get; set; }
        [Required(ErrorMessage = "Question score is required.")]
        [Range(0.01, 10, ErrorMessage = "Question score must be between 0.01 and 10.")]
        public decimal? QuestionScore { get; set; }
        [Required(ErrorMessage = "Options are required.")]
        [MinLength(2, ErrorMessage = "At least 2 options are required.")]
        [MaxLength(20, ErrorMessage = "Maximum 20 options allowed.")]
        public List<UpdateQuizQuestionOptionWithoutIdDto> Options { get; set; } = new();
    }

    public class UpdateQuizQuestionOptionWithoutIdDto
    {
        [Required(ErrorMessage = "Option name is required.")]
        [StringLength(500, MinimumLength = 1, ErrorMessage = "Option name must be between 1 and 500 characters.")]
        public string Name { get; set; } = null!;
        [StringLength(1000, ErrorMessage = "Description must be at most 1000 characters.")]
        public string? Description { get; set; }
        public bool IsCorrect { get; set; } = false;
        [StringLength(2000, ErrorMessage = "Explanation must be at most 2000 characters.")]
        public string? Explanation { get; set; }
    }
}
