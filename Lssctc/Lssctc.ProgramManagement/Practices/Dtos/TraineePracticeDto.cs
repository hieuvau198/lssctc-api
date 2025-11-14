namespace Lssctc.ProgramManagement.Practices.Dtos
{
    public class TraineePracticeDto
    {
        public int Id { get; set; }
        public string PracticeName { get; set; } = null!;
        public string? PracticeDescription { get; set; }
        public int? EstimatedDurationMinutes { get; set; }
        public string? DifficultyLevel { get; set; }
        public int? MaxAttempts { get; set; }
        public DateTime CreatedDate { get; set; }
        public bool? IsActive { get; set; }
        public int ActivityRecordId { get; set; }
        public int? ActivityId { get; set; }
        public bool IsCompleted { get; set; }
        public List<TraineeTaskDto> Tasks { get; set; } = new List<TraineeTaskDto>();
    }
}
