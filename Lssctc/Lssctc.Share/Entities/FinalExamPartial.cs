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

    public decimal? ExamWeight { get; set; }

    public int? Status { get; set; }

    public int TotalScore { get; set; }

    public virtual ICollection<FeSimulation> FeSimulations { get; set; } = new List<FeSimulation>();

    public virtual ICollection<FeTheory> FeTheories { get; set; } = new List<FeTheory>();

    public virtual FinalExam FinalExam { get; set; } = null!;

    public virtual ICollection<PeChecklist> PeChecklists { get; set; } = new List<PeChecklist>();
}
