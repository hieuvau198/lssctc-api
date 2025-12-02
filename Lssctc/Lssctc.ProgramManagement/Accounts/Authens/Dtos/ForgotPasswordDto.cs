using System.ComponentModel.DataAnnotations;

namespace Lssctc.ProgramManagement.Accounts.Authens.Dtos
{
    public class ForgotPasswordDto
    {
        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email address")]
        public string Email { get; set; } = string.Empty;
    }
}
