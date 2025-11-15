using System.ComponentModel.DataAnnotations;

namespace Lssctc.ProgramManagement.Accounts.Profiles.Dtos
{
    public class UpdateUserAndInstructorProfileDto
    {
        // User Profile fields
        [StringLength(255, ErrorMessage = "Username cannot exceed 255 characters.")]
        public string? Username { get; set; }

        [EmailAddress(ErrorMessage = "Invalid email format.")]
        [StringLength(255, ErrorMessage = "Email cannot exceed 255 characters.")]
        public string? Email { get; set; }

        [StringLength(1000, ErrorMessage = "Fullname cannot exceed 1000 characters.")]
        public string? Fullname { get; set; }

        [Phone(ErrorMessage = "Invalid phone number format.")]
        [StringLength(1000, ErrorMessage = "Phone number cannot exceed 1000 characters.")]
        public string? PhoneNumber { get; set; }

        [Url(ErrorMessage = "Invalid URL format for avatar.")]
        [StringLength(1000, ErrorMessage = "Avatar URL cannot exceed 1000 characters.")]
        public string? AvatarUrl { get; set; }

        // Instructor fields
        [StringLength(1000, ErrorMessage = "Instructor code cannot exceed 1000 characters.")]
        public string? InstructorCode { get; set; }

        public DateTime? HireDate { get; set; }

        public bool? IsInstructorActive { get; set; }

        // Instructor Profile fields
        [Range(0, 100, ErrorMessage = "Experience years must be between 0 and 100.")]
        public int? ExperienceYears { get; set; }

        [StringLength(1000, ErrorMessage = "Biography cannot exceed 1000 characters.")]
        public string? Biography { get; set; }

        [Url(ErrorMessage = "Invalid URL format for professional profile.")]
        [StringLength(1000, ErrorMessage = "Professional profile URL cannot exceed 1000 characters.")]
        public string? ProfessionalProfileUrl { get; set; }

        [StringLength(1000, ErrorMessage = "Specialization cannot exceed 1000 characters.")]
        public string? Specialization { get; set; }
    }
}
