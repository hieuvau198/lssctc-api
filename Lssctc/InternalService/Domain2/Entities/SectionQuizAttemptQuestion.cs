using System;
using System.Collections.Generic;

namespace InternalService.Domain2.Entities;

public partial class SectionQuizAttemptQuestion
{
    public int Id { get; set; }

    public int SectionQuizAttemptId { get; set; }

    public decimal? AttemptScore { get; set; }

    public decimal? QuestionScore { get; set; }

    public bool IsCorrect { get; set; }

    public bool IsMultipleAnswers { get; set; }

    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public virtual SectionQuizAttempt SectionQuizAttempt { get; set; } = null!;

    public virtual ICollection<SectionQuizAttemptAnswer> SectionQuizAttemptAnswers { get; set; } = new List<SectionQuizAttemptAnswer>();
}
