using System;
using System.Collections.Generic;

namespace Lssctc.Share.Entities;

public partial class PracticeStepWarning
{
    public int Id { get; set; }

    public int PracticeStepId { get; set; }

    public string? WarningMessage { get; set; }

    public int? WarningTypeId { get; set; }

    public string? Instruction { get; set; }

    public bool? IsDeleted { get; set; }

    public virtual PracticeStep PracticeStep { get; set; } = null!;

    public virtual PracticeStepWarningType? WarningType { get; set; }
}
