using System;
using System.Collections.Generic;

namespace Lssctc.Share.Entities;

public partial class PracticeStepAction
{
    public int Id { get; set; }

    public int StepId { get; set; }

    public int ActionId { get; set; }

    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public bool? IsDeleted { get; set; }

    public virtual SimAction Action { get; set; } = null!;

    public virtual PracticeStep Step { get; set; } = null!;
}
