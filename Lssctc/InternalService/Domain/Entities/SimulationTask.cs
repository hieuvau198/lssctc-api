using System;
using System.Collections.Generic;

namespace InternalService.Domain.Entities;

public partial class SimulationTask
{
    public int Id { get; set; }

    public int CourseId { get; set; }

    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public string? PassingCriteria { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual Course Course { get; set; } = null!;

    public virtual ICollection<TrainingSessionSimulationTask> TrainingSessionSimulationTasks { get; set; } = new List<TrainingSessionSimulationTask>();
}
