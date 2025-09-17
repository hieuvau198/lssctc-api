using System;
using System.Collections.Generic;

namespace LearnerService.Domain.Entities;

public partial class LearningMaterial
{
    public int Id { get; set; }

    public string Title { get; set; } = null!;

    public string? Content { get; set; }

    public string? SourceUrl { get; set; }

    public int MaterialTypeId { get; set; }

    public bool? IsDeleted { get; set; }

    public virtual MaterialType MaterialType { get; set; } = null!;

    public virtual ICollection<Course> Courses { get; set; } = new List<Course>();
}
