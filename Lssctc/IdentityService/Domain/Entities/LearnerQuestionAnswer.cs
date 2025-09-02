using System;
using System.Collections.Generic;

namespace IdentityService.Domain.Entities;

public partial class LearnerQuestionAnswer
{
    public int Id { get; set; }

    public int LearnerTestQuestionId { get; set; }

    public int OptionId { get; set; }

    public string? AnswerText { get; set; }

    public bool? IsCorrect { get; set; }

    public decimal? Points { get; set; }

    public bool? IsDeleted { get; set; }

    public virtual LearnerTestQuestion LearnerTestQuestion { get; set; } = null!;

    public virtual QuestionOption Option { get; set; } = null!;
}
