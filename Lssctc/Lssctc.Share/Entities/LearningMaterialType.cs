using System;
using System.Collections.Generic;

namespace Lssctc.Share.Entities;

public partial class LearningMaterialType
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public virtual ICollection<LearningMaterial> LearningMaterials { get; set; } = new List<LearningMaterial>();
}
