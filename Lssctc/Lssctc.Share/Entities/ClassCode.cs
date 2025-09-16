using System;
using System.Collections.Generic;

namespace Lssctc.Share.Entities;

public partial class ClassCode
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public virtual ICollection<Class> Classes { get; set; } = new List<Class>();
}
