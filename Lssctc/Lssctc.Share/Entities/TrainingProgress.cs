using Lssctc.Share.Enum;
using System;
using System.Collections.Generic;

namespace Lssctc.Share.Entities;

public partial class TrainingProgress
{
    public int Id { get; set; }

    public int CourseMemberId { get; set; }

    public TrainingProgressStatus Status { get; set; }

    public double? ProgressPercentage { get; set; }

    public DateTime StartDate { get; set; }

    public DateTime? LastUpdated { get; set; }

    public string? Name { get; set; }

    public string? Description { get; set; }

    public virtual ClassMember CourseMember { get; set; } = null!;

    public virtual ICollection<LearningRecord> LearningRecords { get; set; } = new List<LearningRecord>();

    public virtual ICollection<TrainingResult> TrainingResults { get; set; } = new List<TrainingResult>();
}
