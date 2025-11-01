using System;
using System.Collections.Generic;

namespace Lssctc.Share.Entities;

public partial class ActivityPractice
{
    public int Id { get; set; }

    public int ActivityId { get; set; }

    public int PracticeId { get; set; }

    public DateTime? CustomDeadline { get; set; }

    public string? CustomDescription { get; set; }

    public int? Status { get; set; }

    public bool? IsActive { get; set; }

    public bool? IsDeleted { get; set; }

    public virtual Activity Activity { get; set; } = null!;

    public virtual Practice Practice { get; set; } = null!;
}
