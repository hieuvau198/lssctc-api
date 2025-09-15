using System;
using System.Collections.Generic;

namespace Lssctc.Share.Entities;

public partial class TraineeCertificate
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public int? CertificateId { get; set; }

    public DateTime? ValidDateEnd { get; set; }

    public int? TraineeId { get; set; }

    public DateTime IssuedDateStart { get; set; }

    public DateTime? IssuedDateEnd { get; set; }

    public int Status { get; set; }

    public virtual Certificate? Certificate { get; set; }

    public virtual Trainee? Trainee { get; set; }
}
