using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lssctc.Share.Enums
{
    public enum TimeslotStatusEnum
    {
        NotStarted = 1,
        Ongoing = 2,
        Completed = 3,
        Cancelled = 4
    }

    public enum AttendanceStatusEnum
    {
        NotStarted = 1,
        Present = 2,
        Absent = 3,
        Cancelled = 4
    }
}