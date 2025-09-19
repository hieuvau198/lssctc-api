using System;
using System.Collections.Generic;

namespace Lssctc.Share.Entities;

public partial class SectionPracticeTimeslot
{
    public int Id { get; set; }

    public int SectionPracticeId { get; set; }

    public int SimulationTimeslotId { get; set; }

    public string? Note { get; set; }

    public virtual SectionPractice SectionPractice { get; set; } = null!;

    public virtual SimulationTimeslot SimulationTimeslot { get; set; } = null!;
}
