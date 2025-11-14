using System.ComponentModel.DataAnnotations;

namespace Lssctc.ProgramManagement.Accounts.Profiles.Dtos
{
    public class UpdateInstructorProfileDto
    {
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
