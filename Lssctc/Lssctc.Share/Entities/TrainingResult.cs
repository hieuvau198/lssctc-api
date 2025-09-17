using System;
using System.Collections.Generic;

namespace Lssctc.Share.Entities;

public partial class TrainingResult
{
    public int Id { get; set; }

    public int TrainingResultTypeId { get; set; }

    public int TrainingProgressId { get; set; }

    public string? ResultValue { get; set; }

    public DateTime ResultDate { get; set; }

    public string? Notes { get; set; }

    public virtual TrainingProgress TrainingProgress { get; set; } = null!;

    public virtual TrainingResultType TrainingResultType { get; set; } = null!;
}
