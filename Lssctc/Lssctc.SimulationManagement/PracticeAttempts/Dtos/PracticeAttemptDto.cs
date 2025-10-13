namespace Lssctc.SimulationManagement.PracticeAttempts.Dtos
{
    public class PracticeAttemptDto
    {
        public int PracticeAttemptId { get; set; } // SectionPracticeAttempt

        public int SectionPracticeId { get; set; } // SectionPractice

        public int LearningRecordPartitionId { get; set; } // LearningRecordPartition

        public decimal? Score { get; set; } 

        public DateTime AttemptDate { get; set; } 

        public int? AttemptStatus { get; set; } 

        public string? Description { get; set; }

        public bool? IsPass { get; set; }
    }

}
