using System.ComponentModel.DataAnnotations;

namespace Lssctc.ProgramManagement.ClassManage.PracticeAttempts.Dtos
{
    public class CreatePracticeAttemptTaskWithCodeDto
    {
        [Required(ErrorMessage = "TaskCode is required.")]
        public string TaskCode { get; set; } = null!;

        [Range(0, 100, ErrorMessage = "Score must be between 0 and 100.")]
        public decimal? Score { get; set; }

        [StringLength(1000, ErrorMessage = "Description cannot exceed 1000 characters.")]
        public string? Description { get; set; }

        [Required(ErrorMessage = "IsPass is required for each task.")]
        public bool? IsPass { get; set; }
    }
}
