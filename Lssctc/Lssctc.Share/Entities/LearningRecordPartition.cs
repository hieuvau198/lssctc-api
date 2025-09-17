using System;
using System.Collections.Generic;

namespace Lssctc.Share.Entities;

public partial class LearningRecordPartition
{
    public int Id { get; set; }

    public int SectionPartitionId { get; set; }

    public string? Name { get; set; }

    public int LearningRecordId { get; set; }

    public string? Description { get; set; }

    public DateTime UpdatedAt { get; set; }

    public DateTime CompletedAt { get; set; }

    public DateTime StartedAt { get; set; }

    public bool IsComplete { get; set; }

    public int RecordPartitionOrder { get; set; }

    public virtual LearningRecord LearningRecord { get; set; } = null!;

    public virtual SectionPartition SectionPartition { get; set; } = null!;
}
