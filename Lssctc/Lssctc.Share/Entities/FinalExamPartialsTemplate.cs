using System;
using System.Collections.Generic;

namespace Lssctc.Share.Entities;

public partial class FinalExamPartialsTemplate
{
    public int Id { get; set; }

    public int FinalExamTemplateId { get; set; }

    public int Type { get; set; }

    public decimal Weight { get; set; }

    public virtual FinalExamTemplate FinalExamTemplate { get; set; } = null!;
}
