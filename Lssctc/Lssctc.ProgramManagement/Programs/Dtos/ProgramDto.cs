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
        [Required]
        public string? Name { get; set; }  
        public string? Description { get; set; } = "A Program for training Mobile Crane";
        public string? ImageUrl { get; set; } = "https://www-assets.liebherr.com/media/bu-media/lhbu-lwe/images/subhome/liebherr-ltm-1920x1920-1_w736.jpg";
    }
    public class UpdateProgramDto
    {
        public string? Name { get; set; }
        public string? Description { get; set; }
        public string? ImageUrl { get; set; }
    }
}
