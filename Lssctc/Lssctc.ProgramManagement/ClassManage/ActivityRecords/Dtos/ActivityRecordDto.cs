using System.ComponentModel.DataAnnotations;

namespace Lssctc.ProgramManagement.ClassManage.ActivityRecords.Dtos
{
    public class ActivityRecordDto
    {
        public int Id { get; set; }
        public int SectionRecordId { get; set; }
        public int? ActivityId { get; set; }
        public string ActivityName { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public decimal? Score { get; set; }
        public bool? IsCompleted { get; set; }
        public DateTime? CompletedDate { get; set; }
        public string ActivityType { get; set; } = string.Empty;

        public int LearningProgressId { get; set; }
        public int? SectionId { get; set; }
        public int TraineeId { get; set; }
        public string TraineeName { get; set; } = string.Empty;
        public int ClassId { get; set; }
    }

    public class FeedbackDto
    {
        public int Id { get; set; }
        public int ActivityRecordId { get; set; }
        public int? InstructorId { get; set; }
        public string? FeedbackText { get; set; }
        public DateTime? CreatedDate { get; set; }
    }

    public class SubmitActivityRecordDto
    {
        [Required]
        public int ActivityRecordId { get; set; }
    }

    public class InstructorFeedbackDto
    {
        [Required]
        [StringLength(1000, MinimumLength = 10)]
        public string? FeedbackText { get; set; }
    }
}
