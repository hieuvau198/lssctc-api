using System;
using System.Collections.Generic;

namespace Lssctc.Share.Entities;

public partial class SimulationComponent
{
    public int Id { get; set; }

    public int BrandModelId { get; set; }

    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public string? ImageUrl { get; set; }

    public bool? IsActive { get; set; }

    public DateTime CreatedDate { get; set; }

    public bool? IsDeleted { get; set; }

    public string? ComponentCode { get; set; }

    public virtual BrandModel BrandModel { get; set; } = null!;
}
