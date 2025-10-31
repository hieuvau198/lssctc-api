using System;
using System.Collections.Generic;

namespace Lssctc.Share.Entities;

public partial class SectionActivity
{
    public int Id { get; set; }

    public int SectionId { get; set; }

    public int ActivityId { get; set; }

    public int? ActivityOrder { get; set; }

    public virtual Activity Activity { get; set; } = null!;

    public virtual Section Section { get; set; } = null!;
}
