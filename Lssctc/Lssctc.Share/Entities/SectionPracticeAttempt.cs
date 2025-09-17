using System;
using System.Collections.Generic;

namespace Lssctc.Share.Entities;

public partial class SectionPracticeAttempt
{
    public int Id { get; set; }

    public int SectionPracticeId { get; set; }

    public int TraineeId { get; set; }

    public decimal? Score { get; set; }

    public DateTime AttemptDate { get; set; }

    public int? AttemptStatus { get; set; }

    public string? Description { get; set; }

    public bool? IsPass { get; set; }

    public bool? IsDeleted { get; set; }

    public virtual SectionPractice SectionPractice { get; set; } = null!;

    public virtual ICollection<SectionPracticeAttemptStep> SectionPracticeAttemptSteps { get; set; } = new List<SectionPracticeAttemptStep>();

    public virtual Trainee Trainee { get; set; } = null!;
}
