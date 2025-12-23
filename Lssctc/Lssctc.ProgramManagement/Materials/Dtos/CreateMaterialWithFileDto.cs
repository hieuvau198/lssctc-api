using System.ComponentModel.DataAnnotations;

namespace Lssctc.ProgramManagement.Materials.Dtos
{
    public class CreateMaterialWithFileDto
    {
        [Required(ErrorMessage = "Name is required.")]
        [StringLength(100, MinimumLength = 3, ErrorMessage = "Name must be between 3 and 100 characters.")]
        public string Name { get; set; } = null!;

        [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters.")]
        public string? Description { get; set; }

        [Required(ErrorMessage = "Learning material type is required.")]
        [RegularExpression("(?i)^(Video|Document)$", ErrorMessage = "Learning material type must be one of: Video, Document.")]
        public string LearningMaterialType { get; set; } = null!;

        [Required(ErrorMessage = "File is required.")]
        public IFormFile File { get; set; } = null!;
    }
}
