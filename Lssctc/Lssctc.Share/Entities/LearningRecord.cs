using System;
using System.Collections.Generic;

namespace Lssctc.Share.Entities;

public partial class LearningRecord
{
    public int Id { get; set; }

    public int SectionId { get; set; }

    public string? Name { get; set; }

    public int ClassMemberId { get; set; }

    public string? SectionName { get; set; }

    public bool IsCompleted { get; set; }

    public bool IsTraineeAttended { get; set; }

    public decimal? Progress { get; set; }

    public virtual ClassMember ClassMember { get; set; } = null!;

    public virtual ICollection<LearningRecordPartition> LearningRecordPartitions { get; set; } = new List<LearningRecordPartition>();

    public virtual Section Section { get; set; } = null!;
}
