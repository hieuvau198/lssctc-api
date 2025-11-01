namespace Lssctc.LearningManagement.TraineePractices.Dtos
{
    public class TraineePracticeDto
    {
        public int SectionPracticeId { get; set; } // SectionPractice
        public int PartitionId { get; set; } // SectionPartition
        public int PracticeId { get; set; } // Practice
        public DateTime? CustomDeadline { get; set; } // SectionPractice
        public string? Status { get; set; } // SectionPractice
        public bool? IsCompleted { get; set; } // SectionPractice
        public string? PracticeName { get; set; } // Practice
        public string? PracticeDescription { get; set; } // Practice
        public int? EstimatedDurationMinutes { get; set; } // Practice
        public string? DifficultyLevel { get; set; } // Practice
    }
}
