namespace Lssctc.ProgramManagement.ClassManage.SectionRecords.Dtos
{
    public class SectionRecordDto
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public int LearningProgressId { get; set; }
        public int? SectionId { get; set; }
        public string? SectionName { get; set; }
        public bool IsCompleted { get; set; }
        public bool IsTraineeAttended { get; set; }
        public decimal? Progress { get; set; }
        public int? DurationMinutes { get; set; } = 20;
        public int TraineeId { get; set; }
        public string TraineeName { get; set; } = string.Empty;
        public int ClassId { get; set; }
    }
}
