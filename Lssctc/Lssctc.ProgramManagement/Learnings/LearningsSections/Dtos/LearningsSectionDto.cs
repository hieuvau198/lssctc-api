namespace Lssctc.ProgramManagement.Learnings.LearningsSections.Dtos
{
    public class LearningsSectionDto
    {
        public int SectionId { get; set; }
        public string? SectionName { get; set; }
        public string? SectionDescription { get; set; }
        public int SectionOrder { get; set; }
        public int DurationMinutes { get; set; }
        public string? SectionRecordStatus { get; set; }
        public int ClassId { get; set; }
        public int SectionRecordId { get; set; }
        public bool IsCompleted { get; set; }
        public decimal? SectionProgress { get; set; }
        public bool IsTraineeAttended { get; set; }
    }
}
