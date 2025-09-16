using System;
using System.Collections.Generic;

namespace Lssctc.Share.Entities;

public partial class ProgramManager
{
    public int Id { get; set; }

    public virtual User IdNavigation { get; set; } = null!;
}
