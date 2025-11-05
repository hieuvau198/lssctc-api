namespace Lssctc.ProgramManagement.ClassManage.Progresses.Dtos
{
    public class ProgressDto
    {
        public int Id { get; set; }
        public int EnrollmentId { get; set; }
        public int TraineeId { get; set; }
        public string TraineeName { get; set; } = string.Empty;
        public int ClassId { get; set; }
        public string ClassName { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public decimal? ProgressPercentage { get; set; }
        public decimal? TheoryScore { get; set; }
        public decimal? PracticalScore { get; set; }
        public decimal? FinalScore { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime? LastUpdated { get; set; }
        public string? Name { get; set; }
        public string? Description { get; set; }
        public ICollection<SectionRecordDto> SectionRecords { get; set; } = new List<SectionRecordDto>();
    }
}
