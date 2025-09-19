using System;
using System.Collections.Generic;

namespace Lssctc.Share.Entities;

public partial class CourseSyllabuse
{
    public int Id { get; set; }

    public int CourseId { get; set; }

    public int SyllabusId { get; set; }

    public virtual Course Course { get; set; } = null!;

    public virtual Syllabuse Syllabus { get; set; } = null!;
}
