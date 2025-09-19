using System;
using System.Collections.Generic;

namespace Lssctc.Share.Entities;

public partial class Syllabuse
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public string CourseName { get; set; } = null!;

    public string CourseCode { get; set; } = null!;

    public string? Description { get; set; }

    public bool? IsActive { get; set; }

    public bool? IsDeleted { get; set; }

    public virtual ICollection<CourseSyllabuse> CourseSyllabuses { get; set; } = new List<CourseSyllabuse>();

    public virtual ICollection<SyllabusSection> SyllabusSections { get; set; } = new List<SyllabusSection>();
}
