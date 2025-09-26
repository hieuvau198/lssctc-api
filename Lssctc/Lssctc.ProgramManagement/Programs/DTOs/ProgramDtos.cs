namespace Lssctc.ProgramManagement.Programs.DTOs
{

    public class ProgramDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string? Description { get; set; }
        public bool? IsDeleted { get; set; }
        public bool? IsActive { get; set; }
        public int? DurationHours { get; set; }
        public int? TotalCourses { get; set; }
        public string? ImageUrl { get; set; }

        public ICollection<ProgramCourseDto> Courses { get; set; } = new List<ProgramCourseDto>();
        public ICollection<EntryRequirementDto> EntryRequirements { get; set; } = new List<EntryRequirementDto>();
    }

    public class UpdateProgramInfoDto
    {
        public string Name { get; set; }
        public string? Description { get; set; }
        public string? ImageUrl { get; set; }
        public bool? IsActive { get; set; }
    }

    public class UpdateProgramCoursesDto
    {
        public ICollection<ProgramCourseOrderDto> Courses { get; set; } = new List<ProgramCourseOrderDto>();
    }

    public class UpdateProgramEntryRequirementsDto
    {
        public ICollection<UpdateEntryRequirementDto> EntryRequirements { get; set; } = new List<UpdateEntryRequirementDto>();
    }

    public class CreateProgramDto
        {
            public string Name { get; set; } 
            public string? Description { get; set; }
            public int? DurationHours { get; set; }
            public string? ImageUrl { get; set; }

        }
        
        public class ProgramCourseDto
        {
            public int Id { get; set; }
            public int CoursesId { get; set; }
            public int CourseOrder { get; set; }
            public string? Name { get; set; }
            public string? Description { get; set; }
        }
        public class CourseOrderDto
        {
            public int CourseId { get; set; }
            public int Order { get; set; }
        }
    public class EntryRequirementDto
    {
        public string Name { get; set; }
        public string? Description { get; set; }
        public string? DocumentUrl { get; set; }
    }

    public class CreateProgramPrerequisiteDto
        {
            public string Name { get; set; } 
            public string? Description { get; set; }
        }

    public class UpdateEntryRequirementDto
    {
        public string Name { get; set; }
        public string? Description { get; set; }
        public string? DocumentUrl { get; set; }
    }
    public class ProgramCourseOrderDto
        {
            public int CourseId { get; set; }
            public int Order { get; set; }
        }
        public class ProgramQueryParameters
        {
            public int PageNumber { get; set; } = 1;
            public int PageSize { get; set; } = 10;

            public string? SearchTerm { get; set; }

        }

}
