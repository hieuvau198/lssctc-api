using System;
using System.Collections.Generic;

namespace Lssctc.Share.Entities;

public partial class ProgramCourse
{
    public int Id { get; set; }

    public int ProgramId { get; set; }

    public int CourseId { get; set; }

    public int CourseOrder { get; set; }

    public string? Name { get; set; }

    public string? Description { get; set; }

    public virtual ICollection<Class> Classes { get; set; } = new List<Class>();

    public virtual Course Course { get; set; } = null!;

    public virtual TrainingProgram Program { get; set; } = null!;
}
