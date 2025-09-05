using System;
using System.Collections.Generic;

namespace InternalService.Domain.Entities;

public partial class User
{
    public int Id { get; set; }

    public string Username { get; set; } = null!;

    public string Password { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string Fullname { get; set; } = null!;

    public string? ProfileImageUrl { get; set; }

    public byte RoleId { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public bool? IsDeleted { get; set; }

    public virtual Admin? Admin { get; set; }

    public virtual ICollection<CourseEnrollment> CourseEnrollments { get; set; } = new List<CourseEnrollment>();

    public virtual Instructor? Instructor { get; set; }

    public virtual Learner? Learner { get; set; }

    public virtual Role Role { get; set; } = null!;

    public virtual Staff? Staff { get; set; }

    public virtual ICollection<Course> Courses { get; set; } = new List<Course>();

    public virtual ICollection<Course> CoursesNavigation { get; set; } = new List<Course>();
}
