using System;
using System.Collections.Generic;

namespace Lssctc.Share.Entities;

public partial class SimAction
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public string? ActionKey { get; set; }

    public bool? IsActive { get; set; }

    public bool? IsDeleted { get; set; }

    public virtual ICollection<PracticeStepAction> PracticeStepActions { get; set; } = new List<PracticeStepAction>();
}
