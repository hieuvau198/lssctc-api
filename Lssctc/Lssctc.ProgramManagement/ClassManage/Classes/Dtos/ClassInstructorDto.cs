namespace Lssctc.ProgramManagement.ClassManage.Classes.Dtos
{
    public class ClassInstructorDto
    {
        public int Id { get; set; }
        public string? Fullname { get; set; } // <-- Renamed from Name
        public string? Email { get; set; } // <-- ADDED
        public string? PhoneNumber { get; set; } // <-- ADDED
        public string? AvatarUrl { get; set; } // <-- ADDED
        public string? InstructorCode { get; set; }
        public DateTime? HireDate { get; set; }
    }
}
