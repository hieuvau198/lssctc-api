using Lssctc.Share.Enum;
using System;
using System.Collections.Generic;

namespace Lssctc.Share.Entities;

public partial class ClassEnrollment
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public DateTime CreatedDate { get; set; }

    public DateTime? ApprovedDate { get; set; }

    public int ClassId { get; set; }

    public int TraineeId { get; set; }

    public string? TraineeContact { get; set; }

    public string Description { get; set; } = null!;

    public ClassEnrollmentStatus Status { get; set; }

    public virtual Class Class { get; set; } = null!;

    public virtual Trainee Trainee { get; set; } = null!;
}
