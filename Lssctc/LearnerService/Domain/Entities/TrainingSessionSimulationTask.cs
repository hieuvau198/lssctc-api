using System;
using System.Collections.Generic;

namespace LearnerService.Domain.Entities;

public partial class TrainingSessionSimulationTask
{
    public int Id { get; set; }

    public int TrainingSessionId { get; set; }

    public int SimulationTaskId { get; set; }

    public DateTime? AssignedAt { get; set; }

    public virtual ICollection<LearnerSimulationTask> LearnerSimulationTasks { get; set; } = new List<LearnerSimulationTask>();

    public virtual SimulationTask SimulationTask { get; set; } = null!;

    public virtual TrainingSession TrainingSession { get; set; } = null!;
}
