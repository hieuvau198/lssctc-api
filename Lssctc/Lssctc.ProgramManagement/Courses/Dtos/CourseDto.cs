using System.ComponentModel.DataAnnotations;

namespace Lssctc.ProgramManagement.Courses.Dtos
{
    public class CourseDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public string? Description { get; set; }
        public int? CategoryId { get; set; }
        public int? LevelId { get; set; }
        public decimal? Price { get; set; }
        public int? DurationHours { get; set; }
        public string? ImageUrl { get; set; }
        public bool IsActive { get; set; }
    }

    public class CreateCourseDto
    {
        [Required]
        public string Name { get; set; } = null!;
        [Required]
        public string? Description { get; set; }
        [Required]
        public int? CategoryId { get; set; }
        [Required]
        public int? LevelId { get; set; }
        public decimal? Price { get; set; }
        [Required]
        public int? DurationHours { get; set; }
        public string? ImageUrl { get; set; } = "https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcS2sUcEWdSaINXf8E4hmy7obh3B1w0-l_T8Tw&s";
    }

    public class UpdateCourseDto
    {
        public string? Name { get; set; }
        public string? Description { get; set; }
        public int? CategoryId { get; set; }
        public int? LevelId { get; set; }
        public decimal? Price { get; set; }
        public int? DurationHours { get; set; }
        public string? ImageUrl { get; set; }
        public bool? IsActive { get; set; }
    }
}
