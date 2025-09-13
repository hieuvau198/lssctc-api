using System;
using System.Collections.Generic;

namespace InternalService.Domain2.Entities;

public partial class SectionMaterial
{
    public int Id { get; set; }

    public int SectionPartitionId { get; set; }

    public int LearningMaterialId { get; set; }

    public string Name { get; set; } = null!;

    public string Description { get; set; } = null!;

    public virtual LearningMaterial LearningMaterial { get; set; } = null!;

    public virtual SectionPartition SectionPartition { get; set; } = null!;
}
