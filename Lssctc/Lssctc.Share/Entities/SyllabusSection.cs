using System;
using System.Collections.Generic;

namespace Lssctc.Share.Entities;

public partial class SyllabusSection
{
    public int Id { get; set; }

    public int SyllabusId { get; set; }

    public string SectionTitle { get; set; } = null!;

    public string? SectionDescription { get; set; }

    public int SectionOrder { get; set; }

    public int? EstimatedDurationMinutes { get; set; }

    public bool? IsDeleted { get; set; }

    public virtual ICollection<Section> Sections { get; set; } = new List<Section>();

    public virtual Syllabuse Syllabus { get; set; } = null!;
}
