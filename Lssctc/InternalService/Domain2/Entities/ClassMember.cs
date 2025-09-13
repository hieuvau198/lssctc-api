using System;
using System.Collections.Generic;

namespace InternalService.Domain2.Entities;

public partial class ClassMember
{
    public int Id { get; set; }

    public int TraineeId { get; set; }

    public int ClassId { get; set; }

    public DateTime AssignedDate { get; set; }

    public int Status { get; set; }

    public virtual Class Class { get; set; } = null!;

    public virtual ICollection<LearningRecord> LearningRecords { get; set; } = new List<LearningRecord>();

    public virtual Trainee Trainee { get; set; } = null!;

    public virtual ICollection<TrainingProgress> TrainingProgresses { get; set; } = new List<TrainingProgress>();
}
