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
        public string Status { get; set; }
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
        public string Name { get; set; } = null!;
        public string? Description { get; set; }
    }
}
