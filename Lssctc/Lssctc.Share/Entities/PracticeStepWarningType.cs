using System;
using System.Collections.Generic;

namespace Lssctc.Share.Entities;

public partial class PracticeStepWarningType
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public bool? IsFailedCriteria { get; set; }

    public virtual ICollection<PracticeStepWarning> PracticeStepWarnings { get; set; } = new List<PracticeStepWarning>();
}
