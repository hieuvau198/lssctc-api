using System;
using System.Collections.Generic;

namespace Lssctc.Share.Entities;

public partial class QuizAttempt
{
    public int Id { get; set; }

    public int ActivityRecordId { get; set; }

    public int? QuizId { get; set; }

    public string Name { get; set; } = null!;

    public decimal? AttemptScore { get; set; }

    public decimal? MaxScore { get; set; }

    public DateTime QuizAttemptDate { get; set; }

    public int Status { get; set; }

    public int? AttemptOrder { get; set; }

    public bool? IsPass { get; set; }

    public virtual ActivityRecord ActivityRecord { get; set; } = null!;

    public virtual ICollection<QuizAttemptQuestion> QuizAttemptQuestions { get; set; } = new List<QuizAttemptQuestion>();
}
