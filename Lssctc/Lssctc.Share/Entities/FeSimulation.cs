using System;
using System.Collections.Generic;

namespace Lssctc.Share.Entities;

public partial class FeSimulation
{
    public int Id { get; set; }

    public int FinalExamPartialId { get; set; }

    public int PracticeId { get; set; }

    public string? Name { get; set; }

    public string? Description { get; set; }

    public virtual FinalExamPartial FinalExamPartial { get; set; } = null!;

    public virtual Practice Practice { get; set; } = null!;

    public virtual ICollection<SeTask> SeTasks { get; set; } = new List<SeTask>();
}
