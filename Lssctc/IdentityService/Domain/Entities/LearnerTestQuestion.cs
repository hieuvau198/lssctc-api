using System;
using System.Collections.Generic;

namespace IdentityService.Domain.Entities;

public partial class LearnerTestQuestion
{
    public int Id { get; set; }

    public int LearnerTestId { get; set; }

    public int QuestionId { get; set; }

    public int OrderIndex { get; set; }

    public decimal? Points { get; set; }

    public bool? IsDeleted { get; set; }

    public virtual ICollection<LearnerQuestionAnswer> LearnerQuestionAnswers { get; set; } = new List<LearnerQuestionAnswer>();

    public virtual LearnerTest LearnerTest { get; set; } = null!;

    public virtual TestQuestion Question { get; set; } = null!;
}
