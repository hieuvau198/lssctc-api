using System;
using System.Collections.Generic;

namespace IdentityService.Domain.Entities;

public partial class Role
{
    public byte Id { get; set; }

    public string Name { get; set; } = null!;

    public virtual ICollection<User> Users { get; set; } = new List<User>();
}
