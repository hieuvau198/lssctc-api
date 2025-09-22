using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lssctc.Share.Enums
{
    public enum ClassStatus { Draft = 1, Open = 2, Active = 3, Completed = 4, Cancelled = 5 }
    public enum ClassRegistrationStatus { Pending = 1, Approved = 2, Rejected = 3, Cancelled = 4 }
    public enum ClassMemberStatus { Pending = 1, Studying = 2, Completed = 3, Failed = 4, Withdrawn = 5 }
    public enum TraineeCertificateStatus { Pending = 1, Issued = 2, Rejected = 3, Expired = 4, Revoked = 5 }
    public enum TrainingProgressStatus { Assigned = 1, Studying = 2, Completed = 3, Failed = 4, Withdrawn = 5 }
    public enum SectionStatus { Planned = 1, Studying = 2, Completed = 3, Cancelled = 4 }
    public enum SectionQuizAttemptStatus { Pass = 1, Failed = 2 }
    public enum SectionPracticeAttemptStatus { Pass = 1, Failed = 2 }
    public enum SectionPracticeTimeslotStatus { Planned = 1, Practicing = 2, Completed = 3, Cancelled = 4 }
    public enum SimulationTimeslotStatus { Open = 1, Closed = 2, Assigned = 3 }
    public enum PaymentStatus { Pending = 1, Paid = 2, Failed = 3, Refunded = 4 }
    public enum TransactionStatus { Pending = 1, Completed = 2, Failed = 3 }

}
