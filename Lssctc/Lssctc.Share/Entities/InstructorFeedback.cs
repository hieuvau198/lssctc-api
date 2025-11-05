using System;
using System.Collections.Generic;

namespace Lssctc.Share.Entities;

public partial class InstructorFeedback
{
    public int Id { get; set; }

    public int ActivityRecordId { get; set; }

    public int? InstructorId { get; set; }

    public string? FeedbackText { get; set; }

    public DateTime? CreatedDate { get; set; }

    public virtual ActivityRecord ActivityRecord { get; set; } = null!;
}
