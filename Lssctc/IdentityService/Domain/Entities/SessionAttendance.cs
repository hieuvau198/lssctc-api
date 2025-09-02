using System;
using System.Collections.Generic;

namespace IdentityService.Domain.Entities;

public partial class SessionAttendance
{
    public int Id { get; set; }

    public int TrainingsessionId { get; set; }

    public int LearnerId { get; set; }

    public string? AttendanceStatus { get; set; }

    public DateTime? AttendedAt { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual Learner Learner { get; set; } = null!;

    public virtual TrainingSession Trainingsession { get; set; } = null!;
}
