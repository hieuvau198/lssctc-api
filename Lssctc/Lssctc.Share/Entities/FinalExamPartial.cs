using System;
using System.Collections.Generic;

namespace Lssctc.Share.Entities;

public partial class FinalExamPartial
{
    public int Id { get; set; }

    public int FinalExamId { get; set; }

    public bool? IsPass { get; set; }

    public int? Type { get; set; }

    public decimal? Marks { get; set; }

    public string? Description { get; set; }

    public int? Duration { get; set; }

    public DateTime? StartTime { get; set; }

    public DateTime? EndTime { get; set; }

    public DateTime? CompleteTime { get; set; }

    public virtual FinalExam FinalExam { get; set; } = null!;
}
