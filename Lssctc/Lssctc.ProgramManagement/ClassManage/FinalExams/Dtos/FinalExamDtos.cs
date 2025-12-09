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

    // --- Final Exam Partial DTOs ---
    public class FinalExamPartialDto
    {
        public int Id { get; set; }
        public string? Type { get; set; } // "Theory", "Simulation", "Practical"
        public decimal? Marks { get; set; }
        public decimal? ExamWeight { get; set; }
        public string? Description { get; set; } // JSON Checklist for PE
        public int? Duration { get; set; }
        public DateTime? StartTime { get; set; } 
        public DateTime? EndTime { get; set; } 
        public DateTime? CompleteTime { get; set; }
        public string? Status { get; set; } // "NotYet", "Submitted", "Approved"

        // Linked Info
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

    // --- Submission DTOs ---

    public class SubmitTeDto
    {
        public List<QuizAnswerSubmissionDto> Answers { get; set; } = new List<QuizAnswerSubmissionDto>();
    }

    public class QuizAnswerSubmissionDto
    {
        public int QuestionId { get; set; }
        public int OptionId { get; set; }
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

    public class PeChecklistItemDto
    {
        public string Criteria { get; set; } = string.Empty;
        public decimal Score { get; set; }
        public decimal MaxScore { get; set; } = 10;
        public bool IsItemPass { get; set; } = true;
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
}