using System;
using System.Collections.Generic;

namespace Lssctc.Share.Entities;

public partial class Class
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public DateTime StartDate { get; set; }

    public DateTime? EndDate { get; set; }

    public int? Capacity { get; set; }

    public int ProgramCourseId { get; set; }

    public int? ClassCodeId { get; set; }

    public string Description { get; set; } = null!;

    public int? Status { get; set; }

    public string? BackgroundImageUrl { get; set; }

    public virtual ICollection<ActivitySession> ActivitySessions { get; set; } = new List<ActivitySession>();

    public virtual ClassCode? ClassCode { get; set; }

    public virtual ICollection<ClassInstructor> ClassInstructors { get; set; } = new List<ClassInstructor>();

    public virtual ICollection<Enrollment> Enrollments { get; set; } = new List<Enrollment>();

    public virtual ProgramCourse ProgramCourse { get; set; } = null!;

    public virtual ICollection<Timeslot> Timeslots { get; set; } = new List<Timeslot>();
}
