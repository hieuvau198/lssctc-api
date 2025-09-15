using System;
using System.Collections.Generic;

namespace Lssctc.Share.Entities;

public partial class TrainingResult
{
    public int Id { get; set; }

    public int TrainingResultTypeId { get; set; }

    public int TraineeId { get; set; }

    public string? ResultValue { get; set; }

    public DateTime ResultDate { get; set; }

    public string? Notes { get; set; }

    public virtual Trainee Trainee { get; set; } = null!;

    public virtual ICollection<TrainingProgress> TrainingProgresses { get; set; } = new List<TrainingProgress>();

    public virtual TrainingResultType TrainingResultType { get; set; } = null!;
}
