using System.ComponentModel.DataAnnotations;

namespace Lssctc.ProgramManagement.Enrollments.Dtos
{
    public class EnrollmentDto
    {
        public int Id { get; set; }
        public int ClassId { get; set; }
        public string ClassName { get; set; } = string.Empty;
        public string ClassCode { get; set; } = string.Empty;
        public int TraineeId { get; set; }
        public string TraineeName { get; set; } = string.Empty; 
        public DateTime? EnrollDate { get; set; }
        public string Status { get; set; } = string.Empty;
    }

    public class CreateEnrollmentDto
    {
        [Required]
        public int ClassId { get; set; }
    }

    public class InstructorAddTraineeDto
    {
        [Required]
        public int ClassId { get; set; }
        [Required]
        public int TraineeId { get; set; }
    }
}
