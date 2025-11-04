using System.ComponentModel.DataAnnotations;

namespace Lssctc.ProgramManagement.Accounts.Authens.Dtos
{
    public class LoginEmailDto
    {
        [Required]
        public string Email { get; set; } = null!;
        [Required]
        public string Password { get; set; } = null!;
    }
}
