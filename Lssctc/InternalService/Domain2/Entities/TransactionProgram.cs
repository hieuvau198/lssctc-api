using System;
using System.Collections.Generic;

namespace InternalService.Domain2.Entities;

public partial class TransactionProgram
{
    public int Id { get; set; }

    public int TransactionId { get; set; }

    public int ProgramId { get; set; }

    public virtual TrainingProgram Program { get; set; } = null!;

    public virtual Transaction Transaction { get; set; } = null!;
}
