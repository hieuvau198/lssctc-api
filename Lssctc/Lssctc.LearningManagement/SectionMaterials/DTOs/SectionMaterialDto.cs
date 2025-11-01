namespace Lssctc.LearningManagement.SectionMaterials.DTOs
{
    public class SectionMaterialDto
    {
        public int Id { get; set; }
        public int SectionPartitionId { get; set; }
        public int LearningMaterialId { get; set; }
        public string? Description { get; set; }
        public string Name { get; set; }
    }

    public class CreateSectionMaterialDto
    {
        public int SectionPartitionId { get; set; }
        public string? Name { get; set; }
        public int LearningMaterialId { get; set; }
        public string? Description { get; set; }
    }

    public class UpdateSectionMaterialDto
    {
        public int? SectionPartitionId { get; set; }
        public string? Name { get; set; }
        public int? LearningMaterialId { get; set; }
        public string? Description { get; set; }
    }

    public class UpsertSectionMaterialDto
    {
        public int SectionPartitionId { get; set; }
        public int LearningMaterialId { get; set; }
        public string? Name { get; set; }
        public string? Description { get; set; }
    }
}
