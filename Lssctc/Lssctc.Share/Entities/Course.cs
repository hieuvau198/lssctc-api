using System;
using System.Collections.Generic;

namespace Lssctc.Share.Entities;

public partial class Course
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public int? CategoryId { get; set; }

    public int? LevelId { get; set; }

    public decimal? Price { get; set; }

    public bool? IsActive { get; set; }

    public bool? IsDeleted { get; set; }

    public string? ImageUrl { get; set; }

    public int? DurationHours { get; set; }

    public int? CourseCodeId { get; set; }

    public virtual CourseCategory? Category { get; set; }

    public virtual CourseCode? CourseCode { get; set; }

    public virtual CourseLevel? Level { get; set; }

    public virtual ICollection<ProgramCourse> ProgramCourses { get; set; } = new List<ProgramCourse>();
}
