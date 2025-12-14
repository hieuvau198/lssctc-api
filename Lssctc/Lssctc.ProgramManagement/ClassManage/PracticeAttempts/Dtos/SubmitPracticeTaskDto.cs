using System.ComponentModel.DataAnnotations;

namespace Lssctc.ProgramManagement.ClassManage.PracticeAttempts.Dtos
{
    public class SubmitPracticeTaskDto
    {
        [Required]
        public int ActivityRecordId { get; set; }

        [Required]
        public string TaskCode { get; set; } = null!;

        public decimal? Score { get; set; }
        public int? Mistakes { get; set; }
        public string? Description { get; set; }
        public bool? IsPass { get; set; }
    }
}
