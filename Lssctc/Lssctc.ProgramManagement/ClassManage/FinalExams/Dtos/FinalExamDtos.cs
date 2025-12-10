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

        public string? ExamCode { get; set; }

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

    public class PracticeDto
    {
        public int id { get; set; }
        public string practiceName { get; set; } = null!;
        public string practiceCode { get; set; } = null!;
        public string practiceDescription { get; set; } = null!;
        public int estimatedDurationMinutes { get; set; }
        public string difficultyLevel { get; set; } = null!;
        public int maxAttempts { get; set; }
        public string createdDate { get; set; } = null!;
        public bool isActive { get; set; }
        public bool isCompleted { get; set; }
    }

    public class SePracticeListDto : PracticeDto
    {
        public int FinalExamPartialId { get; set; }
        public string? FinalExamPartialStatus { get; set; } // e.g., "NotYet", "Submitted"
        public DateTime? StartTime { get; set; }
        public DateTime? EndTime { get; set; }
    }
    // --- Final Exam Partial DTOs ---
    public class FinalExamPartialDto
    {
        public int Id { get; set; }
        public string? Type { get; set; } // "Theory", "Simulation", "Practical"
        public decimal? Marks { get; set; }
        public decimal? ExamWeight { get; set; }
        public string? Description { get; set; }
        public int? Duration { get; set; }
        public DateTime? StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public DateTime? CompleteTime { get; set; }
        public string? Status { get; set; } // "NotYet", "Submitted", "Approved"
        public bool? IsPass { get; set; }

        // Linked Info
        public int? QuizId { get; set; }
        public string? QuizName { get; set; }
        public int? PracticeId { get; set; }
        public string? PracticeName { get; set; }

        // [UPDATE] List of checklist items from Entity
        public List<PeChecklistItemDto>? Checklists { get; set; }
    }

    public class CreateFinalExamPartialDto
    {
        [Required]
        public int FinalExamId { get; set; }

        [Required]
        [RegularExpression("^(Theory|Simulation|Practical)$", ErrorMessage = "Type must be 'Theory', 'Simulation', or 'Practical'.")]
        public string Type { get; set; } = null!;

        [Required]
        [Range(0, 100)]
        public decimal ExamWeight { get; set; }
        public DateTime? StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public int? Duration { get; set; }
        public int? QuizId { get; set; }
        public int? PracticeId { get; set; }
    }

    public class CreateClassPartialDto
    {
        [Required]
        public int ClassId { get; set; }

        [Required]
        [RegularExpression("^(Theory|Simulation|Practical)$", ErrorMessage = "Type must be 'Theory', 'Simulation', or 'Practical'.")]
        public string Type { get; set; } = null!;

        [Required]
        [Range(0, 100)]
        public decimal ExamWeight { get; set; }
        public DateTime? StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public int? Duration { get; set; }
        public int? QuizId { get; set; }
        public int? PracticeId { get; set; }
    }

    public class UpdateClassPartialConfigDto
    {
        [Required]
        public int ClassId { get; set; }

        [Required]
        [RegularExpression("^(Theory|Simulation|Practical)$")]
        public string Type { get; set; } = null!;

        public decimal? ExamWeight { get; set; }
        public int? Duration { get; set; }
        public DateTime? StartTime { get; set; }
        public DateTime? EndTime { get; set; }

        // [UPDATE] Configuration now accepts a list of items to create entities
        public List<PeChecklistItemDto>? ChecklistConfig { get; set; }

        public int? QuizId { get; set; }
        public int? PracticeId { get; set; }
    }

    public class UpdateFinalExamPartialDto
    {
        public decimal? ExamWeight { get; set; }
        public int? Duration { get; set; }
        public DateTime? StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public string? Description { get; set; }
        public int? QuizId { get; set; }
        public int? PracticeId { get; set; }
    }
    public class ValidateExamCodeDto
    {
        [Required]
        public string ExamCode { get; set; } = null!;
    }
    public class SubmitSeFinalDto
    {
        [Required(ErrorMessage = "Marks are required.")]
        [Range(0, 100, ErrorMessage = "Marks must be between 0 and 100.")] // Using 100 assuming the final score is normalized
        public decimal Marks { get; set; }

        [Required(ErrorMessage = "Pass/Fail status is required.")]
        public bool IsPass { get; set; }

        [StringLength(1000, ErrorMessage = "Description cannot exceed 1000 characters.")]
        public string? Description { get; set; }
        public DateTime? CompleteTime { get; set; }
    }
    // --- Submission DTOs ---

    public class SubmitTeDto
    {
        public List<QuizAnswerSubmissionDto> Answers { get; set; } = new List<QuizAnswerSubmissionDto>();
    }

    public class QuizAnswerSubmissionDto
    {
        public int QuestionId { get; set; }
        
        // Backward compatibility: single choice (optional)
        public int? OptionId { get; set; }
        
        // New: multiple choice support (optional)
        public List<int>? OptionIds { get; set; }
    }

    public class GetTeQuizRequestDto
    {
        [Required]
        public string ExamCode { get; set; } = null!;
    }

    public class SubmitSeDto
    {
        [Required]
        [Range(0, 10)]
        public decimal Marks { get; set; }
    }

    public class SubmitPeDto
    {
        [Required]
        public List<PeChecklistItemDto> Checklist { get; set; } = new List<PeChecklistItemDto>();

        public bool IsOverallPass { get; set; }
    }

    // [UPDATE] Refactored to match Entity, removed Score/MaxScore
    public class PeChecklistItemDto
    {
        public int? Id { get; set; } // ID of the PeChecklist entity (null if creating new template)
        public string Name { get; set; } = string.Empty; // Name/Criteria
        public string? Description { get; set; }
        public bool? IsPass { get; set; } // Null = Not graded yet, True/False = Graded
    }

    public class ClassExamConfigDto
    {
        public int ClassId { get; set; }
        public List<FinalExamPartialConfigDto> PartialConfigs { get; set; } = new List<FinalExamPartialConfigDto>();
    }

    public class FinalExamPartialConfigDto
    {
        public string Type { get; set; }
        public decimal? ExamWeight { get; set; }
        public int? Duration { get; set; }
        public DateTime? StartTime { get; set; }
        public DateTime? EndTime { get; set; }

        // Linked Info
        public int? QuizId { get; set; }
        public string? QuizName { get; set; }
        public int? PracticeId { get; set; }
        public string? PracticeName { get; set; }

        // PE Checklist
        public List<PeChecklistItemDto>? Checklist { get; set; }
    }

    // --- Allow Retake DTO ---
    public class AllowRetakeDto
    {
        public string? Note { get; set; }
    }
}