using System;
using System.Collections.Generic;

namespace Lssctc.Share.Entities;

public partial class Enrollment
{
    public int Id { get; set; }

    public int ClassId { get; set; }

    public int TraineeId { get; set; }

    public DateTime? EnrollDate { get; set; }

    public int? Status { get; set; }

    public bool? IsActive { get; set; }

    public bool? IsDeleted { get; set; }

    public string? Note { get; set; }

    public virtual Class Class { get; set; } = null!;

    public virtual ICollection<LearningProgress> LearningProgresses { get; set; } = new List<LearningProgress>();

    public virtual Trainee Trainee { get; set; } = null!;

    public virtual ICollection<TraineeCertificate> TraineeCertificates { get; set; } = new List<TraineeCertificate>();
}
