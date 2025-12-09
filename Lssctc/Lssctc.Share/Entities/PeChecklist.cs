using System;
using System.Collections.Generic;

namespace Lssctc.Share.Entities;

public partial class PeChecklist
{
    public int Id { get; set; }

    public int FinalExamPartialId { get; set; }

    public string? Name { get; set; }

    public string? Description { get; set; }

    public bool? IsPass { get; set; }

    public virtual FinalExamPartial FinalExamPartial { get; set; } = null!;
}
