using System;
using System.Collections.Generic;

namespace Lssctc.Share.Entities;

public partial class ActivityQuiz
{
    public int Id { get; set; }

    public int QuizId { get; set; }

    public int ActivityId { get; set; }

    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public virtual Activity Activity { get; set; } = null!;

    public virtual Quiz Quiz { get; set; } = null!;
}
