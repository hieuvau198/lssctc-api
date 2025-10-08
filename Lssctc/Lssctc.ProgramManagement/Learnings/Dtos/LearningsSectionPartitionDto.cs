namespace Lssctc.ProgramManagement.Learnings.Dtos
{
    public class LearningsSectionPartitionDto
    {
        public int SectionPartitionId { get; set; }
        public int SectionId { get; set; }
        public int PartitionRecordId { get; set; }
        public string? PartitionName { get; set; }
        public string? PartitionDescription { get; set; }
        public int PartitionOrder { get; set; }
        public int PartitionType { get; set; }
        public string? PartitionRecordStatus { get; set; }
        public bool IsCompleted { get; set; }
    }
}
