using System;
using System.Collections.Generic;

namespace Lssctc.Share.Entities;

public partial class TrainingProgram
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public bool? IsDeleted { get; set; }

    public bool? IsActive { get; set; }

    public int? DurationHours { get; set; }

    public int? TotalCourses { get; set; }

    public string? ImageUrl { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public string? BackgroundImageUrl { get; set; }

    public virtual ICollection<ProgramCourse> ProgramCourses { get; set; } = new List<ProgramCourse>();
}
