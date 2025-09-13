using System;
using System.Collections.Generic;

namespace InternalService.Domain2.Entities;

public partial class QuizQuestion
{
    public int Id { get; set; }

    public int QuizId { get; set; }

    public string Name { get; set; } = null!;

    public decimal? QuestionScore { get; set; }

    public string? Description { get; set; }

    public bool IsMultipleAnswers { get; set; }

    public virtual Quiz Quiz { get; set; } = null!;

    public virtual ICollection<QuizQuestionOption> QuizQuestionOptions { get; set; } = new List<QuizQuestionOption>();
}
