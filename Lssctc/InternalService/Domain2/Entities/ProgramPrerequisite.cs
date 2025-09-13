using System;
using System.Collections.Generic;

namespace InternalService.Domain2.Entities;

public partial class ProgramPrerequisite
{
    public int Id { get; set; }

    public int ProgramId { get; set; }

    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public virtual TrainingProgram Program { get; set; } = null!;
}
