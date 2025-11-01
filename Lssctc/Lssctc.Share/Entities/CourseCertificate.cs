using System;
using System.Collections.Generic;

namespace Lssctc.Share.Entities;

public partial class CourseCertificate
{
    public int Id { get; set; }

    public int CourseId { get; set; }

    public int CertificateId { get; set; }

    public decimal? PassingScore { get; set; }

    public bool? IsActive { get; set; }

    public virtual Certificate Certificate { get; set; } = null!;

    public virtual Course Course { get; set; } = null!;

    public virtual ICollection<TraineeCertificate> TraineeCertificates { get; set; } = new List<TraineeCertificate>();
}
