using System.ComponentModel.DataAnnotations;

namespace Lssctc.ProgramManagement.Materials.Dtos
{
    public class MaterialDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public string? Description { get; set; }
        public string? MaterialUrl { get; set; }
        public string? LearningMaterialType { get; set; } // string (Video/Document)
    }

    public class CreateMaterialDto
    {
        [Required(ErrorMessage = "Name is required.")]
        [StringLength(100, MinimumLength = 3, ErrorMessage = "Name must be between 3 and 100 characters.")]
        public string? Name { get; set; }

        [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters.")]
        public string? Description { get; set; }

        [Required(ErrorMessage = "Material URL is required.")]
        [Url(ErrorMessage = "Material URL must be a valid URL.")]
        public string? MaterialUrl { get; set; }

        [Required(ErrorMessage = "Learning material type is required.")]
        [RegularExpression("(?i)^(Video|Document)$", ErrorMessage = "Learning material type must be one of: Video, Document.")]
        public string? LearningMaterialType { get; set; }
    }

    public class UpdateMaterialDto
    {
        [StringLength(100, MinimumLength = 3, ErrorMessage = "Name must be between 3 and 100 characters.")]
        public string? Name { get; set; }

        [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters.")]
        public string? Description { get; set; }

        [Url(ErrorMessage = "Material URL must be a valid URL.")]
        public string? MaterialUrl { get; set; }

        [RegularExpression("(?i)^(Video|Document)$", ErrorMessage = "Learning material type must be one of: Video, Document.")]
        public string? LearningMaterialType { get; set; }
    }

    public class ActivityMaterialDto
    {
        public int Id { get; set; }
        public int ActivityId { get; set; }
        public int LearningMaterialId { get; set; }
        public string Name { get; set; } = null!;
        public string Description { get; set; } = null!;
        public string? LearningMaterialType { get; set; }
        public string? MaterialUrl { get; set; }
    }
}
