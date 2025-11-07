using System.ComponentModel.DataAnnotations;

namespace Lssctc.ProgramManagement.Accounts.Users.Dtos
{
    public class UpdateUserDto
    {
        [StringLength(100, ErrorMessage = "Full name cannot exceed 100 characters.")]
        public string? Fullname { get; set; }

        [Phone(ErrorMessage = "Invalid phone number format.")]
        [StringLength(15, ErrorMessage = "Phone number cannot exceed 15 digits.")]
        public string? PhoneNumber { get; set; }

        [Url(ErrorMessage = "Invalid URL format for avatar.")]
        public string? AvatarUrl { get; set; }
    }
}
