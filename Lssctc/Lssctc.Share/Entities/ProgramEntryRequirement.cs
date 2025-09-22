using System;
using System.Collections.Generic;

namespace Lssctc.Share.Entities;

public partial class ProgramEntryRequirement
{
    public int Id { get; set; }

    public int ProgramId { get; set; }

    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public string? DocumentUrl { get; set; }

    public virtual TrainingProgram Program { get; set; } = null!;
}
