namespace Lssctc.ProgramManagement.Accounts.Profiles.Dtos
{
    public class InstructorProfileWithUserDto
    {
        // User Information
        public int UserId { get; set; }
        public string Username { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string? Fullname { get; set; }
        public string? PhoneNumber { get; set; }
        public string? AvatarUrl { get; set; }
        public int? Role { get; set; }

        // Instructor Information
        public string? InstructorCode { get; set; }
        public DateTime? HireDate { get; set; }
        public bool? IsInstructorActive { get; set; }

        // Profile Information
        public int? ExperienceYears { get; set; }
        public string? Biography { get; set; }
        public string? ProfessionalProfileUrl { get; set; }
        public string? Specialization { get; set; }
    }
}
