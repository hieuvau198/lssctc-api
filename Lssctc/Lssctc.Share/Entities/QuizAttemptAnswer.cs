using System;
using System.Collections.Generic;

namespace Lssctc.Share.Entities;

public partial class QuizAttemptAnswer
{
    public int Id { get; set; }

    public int QuizAttemptQuestionId { get; set; }

    public int? QuizOptionId { get; set; }

    public decimal? AttemptScore { get; set; }

    public bool IsCorrect { get; set; }

    public string? Description { get; set; }

    public string Name { get; set; } = null!;

    public virtual QuizAttemptQuestion QuizAttemptQuestion { get; set; } = null!;
}
