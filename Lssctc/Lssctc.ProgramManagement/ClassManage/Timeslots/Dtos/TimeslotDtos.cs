using System.ComponentModel.DataAnnotations;

namespace Lssctc.ProgramManagement.ClassManage.Timeslots.Dtos
{
    public class TimeslotDto
    {
        public int Id { get; set; }
        public int ClassId { get; set; }
        public string? ClassName { get; set; }
        public string? Name { get; set; }
        public string? LocationDetail { get; set; }
        public string? LocationBuilding { get; set; }
        public string? LocationRoom { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public string? Status { get; set; }
    }
    // --- Input DTO for Timeslot creation ---
    public class CreateTimeslotDto
    {
        [Required(ErrorMessage = "ClassId is required.")]
        [Range(1, int.MaxValue, ErrorMessage = "ClassId must be greater than 0.")]
        public int ClassId { get; set; }

        [Required(ErrorMessage = "Timeslot name is required.")]
        [StringLength(200, ErrorMessage = "Name cannot exceed 200 characters.")]
        public string? Name { get; set; }

        [Required(ErrorMessage = "Start time is required.")]
        public DateTime StartTime { get; set; }

        [Required(ErrorMessage = "End time is required.")]
        public DateTime? EndTime { get; set; }

        [StringLength(1000, ErrorMessage = "Location detail cannot exceed 1000 characters.")]
        public string? LocationDetail { get; set; }

        [StringLength(200, ErrorMessage = "Location building cannot exceed 200 characters.")]
        public string? LocationBuilding { get; set; }

        [StringLength(100, ErrorMessage = "Location room cannot exceed 100 characters.")]
        public string? LocationRoom { get; set; }
    }
    // --- Output DTO for Instructor to view Trainee list for attendance ---
    public class TimeslotAttendanceDto
    {
        public int TimeslotId { get; set; }
        public string? TimeslotName { get; set; }
        public int ClassId { get; set; }
        public string ClassName { get; set; } = null!;
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public List<TraineeAttendanceDto> Trainees { get; set; } = new List<TraineeAttendanceDto>();
    }

    // --- Nested DTO for Trainee details in attendance list ---
    public class TraineeAttendanceDto
    {
        public int EnrollmentId { get; set; }
        public int TraineeId { get; set; }
        public string TraineeName { get; set; } = null!;
        public string TraineeCode { get; set; } = null!;
        public string? AvatarUrl { get; set; }
        public string AttendanceStatus { get; set; } = "NotStarted";
    }

    // --- Input DTO for submitting attendance ---
    public class SubmitAttendanceDto
    {
        [Required]
        public int TimeslotId { get; set; }

        [Required]
        [MinLength(1)]
        public List<TraineeAttendanceInputDto> AttendanceRecords { get; set; } = new List<TraineeAttendanceInputDto>();
    }

    // --- Nested DTO for submitting a single attendance record ---
    public class TraineeAttendanceInputDto
    {
        [Required]
        public int EnrollmentId { get; set; }

        [Required]
        // 1: NotStarted, 2: Present, 3: Absent, 4: Cancelled
        [Range(2, 4, ErrorMessage = "Status must be Present (2), Absent (3), or Cancelled (4).")]
        public int Status { get; set; }

        [StringLength(1000)]
        public string? Note { get; set; }
    }
}
