using System.ComponentModel.DataAnnotations;

namespace Lssctc.ProgramManagement.Materials.Dtos
{
    public class UpdateMaterialWithFileDto
    {
        [StringLength(100, MinimumLength = 3, ErrorMessage = "Name must be between 3 and 100 characters.")]
        public string? Name { get; set; }

        [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters.")]
        public string? Description { get; set; }

        [RegularExpression("(?i)^(Video|Document)$", ErrorMessage = "Learning material type must be one of: Video, Document.")]
        public string? LearningMaterialType { get; set; }

        // Optional: If provided, the file will be replaced. If null, the existing file remains.
        public IFormFile? File { get; set; }
    }
}
