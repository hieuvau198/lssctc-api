using System;
using System.Collections.Generic;

namespace InternalService.Domain2.Entities;

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

    public virtual ICollection<QuizQuestion> QuizQuestions { get; set; } = new List<QuizQuestion>();

    public virtual ICollection<SectionQuiz> SectionQuizzes { get; set; } = new List<SectionQuiz>();
}
