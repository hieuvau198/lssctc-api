using System.ComponentModel.DataAnnotations;

namespace Lssctc.ProgramManagement.Accounts.Authens.Dtos
{
    public class LoginUsernameDto
    {
        [Required]
        public string Username { get; set; } = null!;
        [Required]
        public string Password { get; set; } = null!;
    }

}
