using System;
using System.Collections.Generic;

namespace Lssctc.Share.Entities;

public partial class SectionRecord
{
    public int Id { get; set; }

    public string? Name { get; set; }

    public int LearningProgressId { get; set; }

    public string? SectionName { get; set; }

    public bool IsCompleted { get; set; }

    public bool IsTraineeAttended { get; set; }

    public decimal? Progress { get; set; }

    public virtual ICollection<ActivityRecord> ActivityRecords { get; set; } = new List<ActivityRecord>();

    public virtual LearningProgress LearningProgress { get; set; } = null!;
}
