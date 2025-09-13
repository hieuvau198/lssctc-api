using System;
using System.Collections.Generic;

namespace InternalService.Domain2.Entities;

public partial class SectionPartition
{
    public int Id { get; set; }

    public int SectionId { get; set; }

    public string? Name { get; set; }

    public int PartitionTypeId { get; set; }

    public string? Description { get; set; }

    public virtual ICollection<LearningRecordPartition> LearningRecordPartitions { get; set; } = new List<LearningRecordPartition>();

    public virtual SectionPartitionType PartitionType { get; set; } = null!;

    public virtual Section Section { get; set; } = null!;

    public virtual ICollection<SectionMaterial> SectionMaterials { get; set; } = new List<SectionMaterial>();

    public virtual ICollection<SectionPractice> SectionPractices { get; set; } = new List<SectionPractice>();

    public virtual ICollection<SectionQuiz> SectionQuizzes { get; set; } = new List<SectionQuiz>();

    public virtual ICollection<SimulationTimeslot> SimulationTimeslots { get; set; } = new List<SimulationTimeslot>();
}
