using System;
using System.Collections.Generic;

namespace InternalService.Domain2.Entities;

public partial class ProgramCertificate
{
    public int Id { get; set; }

    public int? ProgramId { get; set; }

    public int? CertificateId { get; set; }

    public virtual Certificate? Certificate { get; set; }

    public virtual TrainingProgram? Program { get; set; }
}
