namespace Lssctc.ProgramManagement.Learnings.LearningsMaterials.Dtos
{
    public class LearningsSectionMaterialDto
    {
        public int SectionMaterialId { get; set; }
        public int MaterialId { get; set; }
        public int PartitionId { get; set; }
        public int PartitionRecordId { get; set; }
        public string? MaterialName { get; set; }
        public string? MaterialDescription { get; set; }
        public int MaterialType { get; set; }
        public string? MaterialUrl { get; set; }
        public string? PartitionRecordStatus { get; set; }
        public bool IsCompleted { get; set; }
    }
}
