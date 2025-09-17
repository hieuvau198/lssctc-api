using System;
using System.Collections.Generic;

namespace Lssctc.Share.Entities;

public partial class ClassInstructor
{
    public int Id { get; set; }

    public int ClassId { get; set; }

    public int InstructorId { get; set; }

    public string Position { get; set; } = null!;

    public virtual Class Class { get; set; } = null!;

    public virtual Instructor Instructor { get; set; } = null!;
}
