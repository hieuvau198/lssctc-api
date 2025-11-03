namespace Lssctc.ProgramManagement.Authens.Dtos
{
    public class LoginResponseModel
    {
        public string? UserName { get; set; }
        public string? AccessToken { get; set; }
        public int ExpiresIn { get; set; } // time in seconds


    }
}
