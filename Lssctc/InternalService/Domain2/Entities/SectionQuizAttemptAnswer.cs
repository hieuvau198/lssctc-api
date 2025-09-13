using System;
using System.Collections.Generic;

namespace InternalService.Domain2.Entities;

public partial class SectionQuizAttemptAnswer
{
    public int Id { get; set; }

    public int SectionQuizAttemptQuestionId { get; set; }

    public decimal? AttemptScore { get; set; }

    public bool IsCorrect { get; set; }

    public string? Description { get; set; }

    public string Name { get; set; } = null!;

    public virtual SectionQuizAttemptQuestion SectionQuizAttemptQuestion { get; set; } = null!;
}
