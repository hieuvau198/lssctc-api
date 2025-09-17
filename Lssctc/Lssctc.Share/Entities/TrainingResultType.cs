using System;
using System.Collections.Generic;

namespace Lssctc.Share.Entities;

public partial class TrainingResultType
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public virtual ICollection<TrainingResult> TrainingResults { get; set; } = new List<TrainingResult>();
}
