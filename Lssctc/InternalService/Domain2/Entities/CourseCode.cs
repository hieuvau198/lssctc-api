using System;
using System.Collections.Generic;

namespace InternalService.Domain2.Entities;

public partial class CourseCode
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public virtual ICollection<Course> Courses { get; set; } = new List<Course>();
}
