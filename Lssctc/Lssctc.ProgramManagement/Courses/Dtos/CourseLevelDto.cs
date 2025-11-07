using System.ComponentModel.DataAnnotations;

namespace Lssctc.ProgramManagement.Courses.Dtos
{
    public class CourseLevelDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public string? Description { get; set; }
    }

    public class CreateCourseLevelDto
    {
        [Required(ErrorMessage = "Level name is required.")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Level name must be between 2 and 100 characters.")]
        public string Name { get; set; } = null!;

        [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters.")]
        public string? Description { get; set; }
    }

    public class UpdateCourseLevelDto
    {
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Level name must be between 2 and 100 characters.")]
        public string? Name { get; set; }

        [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters.")]
        public string? Description { get; set; }
    }
}
