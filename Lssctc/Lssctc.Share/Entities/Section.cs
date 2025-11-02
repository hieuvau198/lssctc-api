using System;
using System.Collections.Generic;

namespace Lssctc.Share.Entities;

public partial class Section
{
    public int Id { get; set; }

    public string SectionTitle { get; set; } = null!;

    public string? SectionDescription { get; set; }

    public int? EstimatedDurationMinutes { get; set; }

    public bool? IsDeleted { get; set; }

    public virtual ICollection<CourseSection> CourseSections { get; set; } = new List<CourseSection>();

    public virtual ICollection<SectionActivity> SectionActivities { get; set; } = new List<SectionActivity>();
}
