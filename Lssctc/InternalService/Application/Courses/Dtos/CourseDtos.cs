namespace InternalService.Application.Courses.Dtos;

public class CourseDto
{
    public int Id { get; set; }
    public int CourseDefinitionId { get; set; }
    public string Title { get; set; } = null!;
    public string CourseCode { get; set; } = null!;
    public string? Description { get; set; }
    public string Category { get; set; } = null!;
    public string Level { get; set; } = null!;
    public DateOnly? StartDate { get; set; }
    public DateOnly? EndDate { get; set; }
    public string? Location { get; set; }
    public string? Status { get; set; }
    public bool? IsDeleted { get; set; }
}

public class CreateCourseDto
{
    public int CourseDefinitionId { get; set; }
    public string Title { get; set; } = null!;
    public string CourseCode { get; set; } = null!;
    public string? Description { get; set; }
    public string Category { get; set; } = null!;
    public string Level { get; set; } = null!;
    public DateOnly? StartDate { get; set; }
    public DateOnly? EndDate { get; set; }
    public string? Location { get; set; }
    public string? Status { get; set; }
}

public class UpdateCourseDto
{
    public string Title { get; set; } = null!;
    public string? Description { get; set; }
    public string? Category { get; set; }
    public string? Level { get; set; }
    public DateOnly? StartDate { get; set; }
    public DateOnly? EndDate { get; set; }
    public string? Location { get; set; }
    public string? Status { get; set; }
}

public class CourseQueryParameters
{
    public string? SearchTerm { get; set; }
    public string? Category { get; set; }
    public string? Level { get; set; }
    public string? Status { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}
