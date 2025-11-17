using System;
using System.Collections.Generic;

namespace Lssctc.Share.Entities;

public partial class MaterialAuthor
{
    public int Id { get; set; }

    public int InstructorId { get; set; }

    public int MaterialId { get; set; }

    public virtual Instructor Instructor { get; set; } = null!;

    public virtual LearningMaterial Material { get; set; } = null!;
}
