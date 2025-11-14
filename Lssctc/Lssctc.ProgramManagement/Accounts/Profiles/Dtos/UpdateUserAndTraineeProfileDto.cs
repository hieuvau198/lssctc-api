using System.ComponentModel.DataAnnotations;

namespace Lssctc.ProgramManagement.Accounts.Profiles.Dtos
{
    public class UpdateUserAndTraineeProfileDto
    {
        // User Profile fields
        [Phone(ErrorMessage = "Invalid phone number format.")]
        [StringLength(1000, ErrorMessage = "Phone number cannot exceed 1000 characters.")]
        public string? PhoneNumber { get; set; }

        [Url(ErrorMessage = "Invalid URL format for avatar.")]
        [StringLength(1000, ErrorMessage = "Avatar URL cannot exceed 1000 characters.")]
        public string? AvatarUrl { get; set; }

        // Trainee Profile fields
        [StringLength(255, ErrorMessage = "Education level cannot exceed 255 characters.")]
        public string? EducationLevel { get; set; }

        [Url(ErrorMessage = "Invalid URL format for education image.")]
        [StringLength(1000, ErrorMessage = "Education image URL cannot exceed 1000 characters.")]
        public string? EducationImageUrl { get; set; }
    }
}
