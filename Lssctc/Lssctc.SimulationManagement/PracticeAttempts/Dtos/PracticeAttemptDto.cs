namespace Lssctc.SimulationManagement.PracticeAttempts.Dtos
{
    public class PracticeAttemptDto
    {
        public int PracticeAttemptId { get; set; } // SectionPracticeAttempt

        public int SectionPracticeId { get; set; } // SectionPractice

        public int LearningRecordPartitionId { get; set; } // LearningRecordPartition

        public decimal? Score { get; set; } // SectionPracticeAttempt

        public DateTime AttemptDate { get; set; }  // SectionPracticeAttempt

        public int? AttemptStatus { get; set; } // SectionPracticeAttempt

        public string? Description { get; set; } // SectionPracticeAttempt

        public bool? IsPass { get; set; } // SectionPracticeAttempt
    }

}
