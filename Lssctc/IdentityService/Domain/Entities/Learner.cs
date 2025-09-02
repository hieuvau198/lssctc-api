using System;
using System.Collections.Generic;

namespace IdentityService.Domain.Entities;

public partial class Learner
{
    public int UserId { get; set; }

    public DateOnly? DateOfBirth { get; set; }

    public string? EnrollmentStatus { get; set; }

    public bool? IsDeleted { get; set; }

    public virtual ICollection<LearnerSimulationTask> LearnerSimulationTasks { get; set; } = new List<LearnerSimulationTask>();

    public virtual ICollection<LearnerTest> LearnerTests { get; set; } = new List<LearnerTest>();

    public virtual ICollection<SessionAttendance> SessionAttendances { get; set; } = new List<SessionAttendance>();

    public virtual ICollection<SessionLearner> SessionLearners { get; set; } = new List<SessionLearner>();

    public virtual User User { get; set; } = null!;
}
