using System;
using System.Collections.Generic;

namespace Lssctc.Share.Entities;

public partial class SectionPracticeAttemptStep
{
    public int Id { get; set; }

    public int AttemptId { get; set; }

    public int PracticeStepId { get; set; }

    public decimal? Score { get; set; }

    public string? Description { get; set; }

    public bool? IsPass { get; set; }

    public bool? IsDeleted { get; set; }

    public virtual SectionPracticeAttempt Attempt { get; set; } = null!;

    public virtual PracticeStep PracticeStep { get; set; } = null!;
}
