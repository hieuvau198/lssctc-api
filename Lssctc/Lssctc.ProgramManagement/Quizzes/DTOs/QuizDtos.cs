using System.ComponentModel.DataAnnotations;

namespace Lssctc.ProgramManagement.Quizzes.DTOs
{
    #region Quiz DTOs

    public class QuizDto
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public decimal? PassScoreCriteria { get; set; }
        public int? TimelimitMinute { get; set; }
        public decimal? TotalScore { get; set; }
        public string? Description { get; set; }
        public int MaxAttempts { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public List<QuizQuestionDto> Questions { get; set; } = new();
    }

    public class QuizOnlyDto
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public decimal? PassScoreCriteria { get; set; }
        public int? TimelimitMinute { get; set; }
        public decimal? TotalScore { get; set; }
        public string? Description { get; set; }
        public int MaxAttempts { get; set; }
    }

    public class QuizTraineeDetailDto
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public decimal? PassScoreCriteria { get; set; }
        public int? TimelimitMinute { get; set; }
        public decimal? TotalScore { get; set; }
        public string? Description { get; set; }
        public int MaxAttempts { get; set; }
        public List<QuizTraineeQuestionDto> Questions { get; set; } = new();
    }

    public class CreateQuizDto
    {
        [Required(ErrorMessage = "Tên quiz là bắt buộc.")]
        [StringLength(100, ErrorMessage = "Tên quiz tối đa 100 ký tự.")]
        public string Name { get; set; } = null!;

        [Range(0.01, 10, ErrorMessage = "Điểm đạt phải lớn hơn 0 và nhỏ hơn hoặc bằng 999.99.")]
        public decimal? PassScoreCriteria { get; set; }

        [Range(1, 600, ErrorMessage = "Thời gian làm bài phải từ 1 đến 600 phút.")]
        public int? TimelimitMinute { get; set; }

        [StringLength(999, ErrorMessage = "Mô tả tối đa 999 ký tự.")]
        public string? Description { get; set; }
    }

    public class ImportQuizExcelDto
    {
        [Required(ErrorMessage = "Excel file is required.")]
        public IFormFile File { get; set; } = null!;

        [Required(ErrorMessage = "Quiz name is required.")]
        [StringLength(100, ErrorMessage = "Quiz name cannot exceed 100 characters.")]
        public string Name { get; set; } = null!;

        [Range(0.01, 10, ErrorMessage = "Pass score must be between 0.01 and 10.")]
        public decimal? PassScoreCriteria { get; set; }

        [Range(1, 600, ErrorMessage = "Time limit must be between 1 and 600 minutes.")]
        public int? TimelimitMinute { get; set; }

        [StringLength(2000, ErrorMessage = "Description cannot exceed 2000 characters.")]
        public string? Description { get; set; }
        public int? MaxAttempts { get; set; }
    }

    #endregion

    #region Question DTOs

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

    public class QuizQuestionNoOptionsDto
    {
        public int Id { get; set; }
        public int QuizId { get; set; }
        public string Name { get; set; } = null!;
        public decimal? QuestionScore { get; set; }
        public string? Description { get; set; }
        public bool IsMultipleAnswers { get; set; }
        public string? ImageUrl { get; set; }
    }

    #endregion

    #region Option DTOs

    public class QuizQuestionOptionDto
    {
        public int Id { get; set; }
        public int QuizQuestionId { get; set; }
        public string? Description { get; set; }
        public bool IsCorrect { get; set; }
        public string? Explanation { get; set; }
        public int? DisplayOrder { get; set; }
        public decimal? OptionScore { get; set; }
        public string Name { get; set; } = null!;
    }

    public class QuizDetailQuestionOptionDto
    {
        public int Id { get; set; }
        public int QuizQuestionId { get; set; }
        public string? Description { get; set; }
        public bool IsCorrect { get; set; }
        public int? DisplayOrder { get; set; }
        public decimal? OptionScore { get; set; }
        public string Name { get; set; } = null!;
    }

    public class QuizTraineeQuestionOptionDto
    {
        public int Id { get; set; }
        public int QuizQuestionId { get; set; }
        public string Name { get; set; } = null!;
        public string? Description { get; set; }
        public int? DisplayOrder { get; set; }
        public decimal? OptionScore { get; set; }
    }

    #endregion

    #region Trainee Quiz
    public class TraineeQuizResponseDto
    {
        public QuizTraineeDetailDto Quiz { get; set; }
        public QuizSessionStatusDto SessionStatus { get; set; }
    }
    public class QuizSessionStatusDto
    {
        public bool IsOpen { get; set; }
        public DateTime? StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public string Message { get; set; }
    }
    #endregion
}
