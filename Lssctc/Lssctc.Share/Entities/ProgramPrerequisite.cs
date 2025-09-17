using System;
using System.Collections.Generic;

namespace Lssctc.Share.Entities;

public partial class ProgramPrerequisite
{
    public int Id { get; set; }

    public int ProgramId { get; set; }

    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public virtual TrainingProgram Program { get; set; } = null!;
}
