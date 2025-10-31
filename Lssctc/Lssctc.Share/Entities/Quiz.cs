using System;
using System.Collections.Generic;

namespace Lssctc.Share.Entities;

public partial class Quiz
{
    public int Id { get; set; }

    public string? Name { get; set; }

    public decimal? PassScoreCriteria { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public int? TimelimitMinute { get; set; }

    public decimal? TotalScore { get; set; }

    public string? Description { get; set; }

    public virtual ICollection<ActivityQuiz> ActivityQuizzes { get; set; } = new List<ActivityQuiz>();

    public virtual ICollection<QuizQuestion> QuizQuestions { get; set; } = new List<QuizQuestion>();
}
