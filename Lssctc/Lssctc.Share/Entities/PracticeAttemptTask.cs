using System;
using System.Collections.Generic;

namespace Lssctc.Share.Entities;

public partial class PracticeAttemptTask
{
    public int Id { get; set; }

    public int PracticeAttemptId { get; set; }

    public decimal? Score { get; set; }

    public string? Description { get; set; }

    public bool? IsPass { get; set; }

    public bool? IsDeleted { get; set; }

    public virtual PracticeAttempt PracticeAttempt { get; set; } = null!;
}
