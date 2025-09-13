using System;
using System.Collections.Generic;

namespace InternalService.Domain2.Entities;

public partial class InstructorProfile
{
    public int Id { get; set; }

    public int? ExperienceYears { get; set; }

    public string? Biography { get; set; }

    public string? ProfessionalProfileUrl { get; set; }

    public string? Specialization { get; set; }

    public virtual Instructor IdNavigation { get; set; } = null!;
}
