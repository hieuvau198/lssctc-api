using System;
using System.Collections.Generic;

namespace Lssctc.Share.Entities;

public partial class Timeslot
{
    public int Id { get; set; }

    public int ClassId { get; set; }

    public string? Name { get; set; }

    public string? LocationDetail { get; set; }

    public string? LocationBuilding { get; set; }

    public string? LocationRoom { get; set; }

    public DateTime StartTime { get; set; }

    public DateTime EndTime { get; set; }

    public int? Status { get; set; }

    public bool? IsDeleted { get; set; }

    public virtual ICollection<Attendance> Attendances { get; set; } = new List<Attendance>();

    public virtual Class Class { get; set; } = null!;
}
