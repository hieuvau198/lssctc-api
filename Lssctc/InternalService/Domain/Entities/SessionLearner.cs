using System;
using System.Collections.Generic;

namespace InternalService.Domain.Entities;

public partial class SessionLearner
{
    public int Id { get; set; }

    public int TrainingsessionId { get; set; }

    public int LearnerId { get; set; }

    public string? EnrollmentStatus { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual Learner Learner { get; set; } = null!;

    public virtual TrainingSession Trainingsession { get; set; } = null!;
}
