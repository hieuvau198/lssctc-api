using System;
using System.Collections.Generic;

namespace Lssctc.Share.Entities;

public partial class QuizAuthor
{
    public int Id { get; set; }

    public int InstructorId { get; set; }

    public int QuizId { get; set; }

    public virtual Instructor Instructor { get; set; } = null!;

    public virtual Quiz Quiz { get; set; } = null!;
}
