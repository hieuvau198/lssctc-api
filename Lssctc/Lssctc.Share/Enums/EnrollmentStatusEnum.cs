using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lssctc.Share.Enums
{
    public enum EnrollmentStatusEnum
    {
        Pending = 1,    // Trainee applied, awaits approval
        Enrolled = 2,     // Approved, or added by instructor
        Inprogress = 3,   // Class has started
        Cancelled = 4,    // Withdrawn by trainee or removed by instructor
        Rejected = 5,     // Admin/Instructor rejected the pending application
        Completed = 6,    // Trainee has completed the class
        Failed = 7        // Trainee failed the class
    }
}
