using System;
using System.Collections.Generic;

namespace IdentityService.Domain.Entities;

public partial class Instructor
{
    public int UserId { get; set; }

    public string? Bio { get; set; }

    public string? Specialization { get; set; }

    public int? YearsExperience { get; set; }

    public bool? IsDeleted { get; set; }

    public virtual ICollection<TrainingSession> TrainingSessions { get; set; } = new List<TrainingSession>();

    public virtual User User { get; set; } = null!;
}
