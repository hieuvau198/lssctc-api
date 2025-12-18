using System;
using System.Collections.Generic;

namespace Lssctc.Share.Entities;

public partial class FinalExamTemplate
{
    public int Id { get; set; }

    public int ClassId { get; set; }

    public int Status { get; set; }

    public virtual Class Class { get; set; } = null!;

    public virtual ICollection<FinalExamPartialsTemplate> FinalExamPartialsTemplates { get; set; } = new List<FinalExamPartialsTemplate>();
}
