using System;
using System.Collections.Generic;

namespace InternalService.Domain2.Entities;

public partial class LearningMaterialType
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public virtual ICollection<LearningMaterial> LearningMaterials { get; set; } = new List<LearningMaterial>();
}
