using System.ComponentModel.DataAnnotations;

namespace Lssctc.ProgramManagement.Courses.Dtos
{
    public class CourseDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public string? Description { get; set; }
        public string? Category { get; set; } // Was int? CategoryId
        public string? Level { get; set; }    // Was int? LevelId
        public decimal? Price { get; set; }
        public int? DurationHours { get; set; }
        public string? ImageUrl { get; set; }
        public bool IsActive { get; set; }
    }

    public class CreateCourseDto
    {
        [Required(ErrorMessage = "Course name is required.")]
        [StringLength(200, MinimumLength = 3, ErrorMessage = "Course name must be between 3 and 200 characters.")]
        public string Name { get; set; } = null!;

        [Required(ErrorMessage = "Description is required.")]
        [StringLength(1000, ErrorMessage = "Description cannot exceed 1000 characters.")]
        public string? Description { get; set; }

        [Required(ErrorMessage = "Category ID is required.")]
        [Range(1, int.MaxValue, ErrorMessage = "Category ID must be a positive number.")]
        public int? CategoryId { get; set; }

        [Required(ErrorMessage = "Level ID is required.")]
        [Range(1, int.MaxValue, ErrorMessage = "Level ID must be a positive number.")]
        public int? LevelId { get; set; }

        [Range(0, 1000000000, ErrorMessage = "Price must be between 0 and 1,000,000,000. Default currency is VND")]
        public decimal? Price { get; set; }

        [Required(ErrorMessage = "Duration in hours is required.")]
        [Range(1, 500, ErrorMessage = "Duration must be between 1 and 500 hours.")]
        public int? DurationHours { get; set; }

        [Url(ErrorMessage = "Image URL must be a valid URL.")]
        public string? ImageUrl { get; set; } = "https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcS2sUcEWdSaINXf8E4hmy7obh3B1w0-l_T8Tw&s";
    }

    public class UpdateCourseDto
    {
        [StringLength(200, MinimumLength = 3, ErrorMessage = "Course name must be between 3 and 200 characters.")]
        public string? Name { get; set; }

        [StringLength(1000, ErrorMessage = "Description cannot exceed 1000 characters.")]
        public string? Description { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Category ID must be a positive number.")]
        public int? CategoryId { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Level ID must be a positive number.")]
        public int? LevelId { get; set; }

        [Range(0, 1000000000, ErrorMessage = "Price must be between 0 and 1,000,000.")]
        public decimal? Price { get; set; }

        [Range(1, 500, ErrorMessage = "Duration must be between 1 and 500 hours.")]
        public int? DurationHours { get; set; }

        [Url(ErrorMessage = "Image URL must be a valid URL.")]
        public string? ImageUrl { get; set; }

        public bool? IsActive { get; set; }
    }
}
