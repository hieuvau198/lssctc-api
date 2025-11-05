using System;
using System.Collections.Generic;

namespace Lssctc.Share.Entities;

public partial class ActivityRecord
{
    public int Id { get; set; }

    public int SectionRecordId { get; set; }

    public int? ActivityId { get; set; }

    public int? Status { get; set; }

    public decimal? Score { get; set; }

    public bool? IsCompleted { get; set; }

    public DateTime? CompletedDate { get; set; }

    public int? ActivityType { get; set; }

    public virtual ICollection<InstructorFeedback> InstructorFeedbacks { get; set; } = new List<InstructorFeedback>();

    public virtual ICollection<PracticeAttempt> PracticeAttempts { get; set; } = new List<PracticeAttempt>();

    public virtual ICollection<QuizAttempt> QuizAttempts { get; set; } = new List<QuizAttempt>();

    public virtual SectionRecord SectionRecord { get; set; } = null!;
}
