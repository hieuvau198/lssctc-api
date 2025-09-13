using System;
using System.Collections.Generic;

namespace InternalService.Domain2.Entities;

public partial class SimulationComponent
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public string? ImageUrl { get; set; }

    public int? Status { get; set; }

    public DateTime CreatedDate { get; set; }

    public bool? IsDeleted { get; set; }

    public virtual ICollection<PracticeStepComponent> PracticeStepComponents { get; set; } = new List<PracticeStepComponent>();
}
