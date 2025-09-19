using System;
using System.Collections.Generic;

namespace Lssctc.Share.Entities;

public partial class Certificate
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public int? EffectiveTime { get; set; }

    public string? Requirement { get; set; }

    public string? CertifyingAuthority { get; set; }

    public string? ImageUrl { get; set; }

    public virtual ICollection<CourseCertificate> CourseCertificates { get; set; } = new List<CourseCertificate>();
}
