namespace Lssctc.ProgramManagement.Accounts.Authens.Dtos;

public class GoogleUserInfoDto
{
    public string Sub { get; set; } = default!;
    public string Name { get; set; } = default!;
    public string GivenName { get; set; } = default!;
    public string FamilyName { get; set; } = default!;
    public string Picture { get; set; } = default!;
    public string Email { get; set; } = default!;
    public bool EmailVerified { get; set; }
}