using System.ComponentModel.DataAnnotations;

namespace Lssctc.ProgramManagement.ClassManage.PracticeAttempts.Dtos
{
    public class PracticeAttemptDto
    {
        public int Id { get; set; }
        public int ActivityRecordId { get; set; }
        public int? PracticeId { get; set; }
        public string? PracticeCode { get; set; }
        public decimal? Score { get; set; }
        public DateTime AttemptDate { get; set; }
        public string? AttemptStatus { get; set; }
        public string? Description { get; set; }
        public bool? IsPass { get; set; }
        public bool IsCurrent { get; set; }

        // === ADDED FIELDS ===
        public int? TotalMistakes { get; set; }
        public DateTime? StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public int? DurationSeconds { get; set; }
        // ====================

        public List<PracticeAttemptTaskDto> PracticeAttemptTasks { get; set; } = new List<PracticeAttemptTaskDto>();
    }

    public class PracticeAttemptTaskDto
    {
        public int Id { get; set; }
        public int PracticeAttemptId { get; set; }
        public int? TaskId { get; set; }
        public string? TaskCode { get; set; }
        public decimal? Score { get; set; }

        // === ADDED FIELD ===
        public int? Mistakes { get; set; }
        // ===================

        public string? Description { get; set; }
        public bool? IsPass { get; set; }
    }

    public class CreatePracticeAttemptDto
    {
        [Required(ErrorMessage = "ClassId is required.")]
        [Range(1, int.MaxValue, ErrorMessage = "ClassId must be greater than 0.")]
        public int ClassId { get; set; }

        [Required(ErrorMessage = "PracticeId is required.")]
        [Range(1, int.MaxValue, ErrorMessage = "PracticeId must be greater than 0.")]
        public int PracticeId { get; set; }

        // Score is auto-calculated: 10 if all tasks pass, 0 if any task fails
        public decimal? Score { get; set; }

        [StringLength(1000, ErrorMessage = "Description cannot exceed 1000 characters.")]
        public string? Description { get; set; }

        // IsPass is auto-calculated based on all tasks passing
        public bool? IsPass { get; set; }

        [Required(ErrorMessage = "At least one practice attempt task is required.")]
        [MinLength(1, ErrorMessage = "At least one practice attempt task is required.")]
        public List<CreatePracticeAttemptTaskDto> PracticeAttemptTasks { get; set; } = new List<CreatePracticeAttemptTaskDto>();
    }

    public class CreatePracticeAttemptTaskDto
    {
        [Range(1, int.MaxValue, ErrorMessage = "TaskId must be greater than 0 when provided.")]
        public int? TaskId { get; set; }

        [Range(0, 100, ErrorMessage = "Score must be between 0 and 100.")]
        public decimal? Score { get; set; }

        [StringLength(1000, ErrorMessage = "Description cannot exceed 1000 characters.")]
        public string? Description { get; set; }

        [Required(ErrorMessage = "IsPass is required for each task.")]
        public bool? IsPass { get; set; }
    }
}
