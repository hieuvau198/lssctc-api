using System;
using System.Collections.Generic;

namespace Lssctc.Share.Entities;

public partial class TrainingProgram
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public bool? IsDeleted { get; set; }

    public bool? IsActive { get; set; }

    public int? DurationHours { get; set; }

    public int? TotalCourses { get; set; }

    public string? ImageUrl { get; set; }

    public virtual ICollection<ProgramCertificate> ProgramCertificates { get; set; } = new List<ProgramCertificate>();

    public virtual ICollection<ProgramCourse> ProgramCourses { get; set; } = new List<ProgramCourse>();

    public virtual ICollection<ProgramPrerequisite> ProgramPrerequisites { get; set; } = new List<ProgramPrerequisite>();

    public virtual ICollection<TransactionProgram> TransactionPrograms { get; set; } = new List<TransactionProgram>();
}
