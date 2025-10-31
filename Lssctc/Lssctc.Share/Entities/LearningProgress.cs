using System;
using System.Collections.Generic;

namespace Lssctc.Share.Entities;

public partial class LearningProgress
{
    public int Id { get; set; }

    public int EnrollmentId { get; set; }

    public int? Status { get; set; }

    public decimal? ProgressPercentage { get; set; }

    public decimal? TheoryScore { get; set; }

    public decimal? PracticalScore { get; set; }

    public decimal? FinalScore { get; set; }

    public DateTime StartDate { get; set; }

    public DateTime? LastUpdated { get; set; }

    public string? Name { get; set; }

    public string? Description { get; set; }

    public virtual Enrollment Enrollment { get; set; } = null!;

    public virtual ICollection<SectionRecord> SectionRecords { get; set; } = new List<SectionRecord>();
}
