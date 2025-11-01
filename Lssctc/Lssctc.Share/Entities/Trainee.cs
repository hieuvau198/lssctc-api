using System;
using System.Collections.Generic;

namespace Lssctc.Share.Entities;

public partial class Trainee
{
    public int Id { get; set; }

    public string TraineeCode { get; set; } = null!;

    public bool? IsActive { get; set; }

    public bool? IsDeleted { get; set; }

    public virtual ICollection<Enrollment> Enrollments { get; set; } = new List<Enrollment>();

    public virtual User IdNavigation { get; set; } = null!;

    public virtual TraineeProfile? TraineeProfile { get; set; }
}
