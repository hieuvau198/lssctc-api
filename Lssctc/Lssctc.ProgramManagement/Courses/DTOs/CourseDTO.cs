namespace Lssctc.ProgramManagement.Courses.DTOs
{
    public class CourseDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public string? Description { get; set; }
        public int? CategoryId { get; set; }
        public string? CategoryName { get; set; }
        public int? LevelId { get; set; }
        public string? LevelName { get; set; }
        public decimal? Price { get; set; }
        public bool? IsActive { get; set; }
        public string? ImageUrl { get; set; }
        public int? DurationHours { get; set; }
        public int? CourseCodeId { get; set; }
    }

    public class CreateCourseDto
    {
        public string Name { get; set; } = null!;
        public string? Description { get; set; }
        public int? CategoryId { get; set; }
        public int? LevelId { get; set; }
        public decimal? Price { get; set; }
        public string? ImageUrl { get; set; }
        public int? DurationHours { get; set; }
        public int? CourseCodeId { get; set; }
    }

    public class UpdateCourseDto
    {
        public string Name { get; set; } = null!;
        public string? Description { get; set; }
        public int? CategoryId { get; set; }
        public int? LevelId { get; set; }
        public decimal? Price { get; set; }
        public bool? IsActive { get; set; }
        public string? ImageUrl { get; set; }
        public int? DurationHours { get; set; }
        public int? CourseCodeId { get; set; }
    }
    public class CourseQueryParameters
    {
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;

        public string? SearchTerm { get; set; }
        public int? CategoryId { get; set; }
        public int? LevelId { get; set; }
        public bool? IsActive { get; set; }
    }
}
