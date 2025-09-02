using System;
using System.Collections.Generic;

namespace IdentityService.Domain.Entities;

public partial class CourseSession
{
    public int Id { get; set; }

    public int CourseId { get; set; }

    public int TrainingSessionId { get; set; }

    public virtual Course Course { get; set; } = null!;

    public virtual TrainingSession TrainingSession { get; set; } = null!;
}
