using Lssctc.ProgramManagement.Accounts.Authens.Dtos;

namespace Lssctc.ProgramManagement.Accounts.Authens.Services;

public interface IGoogleOAuthService
{
    Task<GoogleUserInfoDto?> ValidateTokenAsync(string AccessTokenGoogle);
}