using System;
using System.Collections.Generic;

namespace Lssctc.Share.Entities;

public partial class TraineeCertificate
{
    public int Id { get; set; }

    public int EnrollmentId { get; set; }

    public int CourseCertificateId { get; set; }

    public DateTime? IssuedDate { get; set; }

    public string? CertificateCode { get; set; }

    public string? PdfUrl { get; set; }
        
    public virtual CourseCertificate CourseCertificate { get; set; } = null!;

    public virtual Enrollment Enrollment { get; set; } = null!;
}
