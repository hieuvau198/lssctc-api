using System;
using System.Collections.Generic;

namespace Lssctc.Share.Entities;

public partial class Task
{
    public int Id { get; set; }

    public string TaskName { get; set; } = null!;

    public string? TaskDescription { get; set; }

    public string? ExpectedResult { get; set; }

    public bool? IsDeleted { get; set; }

    public virtual ICollection<PracticeTask> PracticeTasks { get; set; } = new List<PracticeTask>();
}
