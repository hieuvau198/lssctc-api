using System;
using System.Collections.Generic;

namespace LearnerService.Domain.Entities;

public partial class CourseEnrollment
{
    public int Id { get; set; }

    public int CourseId { get; set; }

    public int UserId { get; set; }

    public DateTime? EnrollmentDate { get; set; }

    public string? Status { get; set; }

    public bool? IsDeleted { get; set; }

    public virtual Course Course { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}
