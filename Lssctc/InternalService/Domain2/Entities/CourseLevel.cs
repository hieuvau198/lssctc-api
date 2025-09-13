using System;
using System.Collections.Generic;

namespace InternalService.Domain2.Entities;

public partial class CourseLevel
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public virtual ICollection<Course> Courses { get; set; } = new List<Course>();
}
