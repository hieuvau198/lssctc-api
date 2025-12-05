using System;
using System.Collections.Generic;

namespace Lssctc.Share.Entities;

public partial class Attendance
{
    public int Id { get; set; }

    public int EnrollmentId { get; set; }

    public int TimeslotId { get; set; }

    public string? Name { get; set; }

    public string? Description { get; set; }

    public int? Status { get; set; }

    public bool? IsActive { get; set; }

    public bool? IsDeleted { get; set; }

    public virtual Enrollment Enrollment { get; set; } = null!;

    public virtual Timeslot Timeslot { get; set; } = null!;
}
