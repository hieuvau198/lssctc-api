using System;
using System.Collections.Generic;

namespace LearnerService.Domain.Entities;

public partial class MaterialType
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public virtual ICollection<LearningMaterial> LearningMaterials { get; set; } = new List<LearningMaterial>();
}
