namespace Lssctc.LearningManagement.LearningMaterials.DTOs
{
    public class LearningMaterialDto
    {
        public int Id { get; set; }
        public int LearningMaterialTypeId { get; set; }
        public string Name { get; set; } = null!;
        public string Description { get; set; } = null!;
        public string MaterialUrl { get; set; } = null!;
    }

    public class CreateLearningMaterialDto
    {
        public int LearningMaterialTypeId { get; set; }
        public string Name { get; set; } = null!;
        public string Description { get; set; } = null!;
        public string MaterialUrl { get; set; } = null!;
    }

    public class UpdateLearningMaterialDto
    {
        public int? LearningMaterialTypeId { get; set; }
        public string? Name { get; set; }
        public string? Description { get; set; }
        public string? MaterialUrl { get; set; }
    }
}
