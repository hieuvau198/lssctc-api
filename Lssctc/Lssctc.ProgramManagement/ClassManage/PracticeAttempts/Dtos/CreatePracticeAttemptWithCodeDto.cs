using System.ComponentModel.DataAnnotations;

namespace Lssctc.ProgramManagement.ClassManage.PracticeAttempts.Dtos
{
    public class CreatePracticeAttemptWithCodeDto
    {
        [Required(ErrorMessage = "ActivityRecordId is required.")]
        public int ActivityRecordId { get; set; }

        [Required(ErrorMessage = "ClassId is required.")]
        [Range(1, int.MaxValue, ErrorMessage = "ClassId must be greater than 0.")]
        public int ClassId { get; set; }

        [Required(ErrorMessage = "PracticeCode is required.")]
        public string PracticeCode { get; set; } = null!;

        public decimal? Score { get; set; }

        [StringLength(1000, ErrorMessage = "Description cannot exceed 1000 characters.")]
        public string? Description { get; set; }

        public bool? IsPass { get; set; }

        // New fields for Update 2
        public int? TotalMistakes { get; set; }
        public DateTime? StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public int? DurationSeconds { get; set; }

        [Required(ErrorMessage = "At least one practice attempt task is required.")]
        [MinLength(1, ErrorMessage = "At least one practice attempt task is required.")]
        public List<CreatePracticeAttemptTaskWithCodeDto> PracticeAttemptTasks { get; set; } = new List<CreatePracticeAttemptTaskWithCodeDto>();
    }
}
