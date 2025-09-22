using System;
using System.Collections.Generic;

namespace Lssctc.Share.Entities;

public partial class Practice
{
    public int Id { get; set; }

    public string PracticeName { get; set; } = null!;

    public string? PracticeDescription { get; set; }

    public int? EstimatedDurationMinutes { get; set; }

    public string? DifficultyLevel { get; set; }

    public int? MaxAttempts { get; set; }

    public DateTime CreatedDate { get; set; }

    public bool? IsActive { get; set; }

    public bool? IsDeleted { get; set; }

    public virtual ICollection<PracticeStep> PracticeSteps { get; set; } = new List<PracticeStep>();

    public virtual ICollection<SectionPractice> SectionPractices { get; set; } = new List<SectionPractice>();
}
