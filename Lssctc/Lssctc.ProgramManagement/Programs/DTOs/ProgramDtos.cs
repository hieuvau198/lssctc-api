namespace Lssctc.ProgramManagement.Programs.DTOs
{
    
        public class ProgramDto
        {
            public int Id { get; set; }
            public string Name { get; set; } = null!;
            public string? Description { get; set; }
            public bool? IsDeleted { get; set; }
            public bool? IsActive { get; set; }
            public int? DurationHours { get; set; }
            public int? TotalCourses { get; set; }
            public string? ImageUrl { get; set; }

            public ICollection<ProgramCourseDto> Courses { get; set; } = new List<ProgramCourseDto>();
            public ICollection<ProgramPrerequisiteDto> Prerequisites { get; set; } = new List<ProgramPrerequisiteDto>();
        }
        public class CreateProgramDto
        {
            public string Name { get; set; } = null!;
            public string? Description { get; set; }
            public int? DurationHours { get; set; }
            public string? ImageUrl { get; set; }

            public ICollection<ProgramCourseOrderDto> Courses { get; set; } = new List<ProgramCourseOrderDto>();

            public ICollection<CreateProgramPrerequisiteDto> Prerequisites { get; set; } = new List<CreateProgramPrerequisiteDto>();
        }
        public class UpdateProgramDto
        {
            public int Id { get; set; }
            public string Name { get; set; } = null!;
            public string? Description { get; set; }
            public int? DurationHours { get; set; }
            public string? ImageUrl { get; set; }
            public bool? IsActive { get; set; }

            public ICollection<ProgramCourseOrderDto> Courses { get; set; } = new List<ProgramCourseOrderDto>();

            public ICollection<UpdateProgramPrerequisiteDto> Prerequisites { get; set; } = new List<UpdateProgramPrerequisiteDto>();
        }
        public class ProgramCourseDto
        {
            public int Id { get; set; }
            public int CoursesId { get; set; }
            public int CourseOrder { get; set; }
            public string? Name { get; set; }
            public string? Description { get; set; }
        }
        public class ProgramPrerequisiteDto
        {
            public int Id { get; set; }
            public string Name { get; set; } = null!;
            public string? Description { get; set; }
        }

        public class CreateProgramPrerequisiteDto
        {
            public string Name { get; set; } = null!;
            public string? Description { get; set; }
        }

        public class UpdateProgramPrerequisiteDto
        {
            public int Id { get; set; }
            public string Name { get; set; } = null!;
            public string? Description { get; set; }
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

            public bool? IsActive { get; set; }

            public bool? IsDeleted { get; set; }

            public int? MinDurationHours { get; set; }

            public int? MaxDurationHours { get; set; }
        }

}
