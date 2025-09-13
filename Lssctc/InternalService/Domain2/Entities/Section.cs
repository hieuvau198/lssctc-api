using System;
using System.Collections.Generic;

namespace InternalService.Domain2.Entities;

public partial class Section
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public int ClassesId { get; set; }

    public int? DurationMinutes { get; set; }

    public int Order { get; set; }

    public DateTime StartDate { get; set; }

    public DateTime? EndDate { get; set; }

    public int Status { get; set; }

    public virtual Class Classes { get; set; } = null!;

    public virtual ICollection<LearningRecord> LearningRecords { get; set; } = new List<LearningRecord>();

    public virtual ICollection<SectionPartition> SectionPartitions { get; set; } = new List<SectionPartition>();
}
