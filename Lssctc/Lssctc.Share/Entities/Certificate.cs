using System;
using System.Collections.Generic;

namespace Lssctc.Share.Entities;

public partial class Certificate
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public string? TemplateUrl { get; set; }

    public bool? IsActive { get; set; }

    public string? TemplateHtml { get; set; }

    public virtual ICollection<CourseCertificate> CourseCertificates { get; set; } = new List<CourseCertificate>();
}
