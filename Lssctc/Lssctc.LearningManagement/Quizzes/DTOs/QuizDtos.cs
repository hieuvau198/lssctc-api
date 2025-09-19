using System.ComponentModel.DataAnnotations;

namespace Lssctc.LearningManagement.Quizzes.DTOs
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

    public class CreateQuizDto
    {
        [Required(ErrorMessage = "Tên quiz là bắt buộc.")]
        [StringLength(100, ErrorMessage = "Tên quiz tối đa 100 ký tự.")]
        public string Name { get; set; } = null!;

        [Range(0, 999.99, ErrorMessage = "Điểm đạt phải từ 0 đến 999.99.")]
        public decimal? PassScoreCriteria { get; set; }

        [Range(1, 600, ErrorMessage = "Thời gian làm bài phải từ 1 đến 600 phút.")]
        public int? TimelimitMinute { get; set; }

        [Range(0, 999.99, ErrorMessage = "Tổng điểm phải từ 0 đến 999.99.")]
        public decimal? TotalScore { get; set; }

        [StringLength(2000, ErrorMessage = "Mô tả tối đa 2000 ký tự.")]
        public string? Description { get; set; }
    }

    public class UpdateQuizDto : CreateQuizDto { }
}
