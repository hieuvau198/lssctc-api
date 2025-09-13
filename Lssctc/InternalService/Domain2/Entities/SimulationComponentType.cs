using System;
using System.Collections.Generic;

namespace InternalService.Domain2.Entities;

public partial class SimulationComponentType
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public bool? IsDeleted { get; set; }
}
