using Lssctc.ProgramManagement.Accounts.Authens.Dtos;

namespace Lssctc.ProgramManagement.Accounts.Authens.Services
{
    public interface IAuthensService
    {
        Task<LoginResponseModel> LoginWithUsername(LoginUsernameDto request);
        Task<LoginResponseModel> LoginWithEmail(LoginEmailDto request);
        Task<bool> Logout(string accessToken);
    }    
}
