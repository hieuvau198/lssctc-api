using System;
using System.Collections.Generic;

namespace InternalService.Domain2.Entities;

public partial class SectionQuiz
{
    public int Id { get; set; }

    public int QuizId { get; set; }

    public string Name { get; set; } = null!;

    public int SectionPartitionId { get; set; }

    public string? Description { get; set; }

    public virtual Quiz Quiz { get; set; } = null!;

    public virtual SectionPartition SectionPartition { get; set; } = null!;

    public virtual ICollection<SectionQuizAttempt> SectionQuizAttempts { get; set; } = new List<SectionQuizAttempt>();
}
