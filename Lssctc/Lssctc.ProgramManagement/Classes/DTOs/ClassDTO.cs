namespace Lssctc.ProgramManagement.Classes.DTOs
{
    public class ClassCodeDto
    {
        public int? Id { get; set; }       
        public string? Name { get; set; }  
    }
    public class ClassInstructorDto
    {
        public int Id { get; set; }
        public int ClassId { get; set; }
        public int InstructorId { get; set; }
        public string Position { get; set; } = null!;
    }

    public class AssignInstructorDto
    {
        public int ClassId { get; set; }
        public int InstructorId { get; set; }
        public string Position { get; set; } = null!;
    }
    public class ClassMemberDto
    {
        public int Id { get; set; }
        public int TraineeId { get; set; }
        public int ClassId { get; set; }
        public DateTime AssignedDate { get; set; }
        public int Status { get; set; }
    }

    public class AssignTraineeDto
    {
        public int ClassId { get; set; }
        public int TraineeId { get; set; }
    }

    public class TraineeDto
    {
        public int Id { get; set; }
        public string? TraineeCode { get; set; }
        public bool? IsActive { get; set; }
    }
    public class InstructorDto
    {
        public int Id { get; set; }
        public string? InstructorCode { get; set; }
        public bool? IsActive { get; set; }
        public DateTime? HireDate { get; set; }
    }
    public class ClassCreateDto
    {
        public string Name { get; set; } = null!;
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public int? Capacity { get; set; }
        public int ProgramCourseId { get; set; }
        public ClassCodeDto? ClassCode { get; set; }
        public string Description { get; set; } = null!;
        public int Status { get; set; }
    }

    public class ClassDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public int? Capacity { get; set; }
        public int ProgramCourseId { get; set; }
        public ClassCodeDto? ClassCode { get; set; }
        public string Description { get; set; } = null!;
        public int Status { get; set; }

        public List<ClassInstructorDto> Instructors { get; set; } = new();
        public List<ClassMemberDto> Members { get; set; } = new();
    }
    
}
