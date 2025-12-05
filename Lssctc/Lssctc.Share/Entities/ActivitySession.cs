using System;
using System.Collections.Generic;

namespace Lssctc.Share.Entities;

public partial class ActivitySession
{
    public int Id { get; set; }

    public int ClassId { get; set; }

    public int ActivityId { get; set; }

    public bool? IsActive { get; set; }

    public DateTime? StartTime { get; set; }

    public DateTime? EndTime { get; set; }

    public virtual Activity Activity { get; set; } = null!;

    public virtual Class Class { get; set; } = null!;
}
