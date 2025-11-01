using System;
using System.Collections.Generic;

namespace Lssctc.Share.Entities;

public partial class CourseSection
{
    public int Id { get; set; }

    public int CourseId { get; set; }

    public int SectionId { get; set; }

    public virtual Course Course { get; set; } = null!;

    public virtual Section Section { get; set; } = null!;
}
