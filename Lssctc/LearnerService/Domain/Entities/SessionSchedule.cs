using System;
using System.Collections.Generic;

namespace LearnerService.Domain.Entities;

public partial class SessionSchedule
{
    public int Id { get; set; }

    public int TrainingsessionId { get; set; }

    public DateOnly ScheduleDate { get; set; }

    public TimeOnly StartTime { get; set; }

    public TimeOnly EndTime { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual TrainingSession Trainingsession { get; set; } = null!;
}
