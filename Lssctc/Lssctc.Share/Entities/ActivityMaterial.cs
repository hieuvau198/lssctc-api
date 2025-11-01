using System;
using System.Collections.Generic;

namespace Lssctc.Share.Entities;

public partial class ActivityMaterial
{
    public int Id { get; set; }

    public int ActivityId { get; set; }

    public int LearningMaterialId { get; set; }

    public string Name { get; set; } = null!;

    public string Description { get; set; } = null!;

    public virtual Activity Activity { get; set; } = null!;

    public virtual LearningMaterial LearningMaterial { get; set; } = null!;
}
