using System;
using System.Collections.Generic;

namespace Lssctc.Share.Entities;

public partial class PracticeAttempt
{
    public int Id { get; set; }

    public int ActivityRecordId { get; set; }

    public int? PracticeId { get; set; }

    public decimal? Score { get; set; }

    public DateTime AttemptDate { get; set; }

    public int? AttemptStatus { get; set; }

    public string? Description { get; set; }

    public bool? IsPass { get; set; }

    public bool? IsDeleted { get; set; }

    public bool IsCurrent { get; set; }

    public virtual ActivityRecord ActivityRecord { get; set; } = null!;

    public virtual ICollection<PracticeAttemptTask> PracticeAttemptTasks { get; set; } = new List<PracticeAttemptTask>();
}
