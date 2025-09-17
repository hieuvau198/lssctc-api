using System;
using System.Collections.Generic;

namespace Lssctc.Share.Entities;

public partial class SectionQuizAttempt
{
    public int Id { get; set; }

    public int SectionQuizId { get; set; }

    public string Name { get; set; } = null!;

    public decimal? AttemptScore { get; set; }

    public int TraineeId { get; set; }

    public decimal? MaxScore { get; set; }

    public DateTime QuizAttemptDate { get; set; }

    public int Status { get; set; }

    public int? AttemptOrder { get; set; }

    public virtual SectionQuiz SectionQuiz { get; set; } = null!;

    public virtual ICollection<SectionQuizAttemptQuestion> SectionQuizAttemptQuestions { get; set; } = new List<SectionQuizAttemptQuestion>();

    public virtual Trainee Trainee { get; set; } = null!;
}
