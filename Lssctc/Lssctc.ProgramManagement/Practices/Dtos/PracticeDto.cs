using System.ComponentModel.DataAnnotations;

namespace Lssctc.ProgramManagement.Practices.Dtos
{
    public class PracticeDto
    {
        public int Id { get; set; }
        public string PracticeName { get; set; } = null!;
        public string? PracticeCode { get; set; } // Added
        public string? PracticeDescription { get; set; }
        public int? EstimatedDurationMinutes { get; set; }
        public string? DifficultyLevel { get; set; }
        public int? MaxAttempts { get; set; }
        public DateTime CreatedDate { get; set; }
        public bool? IsActive { get; set; }
        public string? PracticeCode { get; set; }
    }
    public class CreatePracticeDto
    {
        [Required(ErrorMessage = "Practice name is required.")]
        [StringLength(200, ErrorMessage = "Practice name cannot exceed 200 characters.")]
        public string? PracticeName { get; set; }

        [StringLength(50, ErrorMessage = "Practice code cannot exceed 50 characters.")]
        public string? PracticeCode { get; set; } // Added

        [StringLength(1000, ErrorMessage = "Practice description cannot exceed 1000 characters.")]
        public string? PracticeDescription { get; set; }

        [Range(1, 600, ErrorMessage = "Estimated duration must be between 1 and 600 minutes.")]
        public int? EstimatedDurationMinutes { get; set; }

        [RegularExpression("^(Entry|Intermediate|Advanced)$", ErrorMessage = "DifficultyLevel must be one of: Entry, Intermediate, Advanced.")]
        public string? DifficultyLevel { get; set; }

        [Range(1, 10, ErrorMessage = "Max attempts must be between 1 and 10.")]
        public int? MaxAttempts { get; set; }

        public bool? IsActive { get; set; }
        [StringLength(50, ErrorMessage = "Practice code cannot exceed 50 characters.")]
        public string? PracticeCode { get; set; }
    }
    public class UpdatePracticeDto
    {
        [StringLength(200, ErrorMessage = "Practice name cannot exceed 200 characters.")]
        public string? PracticeName { get; set; }

        [StringLength(50, ErrorMessage = "Practice code cannot exceed 50 characters.")]
        public string? PracticeCode { get; set; } // Added

        [StringLength(1000, ErrorMessage = "Practice description cannot exceed 1000 characters.")]
        public string? PracticeDescription { get; set; }

        [Range(1, 600, ErrorMessage = "Estimated duration must be between 1 and 600 minutes.")]
        public int? EstimatedDurationMinutes { get; set; }

        [RegularExpression("^(Entry|Intermediate|Advanced)$", ErrorMessage = "DifficultyLevel must be one of: Entry, Intermediate, Advanced.")]
        public string? DifficultyLevel { get; set; }

        [Range(1, 10, ErrorMessage = "Max attempts must be between 1 and 10.")]
        public int? MaxAttempts { get; set; }

        public bool? IsActive { get; set; }
        [StringLength(50, ErrorMessage = "Practice code cannot exceed 50 characters.")]
        public string? PracticeCode { get; set; }
    }
}
