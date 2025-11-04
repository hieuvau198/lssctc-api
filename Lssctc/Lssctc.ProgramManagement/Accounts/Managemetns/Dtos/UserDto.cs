using System.ComponentModel.DataAnnotations;

namespace Lssctc.ProgramManagement.Accounts.Managemetns.Dtos
{
    public class UserDto
    {
        public int Id { get; set; }
        public string Username { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string? Fullname { get; set; }
        public string? Role { get; set; }
        public string? PhoneNumber { get; set; }
        public string? AvatarUrl { get; set; }
        public bool IsActive { get; set; }
    }
    public class UserSimpleDto
    {
        public int Id { get; set; }
        public string Username { get; set; } = null!;
        public string? Fullname { get; set; }
        public string? AvatarUrl { get; set; }
    }
    public class UserChangePasswordDto
    {
        [Required(ErrorMessage = "Current password is required.")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Current password must be at least 6 characters long.")]
        public string CurrentPassword { get; set; } = null!;

        [Required(ErrorMessage = "New password is required.")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "New password must be at least 6 characters long.")]
        public string NewPassword { get; set; } = null!;
    }
}
