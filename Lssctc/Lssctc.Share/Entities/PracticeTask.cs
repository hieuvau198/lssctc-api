using System;
using System.Collections.Generic;

namespace Lssctc.Share.Entities;

public partial class PracticeTask
{
    public int Id { get; set; }

    public int PracticeId { get; set; }

    public int TaskId { get; set; }

    public int Status { get; set; }

    public virtual Practice Practice { get; set; } = null!;

    public virtual Task Task { get; set; } = null!;
}
