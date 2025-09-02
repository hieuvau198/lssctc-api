using System;
using System.Collections.Generic;

namespace IdentityService.Domain.Entities;

public partial class TestQuestion
{
    public int Id { get; set; }

    public int TestId { get; set; }

    public bool? IsMultipleAnswers { get; set; }

    public int OptionQuantity { get; set; }

    public int AnswerQuantity { get; set; }

    public decimal? Points { get; set; }

    public string? Explanation { get; set; }

    public bool? IsDeleted { get; set; }

    public virtual ICollection<LearnerTestQuestion> LearnerTestQuestions { get; set; } = new List<LearnerTestQuestion>();

    public virtual ICollection<QuestionOption> QuestionOptions { get; set; } = new List<QuestionOption>();

    public virtual Test Test { get; set; } = null!;
}
