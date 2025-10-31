using System;
using System.Collections.Generic;

namespace Lssctc.Share.Entities;

public partial class LearningMaterial
{
    public int Id { get; set; }

    public string LearningMaterialType { get; set; } = null!;

    public string Name { get; set; } = null!;

    public string Description { get; set; } = null!;

    public string MaterialUrl { get; set; } = null!;

    public virtual ICollection<ActivityMaterial> ActivityMaterials { get; set; } = new List<ActivityMaterial>();
}
