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
        public string Position { get; set; }
        public InstructorDto Instructor { get; set; } = new();
    }

    public class AssignInstructorDto
    {
        public int ClassId { get; set; }
        public int InstructorId { get; set; }
        public string Position { get; set; } 
    }
    public class ClassMemberDto
    {
        public int Id { get; set; }
        public int TraineeId { get; set; }
        public int ClassId { get; set; }
        public DateTime AssignedDate { get; set; }
        public string Status { get; set; }
        public TraineeDto Trainee { get; set; } = new();
        public List<TrainingProgressDto> TrainingProgresses { get; set; } = new();
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
        
        // User info
        public string? FullName { get; set; }
        public string? Email { get; set; }
    }
    public class InstructorDto
    {
        public int Id { get; set; }
        public string? InstructorCode { get; set; }
        public bool? IsActive { get; set; }
        public DateTime? HireDate { get; set; }

        public int? ExperienceYears { get; set; }

        public string? Biography { get; set; }

        public string? ProfessionalProfileUrl { get; set; }

        public string? Specialization { get; set; }

        public string? FullName { get; set; }
        public string? Email { get; set; }


    }


    public class ClassCreateDto
    {
        public string ClassCode { get; set; } 
        public string Name { get; set; } 
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public int? Capacity { get; set; }
        public int ProgramCourseId { get; set; }
        public string Description { get; set; } 
    }
    public class ClassUpdateDto
    {
        public string Name { get; set; } = null!;
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public int? Capacity { get; set; }
        public string Description { get; set; } = null!;
        public string? ClassCode { get; set; } 
    }
    public class ClassDto
    {
        public int Id { get; set; }
        public string Name { get; set; } 
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public int? Capacity { get; set; }
        public int ProgramCourseId { get; set; }
        public ClassCodeDto? ClassCode { get; set; }
        public string Description { get; set; } 
        public string Status { get; set; }

        public List<ClassInstructorDto> Instructors { get; set; } = new();
        public List<ClassMemberDto> Members { get; set; } = new();
    }

    // My Class
    public class MyClassDto
    {
        public int CourseId { get; set; }
        public string? CourseName { get; set; }
        public string? CourseCode { get; set; } 
        public int? DurationHours { get; set; }
        public string? ImageUrl { get; set; }

        public int ClassId { get; set; }
        public string? ClassName { get; set; }
        public string? ClassCode { get; set; }         
        public int Status { get; set; }               
        public int? InstructorId { get; set; }         
        public string? InstructorName { get; set; }    
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
    }

    //Enrollment
    public class ClassEnrollmentCreateDto
    {
        public int ClassId { get; set; }
        public int TraineeId { get; set; }
        public string Name { get; set; } 
        public string Description { get; set; } 
        public string? TraineeContact { get; set; }
    }

    public class ClassEnrollmentDto
    {
        public int Id { get; set; }
        public int ClassId { get; set; }
        public int TraineeId { get; set; }
        public string Name { get; set; } 
        public string Description { get; set; } 
        public string Status { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? ApprovedDate { get; set; }

        public string? TraineeContact { get; set; }

        // Navigation
        public string ClassName { get; set; } 
        public string TraineeCode { get; set; } 
    }

    //


    //Approve enrollment
    public class ApproveEnrollmentDto
    {
        public int EnrollmentId { get; set; }
        public string? Description { get; set; }
    }


    // training progress, results,
    public class TrainingProgressDto
    {
        public int Id { get; set; }
        public int CourseMemberId { get; set; }
        public string Status { get; set; }
        public double? ProgressPercentage { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime? LastUpdated { get; set; }
        public string? Name { get; set; }
        public string? Description { get; set; }

        public List<TrainingResultDto> TrainingResults { get; set; } = new();
    }

    public class CreateTrainingProgressDto
    {
        public int CourseMemberId { get; set; }
        public string Status { get; set; }
        public string? Name { get; set; }
        public string? Description { get; set; }
        public double? ProgressPercentage { get; set; }
        public DateTime StartDate { get; set; } = DateTime.UtcNow;
        public DateTime? LastUpdated { get; set; } = DateTime.UtcNow;
    }

    public class UpdateTrainingProgressDto
    {
        public int Id { get; set; }
        public string Status { get; set; }
        public double? ProgressPercentage { get; set; }
        public string? Description { get; set; }
    }

    // Training Result 
    public class TrainingResultDto
    {
        public int Id { get; set; }
        public int TrainingResultTypeId { get; set; }
        public string? ResultValue { get; set; }
        public DateTime ResultDate { get; set; }
        public string? Notes { get; set; }

        public TrainingResultTypeDto TrainingResultType { get; set; }
    }

    public class CreateTrainingResultDto
    {
        public int TrainingProgressId { get; set; }
        public int TrainingResultTypeId { get; set; }
        public string? ResultValue { get; set; }
        public DateTime ResultDate { get; set; }
        public string? Notes { get; set; }
    }

    public class UpdateTrainingResultDto
    {
        public int Id { get; set; }
        public string? ResultValue { get; set; }
        public string? Notes { get; set; }
    }

    // Training Result Type
    public class TrainingResultTypeDto
    {
        public int Id { get; set; }
        public string Name { get; set; } 
        public string? Description { get; set; }
    }



    /// <summary>
    /// Section
    /// </summary>
    public class SectionDto
    {
        public int Id { get; set; }
        public string Name { get; set; } 
        public string? Description { get; set; }
        public int Order { get; set; }
        public int? DurationMinutes { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string Status { get; set; } 
    }

    public class SectionCreateDto
    {
        public int ClassId { get; set; }
        public string Name { get; set; } = null!;
        public string? Description { get; set; }
        public int SyllabusSectionId { get; set; }
        public int? DurationMinutes { get; set; }
        public int Order { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
    }


    // Syllabus Section DTOs
    public class SyllabusSectionCreateDto
    {
        public string SyllabusName { get; set; } = null!;
        public string CourseName { get; set; } = null!;
        public string CourseCode { get; set; } = null!;
        public string SectionTitle { get; set; } = null!;
        public string? SectionDescription { get; set; }
        public int SectionOrder { get; set; }
        public int? EstimatedDurationMinutes { get; set; }
    }

    public class SyllabusSectionDto
    {
        public int Id { get; set; }
        public string SectionTitle { get; set; } = null!;
        public string? SectionDescription { get; set; }
        public int SectionOrder { get; set; }
        public int? EstimatedDurationMinutes { get; set; }
        public string SyllabusName { get; set; } = null!;
        public string CourseName { get; set; } = null!;
        public string CourseCode { get; set; } = null!;
    }
}
