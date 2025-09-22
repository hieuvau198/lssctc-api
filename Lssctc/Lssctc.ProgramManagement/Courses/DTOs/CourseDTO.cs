namespace Lssctc.ProgramManagement.Courses.DTOs
{
    public class CourseSyllabusCreateDto
    {
        public int CourseId { get; set; }
        
        public string SyllabusName { get; set; }
        public string SyllabusDescription { get; set; }
    }
    public class UpdateCourseSyllabusDto
    {
        public string Name { get; set; } 
        public string Description { get; set; } 
    }

    public class CourseSyllabusDto
    {
        public int Id { get; set; }
        public int CourseId { get; set; }
        public int SyllabusId { get; set; }
        public string? CourseName { get; set; }
        public string? SyllabusName { get; set; }
        public string? SyllabusDescription { get; set; }
    }
    public class CourseDto
    {
        public int Id { get; set; }
        public string Name { get; set; } 
        public string? Description { get; set; }
        public int? CategoryId { get; set; }
        public string? CategoryName { get; set; }
        public int? LevelId { get; set; }
        public string? LevelName { get; set; }
        public decimal? Price { get; set; }
        public bool? IsActive { get; set; }
        public string? ImageUrl { get; set; }
        public int? DurationHours { get; set; }
        public string? CourseCodeName { get; set; }
    }

    public class CreateCourseDto
    {
        public string Name { get; set; } 
        public string? Description { get; set; }
        public int? CategoryId { get; set; }
        public int? LevelId { get; set; }
        public decimal? Price { get; set; }
        public string? ImageUrl { get; set; }
        public int? DurationHours { get; set; }
        public string? CourseCodeName { get; set; }
    }

    public class UpdateCourseDto
    {
        public string Name { get; set; } 
        public string? Description { get; set; }
        public int? CategoryId { get; set; }
        public int? LevelId { get; set; }
        public decimal? Price { get; set; }
        public bool? IsActive { get; set; }
        public string? ImageUrl { get; set; }
        public int? DurationHours { get; set; }
        //public string? CourseCodeName { get; set; }
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
