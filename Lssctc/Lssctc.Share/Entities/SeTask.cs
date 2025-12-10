using System;
using System.Collections.Generic;

namespace Lssctc.Share.Entities;

public partial class SeTask
{
    public int Id { get; set; }

    public int FeSimulationId { get; set; }

    public int SimTaskId { get; set; }

    public bool? IsPass { get; set; }

    public int? Status { get; set; }

    public string? Name { get; set; }

    public string? Description { get; set; }

    public DateTime? AttemptTime { get; set; }

    public DateTime? CompleteTime { get; set; }

    public int? DurationSecond { get; set; }

    public virtual FeSimulation FeSimulation { get; set; } = null!;

    public virtual SimTask SimTask { get; set; } = null!;
}
