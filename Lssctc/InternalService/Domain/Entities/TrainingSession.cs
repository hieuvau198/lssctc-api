using System;
using System.Collections.Generic;

namespace InternalService.Domain.Entities;

public partial class TrainingSession
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public int SessionTypeId { get; set; }

    public int InstructorId { get; set; }

    public DateTime StartDate { get; set; }

    public DateTime EndDate { get; set; }

    public string? Location { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual ICollection<CourseSession> CourseSessions { get; set; } = new List<CourseSession>();

    public virtual Instructor Instructor { get; set; } = null!;

    public virtual ICollection<SessionAttendance> SessionAttendances { get; set; } = new List<SessionAttendance>();

    public virtual ICollection<SessionLearner> SessionLearners { get; set; } = new List<SessionLearner>();

    public virtual ICollection<SessionSchedule> SessionSchedules { get; set; } = new List<SessionSchedule>();

    public virtual SessionType SessionType { get; set; } = null!;

    public virtual ICollection<TrainingSessionSimulationTask> TrainingSessionSimulationTasks { get; set; } = new List<TrainingSessionSimulationTask>();
}
