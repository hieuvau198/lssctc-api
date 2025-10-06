using System;
using System.Collections.Generic;

namespace Lssctc.Share.Entities;

public partial class SectionPractice
{
    public int Id { get; set; }

    public int SectionPartitionId { get; set; }

    public int PracticeId { get; set; }

    public DateTime? CustomDeadline { get; set; }

    public string? CustomDescription { get; set; }

    public int? Status { get; set; }

    public bool? IsActive { get; set; }

    public bool? IsDeleted { get; set; }

    public virtual Practice Practice { get; set; } = null!;

    public virtual SectionPartition SectionPartition { get; set; } = null!;

    public virtual ICollection<SectionPracticeAttempt> SectionPracticeAttempts { get; set; } = new List<SectionPracticeAttempt>();

    public virtual ICollection<SectionPracticeTimeslot> SectionPracticeTimeslots { get; set; } = new List<SectionPracticeTimeslot>();
}
