using System;
using System.Collections.Generic;

namespace Lssctc.Share.Entities;

public partial class TransactionProgram
{
    public int Id { get; set; }

    public int TransactionId { get; set; }

    public int ProgramId { get; set; }

    public virtual TrainingProgram Program { get; set; } = null!;

    public virtual Transaction Transaction { get; set; } = null!;
}
