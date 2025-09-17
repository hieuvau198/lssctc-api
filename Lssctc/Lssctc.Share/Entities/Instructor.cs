using System;
using System.Collections.Generic;

namespace Lssctc.Share.Entities;

public partial class Instructor
{
    public int Id { get; set; }

    public DateTime? HireDate { get; set; }

    public string? InstructorCode { get; set; }

    public bool? IsActive { get; set; }

    public bool? IsDeleted { get; set; }

    public virtual ICollection<ClassInstructor> ClassInstructors { get; set; } = new List<ClassInstructor>();

    public virtual User IdNavigation { get; set; } = null!;

    public virtual InstructorProfile? InstructorProfile { get; set; }
}
