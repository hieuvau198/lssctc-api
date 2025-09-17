using System;
using System.Collections.Generic;

namespace Lssctc.Share.Entities;

public partial class CourseCertificate
{
    public int Id { get; set; }

    public int? CourseId { get; set; }

    public int? CertificateId { get; set; }

    public virtual Certificate? Certificate { get; set; }

    public virtual Course? Course { get; set; }

    public virtual ICollection<TraineeCertificate> TraineeCertificates { get; set; } = new List<TraineeCertificate>();
}
