using System.ComponentModel.DataAnnotations;

namespace Lssctc.ProgramManagement.Programs.Dtos
{
    public class ProgramDto
    {
        public int? Id { get; set; }
        public string? Name { get; set; }   
        public string? Description { get; set; }
        public bool? IsActive { get; set; }
        public int? DurationHours { get; set; }
        public int? TotalCourses { get; set; }
        public string? ImageUrl { get; set; }
    }
    public class CreateProgramDto
    {
        [Required(ErrorMessage = "Program name is required.")]
        [StringLength(200, MinimumLength = 3, ErrorMessage = "Program name must be between 3 and 200 characters.")]
        public string? Name { get; set; }

        [StringLength(1000, ErrorMessage = "Description cannot exceed 1000 characters.")]
        public string? Description { get; set; } = "A Program for training Mobile Crane";

        [Url(ErrorMessage = "Image URL must be a valid URL.")]
        public string? ImageUrl { get; set; } = "https://www-assets.liebherr.com/media/bu-media/lhbu-lwe/images/subhome/liebherr-ltm-1920x1920-1_w736.jpg";
    }

    public class UpdateProgramDto
    {
        [StringLength(200, MinimumLength = 3, ErrorMessage = "Program name must be between 3 and 200 characters.")]
        public string? Name { get; set; }

        [StringLength(1000, ErrorMessage = "Description cannot exceed 1000 characters.")]
        public string? Description { get; set; }

        [Url(ErrorMessage = "Image URL must be a valid URL.")]
        public string? ImageUrl { get; set; }
    }
}
