using System;
using System.Collections.Generic;

namespace InternalService.Domain2.Entities;

public partial class ClassCode
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public virtual ICollection<Class> Classes { get; set; } = new List<Class>();
}
