using Lssctc.Share.Enum;
using System;
using System.Collections.Generic;

namespace Lssctc.Share.Entities;

public partial class Class
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public DateTime StartDate { get; set; }

    public DateTime? EndDate { get; set; }

    public int? Capacity { get; set; }

    public int ProgramCourseId { get; set; }

    public int? ClassCodeId { get; set; }

    public string Description { get; set; } = null!;

    public ClassStatus Status { get; set; }

    public virtual ClassCode? ClassCode { get; set; }

    public virtual ICollection<ClassEnrollment> ClassEnrollments { get; set; } = new List<ClassEnrollment>();

    public virtual ICollection<ClassInstructor> ClassInstructors { get; set; } = new List<ClassInstructor>();

    public virtual ICollection<ClassMember> ClassMembers { get; set; } = new List<ClassMember>();

    public virtual ProgramCourse ProgramCourse { get; set; } = null!;

    public virtual ICollection<Section> Sections { get; set; } = new List<Section>();
}
