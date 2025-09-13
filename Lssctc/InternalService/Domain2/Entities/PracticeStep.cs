using System;
using System.Collections.Generic;

namespace InternalService.Domain2.Entities;

public partial class PracticeStep
{
    public int Id { get; set; }

    public int PracticeId { get; set; }

    public string StepName { get; set; } = null!;

    public string? StepDescription { get; set; }

    public string? ExpectedResult { get; set; }

    public int StepOrder { get; set; }

    public bool? IsDeleted { get; set; }

    public virtual Practice Practice { get; set; } = null!;

    public virtual ICollection<PracticeStepComponent> PracticeStepComponents { get; set; } = new List<PracticeStepComponent>();

    public virtual ICollection<PracticeStepWarning> PracticeStepWarnings { get; set; } = new List<PracticeStepWarning>();

    public virtual ICollection<SectionPracticeAttemptStep> SectionPracticeAttemptSteps { get; set; } = new List<SectionPracticeAttemptStep>();
}
