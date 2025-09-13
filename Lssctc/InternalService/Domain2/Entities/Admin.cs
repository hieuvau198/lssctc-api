using System;
using System.Collections.Generic;

namespace InternalService.Domain2.Entities;

public partial class Admin
{
    public int Id { get; set; }

    public virtual User IdNavigation { get; set; } = null!;
}
