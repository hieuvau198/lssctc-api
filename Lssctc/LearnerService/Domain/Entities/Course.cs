using System;
using System.Collections.Generic;

namespace LearnerService.Domain.Entities;

public partial class Course
{
    public int Id { get; set; }

    public int CourseDefinitionId { get; set; }

    public string Title { get; set; } = null!;

    public string CourseCode { get; set; } = null!;

    public string? Description { get; set; }

    public string Category { get; set; } = null!;

    public string Level { get; set; } = null!;

    public DateOnly? StartDate { get; set; }

    public DateOnly? EndDate { get; set; }

    public string? Location { get; set; }

    public string? Status { get; set; }

    public bool? IsDeleted { get; set; }

    public virtual CourseDefinition CourseDefinition { get; set; } = null!;

    public virtual ICollection<CourseEnrollment> CourseEnrollments { get; set; } = new List<CourseEnrollment>();

    public virtual ICollection<CourseSession> CourseSessions { get; set; } = new List<CourseSession>();

    public virtual ICollection<SimulationTask> SimulationTasks { get; set; } = new List<SimulationTask>();

    public virtual ICollection<Course> Courses { get; set; } = new List<Course>();

    public virtual ICollection<LearningMaterial> Materials { get; set; } = new List<LearningMaterial>();

    public virtual ICollection<Course> Prerequisites { get; set; } = new List<Course>();

    public virtual ICollection<User> Users { get; set; } = new List<User>();

    public virtual ICollection<User> UsersNavigation { get; set; } = new List<User>();
}
