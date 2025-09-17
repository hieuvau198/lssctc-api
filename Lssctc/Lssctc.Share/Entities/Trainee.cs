using System;
using System.Collections.Generic;

namespace Lssctc.Share.Entities;

public partial class Trainee
{
    public int Id { get; set; }

    public string TraineeCode { get; set; } = null!;

    public bool? IsActive { get; set; }

    public bool? IsDeleted { get; set; }

    public virtual ICollection<ClassEnrollment> ClassEnrollments { get; set; } = new List<ClassEnrollment>();

    public virtual ICollection<ClassMember> ClassMembers { get; set; } = new List<ClassMember>();

    public virtual User IdNavigation { get; set; } = null!;

    public virtual ICollection<Payment> Payments { get; set; } = new List<Payment>();

    public virtual ICollection<TraineeCertificate> TraineeCertificates { get; set; } = new List<TraineeCertificate>();

    public virtual TraineeProfile? TraineeProfile { get; set; }
}
