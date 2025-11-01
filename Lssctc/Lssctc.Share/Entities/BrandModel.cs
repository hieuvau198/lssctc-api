using System;
using System.Collections.Generic;

namespace Lssctc.Share.Entities;

public partial class BrandModel
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public string? Manufacturer { get; set; }

    public string? CountryOfOrigin { get; set; }

    public bool? IsActive { get; set; }

    public bool? IsDeleted { get; set; }

    public virtual ICollection<SimulationComponent> SimulationComponents { get; set; } = new List<SimulationComponent>();
}
