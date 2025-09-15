using System;
using System.Collections.Generic;

namespace Lssctc.Share.Entities;

public partial class PracticeStepType
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public bool? IsDeleted { get; set; }
}
