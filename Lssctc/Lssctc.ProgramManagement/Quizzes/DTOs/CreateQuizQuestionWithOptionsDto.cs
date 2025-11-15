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

        // Question Score - REQUIRED for CreateQuizWithQuestionsDto
        [Required(ErrorMessage = "Question score is required.")]
        [Range(0.01, 9.99, ErrorMessage = "Question score must be between 0.01 and 9.99.")]
        public decimal? QuestionScore { get; set; }

        // Options
        [Required(ErrorMessage = "Options are required.")]
        [MinLength(2, ErrorMessage = "At least 2 options are required.")]
        [MaxLength(20, ErrorMessage = "Maximum 20 options allowed.")]
        public List<CreateQuizQuestionOptionDto> Options { get; set; } = new();
    }

    /// <summary>
    /// DTO for creating quiz options.
    /// IMPORTANT: OptionScore is AUTO-CALCULATED by the server based on:
    /// - QuestionScore (from parent question)
    /// - IsMultipleAnswers flag
    /// - Number of correct options
    /// 
    /// DO NOT provide OptionScore in the JSON request - it will be ignored or cause validation errors.
    /// </summary>
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

        /// <summary>
        /// DEPRECATED for CreateQuizWithQuestionsDto - will be auto-calculated.
        /// Client should NOT provide this value.
        /// 
        /// Only used internally when creating questions individually via CreateQuestionWithOptionsByQuizId.
        /// 
        /// CALCULATION LOGIC:
        /// - Single choice: correct option = QuestionScore, incorrect option = 0
        /// - Multiple choice: each correct option = QuestionScore / number of correct options, incorrect = 0
        /// 
        /// Example:
        ///   Question: "What is 2+2?" (QuestionScore = 2.5, Single choice)
        ///   Option A: "4" (IsCorrect=true) → OptionScore = 2.5 (AUTO-CALCULATED)
        ///   Option B: "5" (IsCorrect=false) → OptionScore = 0 (AUTO-CALCULATED)
        /// 
        ///   Question: "Which are fruits?" (QuestionScore = 3.0, Multiple choice, 2 correct)
        ///   Option A: "Apple" (IsCorrect=true) → OptionScore = 1.5 (AUTO-CALCULATED)
        ///   Option B: "Banana" (IsCorrect=true) → OptionScore = 1.5 (AUTO-CALCULATED)
        ///   Option C: "Stone" (IsCorrect=false) → OptionScore = 0 (AUTO-CALCULATED)
        /// </summary>
        [Range(0, 999.99, ErrorMessage = "Option score must be between 0 and 999.99.")]
        public decimal? OptionScore { get; set; }
    }

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

    /// <summary>
    /// DTO to create a complete Quiz with all Questions and Options in one request.
    /// 
    /// IMPORTANT NOTES:
    /// 1. Each question MUST have QuestionScore (0.01 - 9.99)
    /// 2. Total of all question scores MUST NOT exceed 10
    /// 3. Each question MUST have 2-20 options
    /// 4. Each question MUST have at least 1 correct option
    /// 5. Single choice questions can only have 1 correct option
    /// 6. OptionScore is AUTO-CALCULATED - DO NOT provide it in JSON
    /// 
    /// Server will automatically calculate OptionScore for each option based on:
    /// - Question type (single vs multiple choice)
    /// - Number of correct options
    /// - Question score
    /// 
    /// EXAMPLE JSON (correct format):
    /// {
    ///   "name": "Math Quiz",
    ///   "passScoreCriteria": 6,
    ///   "timelimitMinute": 30,
    ///   "description": "Basic math",
    ///   "questions": [
    ///     {
    ///       "name": "What is 2+2?",
    ///       "questionScore": 5,
    ///       "isMultipleAnswers": false,
    ///       "options": [
    ///         {"name": "4", "isCorrect": true},
    ///         {"name": "5", "isCorrect": false}
    ///       ]
    ///     },
    ///     {
    ///       "name": "Which are fruits?",
    ///       "questionScore": 5,
    ///       "isMultipleAnswers": true,
    ///       "options": [
    ///         {"name": "Apple", "isCorrect": true},
    ///         {"name": "Banana", "isCorrect": true},
    ///         {"name": "Stone", "isCorrect": false}
    ///       ]
    ///     }
    ///   ]
    /// }
    /// 
    /// INCORRECT (DO NOT USE):
    /// {
    ///   "questions": [
    ///     {
    ///       "options": [
    ///         {"name": "4", "isCorrect": true, "optionScore": 5}  ← WRONG! Will be ignored
    ///       ]
    ///     }
    ///   ]
    /// }
    /// </summary>
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

        [Range(0.01, 9.99, ErrorMessage = "Question score must be between 0.01 and 9.99.")]
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
