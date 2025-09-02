using System;
using System.Collections.Generic;

namespace IdentityService.Domain.Entities;

public partial class Staff
{
    public int UserId { get; set; }

    public string? EmploymentStatus { get; set; }

    public bool? IsDeleted { get; set; }

    public virtual User User { get; set; } = null!;
}
