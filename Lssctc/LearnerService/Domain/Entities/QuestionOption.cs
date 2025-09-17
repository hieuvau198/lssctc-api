using System;
using System.Collections.Generic;

namespace LearnerService.Domain.Entities;

public partial class QuestionOption
{
    public int Id { get; set; }

    public int QuestionId { get; set; }

    public bool? IsCorrect { get; set; }

    public decimal? Points { get; set; }

    public int OrderIndex { get; set; }

    public bool? IsDeleted { get; set; }

    public virtual ICollection<LearnerQuestionAnswer> LearnerQuestionAnswers { get; set; } = new List<LearnerQuestionAnswer>();

    public virtual TestQuestion Question { get; set; } = null!;
}
