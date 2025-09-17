using System;
using System.Collections.Generic;

namespace Lssctc.Share.Entities;

public partial class PracticeStepComponent
{
    public int Id { get; set; }

    public int StepId { get; set; }

    public int ComponentId { get; set; }

    public int ComponentOrder { get; set; }

    public bool? IsDeleted { get; set; }

    public virtual SimulationComponent Component { get; set; } = null!;

    public virtual PracticeStep Step { get; set; } = null!;
}
