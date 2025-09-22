using System;
using System.Collections.Generic;

namespace Lssctc.Share.Entities;

public partial class QuizQuestionOption
{
    public int Id { get; set; }

    public int QuizQuestionId { get; set; }

    public string? Description { get; set; }

    public bool IsCorrect { get; set; }

    public int? DisplayOrder { get; set; }

    public decimal? OptionScore { get; set; }

    public string Name { get; set; } = null!;

    public virtual QuizQuestion QuizQuestion { get; set; } = null!;
}
