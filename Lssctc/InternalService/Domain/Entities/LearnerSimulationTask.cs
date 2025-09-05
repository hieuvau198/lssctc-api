using System;
using System.Collections.Generic;

namespace InternalService.Domain.Entities;

public partial class LearnerSimulationTask
{
    public int Id { get; set; }

    public int LearnerId { get; set; }

    public int TrainingSessionSimulationTaskId { get; set; }

    public string? Status { get; set; }

    public string? Progress { get; set; }

    public string? Feedback { get; set; }

    public DateTime? StartedAt { get; set; }

    public DateTime? CompletedAt { get; set; }

    public virtual Learner Learner { get; set; } = null!;

    public virtual TrainingSessionSimulationTask TrainingSessionSimulationTask { get; set; } = null!;
}
