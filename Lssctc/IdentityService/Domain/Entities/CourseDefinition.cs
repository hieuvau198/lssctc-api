using System;
using System.Collections.Generic;

namespace IdentityService.Domain.Entities;

public partial class CourseDefinition
{
    public int Id { get; set; }

    public string Title { get; set; } = null!;

    public string CourseCode { get; set; } = null!;

    public string? Description { get; set; }

    public int CategoryId { get; set; }

    public int LevelId { get; set; }

    public int? Duration { get; set; }

    public bool? IsDeleted { get; set; }

    public virtual CourseCategory Category { get; set; } = null!;

    public virtual ICollection<Course> Courses { get; set; } = new List<Course>();

    public virtual CourseLevel Level { get; set; } = null!;
}
