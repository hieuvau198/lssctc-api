namespace Lssctc.SimulationManagement.TraineePractices.Dtos
{
    public class TraineePracticeDto
    {
        public int SectionPracticeId { get; set; }
        public int PartitionId { get; set; }
        public int PracticeId { get; set; }
        public DateTime? CustomDeadline { get; set; }
        public string? Status { get; set; }
        public bool? IsCompleted { get; set; }
        public string? PracticeName { get; set; }
        public string? PracticeDescription { get; set; }
        public int? EstimatedDurationMinutes { get; set; }
        public string? DifficultyLevel { get; set; }
    }
}
