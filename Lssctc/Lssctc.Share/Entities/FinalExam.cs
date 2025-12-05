using System;
using System.Collections.Generic;

namespace Lssctc.Share.Entities;

public partial class FinalExam
{
    public int Id { get; set; }

    public int EnrollmentId { get; set; }

    public bool? IsPass { get; set; }

    public decimal? TotalMarks { get; set; }

    public DateTime? CompleteTime { get; set; }

    public virtual Enrollment Enrollment { get; set; } = null!;

    public virtual ICollection<FinalExamPartial> FinalExamPartials { get; set; } = new List<FinalExamPartial>();
}
