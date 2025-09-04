using System;
using System.Collections.Generic;

namespace LearnerService.Domain.Entities;

public partial class CourseCategory
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public virtual ICollection<CourseDefinition> CourseDefinitions { get; set; } = new List<CourseDefinition>();
}
