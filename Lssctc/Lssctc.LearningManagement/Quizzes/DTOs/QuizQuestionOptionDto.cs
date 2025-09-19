using System.ComponentModel.DataAnnotations;

namespace Lssctc.LearningManagement.Quizzes.DTOs
{
    public class QuizQuestionOptionDto
    {
        public int Id { get; set; }

        public int QuizQuestionId { get; set; }

        public string? Description { get; set; }

        public bool IsCorrect { get; set; }

        public int? DisplayOrder { get; set; }

        public decimal? OptionScore { get; set; }

        public string Name { get; set; } = null!;
    }

    // DTO tạo mới
    public class CreateQuizQuestionOptionDto
    {
        [Required(ErrorMessage = "Name is required.")]
        [StringLength(100, ErrorMessage = "Name must be at most 100 characters.")]
        public string Name { get; set; } = null!;

        [StringLength(2000, ErrorMessage = "Description must be at most 2000 characters.")]
        public string? Description { get; set; }

        // DB đang default = 1, để đồng bộ thì để true
        public bool IsCorrect { get; set; } = true;

        // Khuyến nghị: bắt buộc gửi, >=1, tăng dần/không trùng trong cùng câu hỏi
        [Required(ErrorMessage = "DisplayOrder is required.")]
        [Range(1, int.MaxValue, ErrorMessage = "DisplayOrder must be >= 1.")]
        public int? DisplayOrder { get; set; }

        // DECIMAL(5,2)
        [Range(0, 999.99, ErrorMessage = "OptionScore must be between 0 and 999.99.")]
        public decimal? OptionScore { get; set; }
    }


    // Bulk theo payload nhiều item
    public class CreateQuizQuestionOptionBulkDto
    {
        [MinLength(1)]
        public List<CreateQuizQuestionOptionDto> Items { get; set; } = new();
    }

    public class UpdateQuizQuestionOptionDto
    {
        public string? Name { get; set; }
        public bool? IsCorrect { get; set; }
        public int? DisplayOrder { get; set; }
        public string? Explanation { get; set; }
    }
}
