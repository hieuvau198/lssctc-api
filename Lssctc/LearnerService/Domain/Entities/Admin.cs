using System;
using System.Collections.Generic;

namespace LearnerService.Domain.Entities;

public partial class Admin
{
    public int UserId { get; set; }

    public bool? IsDeleted { get; set; }

    public virtual User User { get; set; } = null!;
}
