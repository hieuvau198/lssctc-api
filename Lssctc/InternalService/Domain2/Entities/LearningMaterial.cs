using System;
using System.Collections.Generic;

namespace InternalService.Domain2.Entities;

public partial class LearningMaterial
{
    public int Id { get; set; }

    public int LearningMaterialTypeId { get; set; }

    public string Name { get; set; } = null!;

    public string Description { get; set; } = null!;

    public string MaterialUrl { get; set; } = null!;

    public virtual LearningMaterialType LearningMaterialType { get; set; } = null!;

    public virtual ICollection<SectionMaterial> SectionMaterials { get; set; } = new List<SectionMaterial>();
}
