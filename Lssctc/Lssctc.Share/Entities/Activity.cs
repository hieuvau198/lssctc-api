using System;
using System.Collections.Generic;

namespace Lssctc.Share.Entities;

public partial class Activity
{
    public int Id { get; set; }

    public string ActivityTitle { get; set; } = null!;

    public string? ActivityDescription { get; set; }

    public int? ActivityType { get; set; }

    public int? EstimatedDurationMinutes { get; set; }

    public bool? IsDeleted { get; set; }

    public virtual ICollection<ActivityMaterial> ActivityMaterials { get; set; } = new List<ActivityMaterial>();

    public virtual ICollection<ActivityPractice> ActivityPractices { get; set; } = new List<ActivityPractice>();

    public virtual ICollection<ActivityQuiz> ActivityQuizzes { get; set; } = new List<ActivityQuiz>();

    public virtual ICollection<ActivitySession> ActivitySessions { get; set; } = new List<ActivitySession>();

    public virtual ICollection<SectionActivity> SectionActivities { get; set; } = new List<SectionActivity>();
}
