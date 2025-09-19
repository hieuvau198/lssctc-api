using System.ComponentModel.DataAnnotations;

namespace Lssctc.LearningManagement.Quizzes.DTOs
{
    public class QuizQuestionOptionDto
    {
        public int Id { get; set; }
        public int QuizQuestionId { get; set; }
        public string? Name { get; set; }          
        public bool IsCorrect { get; set; }
        public int DisplayOrder { get; set; }
        public string? Explanation { get; set; }   // lời giải/giải thích (nếu có cột này)
    }

    public class CreateQuizQuestionOptionDto
    {
        [Required, StringLength(100)]
        public string Name { get; set; } = null!;

        [StringLength(2000)]
        public string? Description { get; set; }

        public bool IsCorrect { get; set; } = false;

        // BẮT BUỘC gửi: phải đúng số tiếp theo
        [Required]
        [Range(1, int.MaxValue)]
        public int DisplayOrder { get; set; }


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
