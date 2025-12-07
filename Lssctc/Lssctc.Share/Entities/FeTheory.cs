using System;
using System.Collections.Generic;

namespace Lssctc.Share.Entities;

public partial class FeTheory
{
    public int Id { get; set; }

    public int FinalExamPartialId { get; set; }

    public int QuizId { get; set; }

    public string? Name { get; set; }

    public string? Description { get; set; }

    public virtual FinalExamPartial FinalExamPartial { get; set; } = null!;

    public virtual Quiz Quiz { get; set; } = null!;
}
