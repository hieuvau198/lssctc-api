using System.ComponentModel.DataAnnotations;

namespace Lssctc.ProgramManagement.ClassManage.FinalExams.Dtos
{
    // --- Final Exam DTOs ---
    public class FinalExamDto
    {
        public int Id { get; set; }
        public int EnrollmentId { get; set; }
        public string? TraineeName { get; set; }
        public string? TraineeCode { get; set; }
        public bool? IsPass { get; set; }
        public decimal? TotalMarks { get; set; } 
        public DateTime? CompleteTime { get; set; }
        public List<FinalExamPartialDto> Partials { get; set; } = new List<FinalExamPartialDto>();
    }

    public class CreateFinalExamDto
    {
        [Required]
        public int EnrollmentId { get; set; }
    }

    public class UpdateFinalExamDto
    {
        public bool? IsPass { get; set; }
        public decimal? TotalMarks { get; set; }
    }

    // --- Final Exam Partial DTOs ---
    public class FinalExamPartialDto
    {
        public int Id { get; set; }
        public string? Type { get; set; } // Changed to String: "Theory", "Simulation", "Practical"
        public decimal? Marks { get; set; }
        public decimal? ExamWeight { get; set; }
        public string? Description { get; set; } // Contains JSON Checklist for PE
        public int? Duration { get; set; }

        // Linked Info (Flattened for easier frontend consumption)
        public int? QuizId { get; set; }
        public string? QuizName { get; set; }
        public int? PracticeId { get; set; }
        public string? PracticeName { get; set; }
    }

    public class CreateFinalExamPartialDto
    {
        [Required]
        public int FinalExamId { get; set; }

        [Required]
        [RegularExpression("^(Theory|Simulation|Practical)$", ErrorMessage = "Type must be 'Theory', 'Simulation', or 'Practical'.")]
        public string Type { get; set; } = null!; // Changed from int to string

        [Required]
        [Range(0, 100, ErrorMessage = "Weight must be between 0 and 100")]
        public decimal ExamWeight { get; set; }

        public int? Duration { get; set; } // In minutes

        public int? QuizId { get; set; }
        public int? PracticeId { get; set; }
    }

    public class CreateClassPartialDto
    {
        [Required]
        public int ClassId { get; set; } // Thay vì FinalExamId

        [Required]
        [RegularExpression("^(Theory|Simulation|Practical)$", ErrorMessage = "Type must be 'Theory', 'Simulation', or 'Practical'.")]
        public string Type { get; set; } = null!;

        [Required]
        [Range(0, 100)]
        public decimal ExamWeight { get; set; }

        public int? Duration { get; set; }
        public int? QuizId { get; set; }
        public int? PracticeId { get; set; }
    }

    public class UpdateFinalExamPartialDto
    {
        public decimal? ExamWeight { get; set; }
        public int? Duration { get; set; }
        public int? QuizId { get; set; }
        public int? PracticeId { get; set; }
    }

    // --- Submission DTOs ---

    // For Theory Exam (TE)
    public class SubmitTeDto
    {
        [Required]
        [Range(0, 10)]
        public decimal Marks { get; set; }
    }

    // For Simulation Exam (SE)
    public class SubmitSeDto
    {
        [Required]
        [Range(0, 10)]
        public decimal Marks { get; set; }
    }

    // For Practical Exam (PE) - Using Checklist
    public class SubmitPeDto
    {
        [Required]
        public List<PeChecklistItemDto> Checklist { get; set; } = new List<PeChecklistItemDto>();
    }

    public class PeChecklistItemDto
    {
        public string Criteria { get; set; } = string.Empty; // e.g., "Kỹ thuật nâng hạ"
        public decimal Score { get; set; } // Obtained score (e.g., 8)
        public decimal MaxScore { get; set; } = 10; // Max score (e.g., 10)
    }
}