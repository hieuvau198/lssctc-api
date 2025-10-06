using System;
using System.Collections.Generic;

namespace Lssctc.Share.Entities;

public partial class SimulationTimeslot
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public DateTime StartTime { get; set; }

    public DateTime EndTime { get; set; }

    public string? Note { get; set; }

    public int? Status { get; set; }

    public bool? IsActive { get; set; }

    public bool? IsDeleted { get; set; }

    public virtual ICollection<SectionPracticeTimeslot> SectionPracticeTimeslots { get; set; } = new List<SectionPracticeTimeslot>();
}
