using System.ComponentModel.DataAnnotations;

namespace Lssctc.ProgramManagement.Quizzes.DTOs
{
    public class QuizDto
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public decimal? PassScoreCriteria { get; set; }
        public int? TimelimitMinute { get; set; }
        public decimal? TotalScore { get; set; }
        public string? Description { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public List<QuizQuestionDto> Questions { get; set; } = new();
    }

    public class QuizDetailDto
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public decimal? PassScoreCriteria { get; set; }
        public int? TimelimitMinute { get; set; }
        public decimal? TotalScore { get; set; }
        public string? Description { get; set; }

        public List<QuizDetailQuestionDto> Questions { get; set; } = new();
    }

    public class QuizOnlyDto
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public decimal? PassScoreCriteria { get; set; }
        public int? TimelimitMinute { get; set; }
        public decimal? TotalScore { get; set; }
        public string? Description { get; set; }


    }

    public class QuizTraineeDetailDto
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public decimal? PassScoreCriteria { get; set; }
        public int? TimelimitMinute { get; set; }
        public decimal? TotalScore { get; set; }
        public string? Description { get; set; }
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



    public class UpdateQuizDto : CreateQuizDto { }


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


    public class QuizTraineeQuestionOptionDto
    {
        public int Id { get; set; }
        public int QuizQuestionId { get; set; }
        public string Name { get; set; } = null!;
        public string? Description { get; set; }
        public int? DisplayOrder { get; set; }
        public decimal? OptionScore { get; set; }
        // KHÔNG có IsCorrect
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
}
