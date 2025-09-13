using System;
using System.Collections.Generic;

namespace InternalService.Domain2.Entities;

public partial class SectionPartitionType
{
    public int Id { get; set; }

    public string? Description { get; set; }

    public string Name { get; set; } = null!;

    public string PassCriteria { get; set; } = null!;

    public bool? IsActionRequired { get; set; }

    public virtual ICollection<SectionPartition> SectionPartitions { get; set; } = new List<SectionPartition>();
}
