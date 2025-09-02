using System;
using System.Collections.Generic;

namespace IdentityService.Domain.Entities;

public partial class LearnerTest
{
    public int Id { get; set; }

    public int LearnerId { get; set; }

    public int TestId { get; set; }

    public int? AttemptNumber { get; set; }

    public decimal? Score { get; set; }

    public DateTime? StartedAt { get; set; }

    public DateTime? CompletedAt { get; set; }

    public string? Status { get; set; }

    public string? Review { get; set; }

    public bool? IsDeleted { get; set; }

    public virtual Learner Learner { get; set; } = null!;

    public virtual ICollection<LearnerTestQuestion> LearnerTestQuestions { get; set; } = new List<LearnerTestQuestion>();

    public virtual Test Test { get; set; } = null!;
}
