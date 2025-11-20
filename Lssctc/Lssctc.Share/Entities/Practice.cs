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

    public string? PracticeCode { get; set; }

    public virtual ICollection<ActivityPractice> ActivityPractices { get; set; } = new List<ActivityPractice>();

    public virtual ICollection<PracticeTask> PracticeTasks { get; set; } = new List<PracticeTask>();
}
