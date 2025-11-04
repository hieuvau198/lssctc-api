using Lssctc.ProgramManagement.Accounts.Authens.Dtos;

namespace Lssctc.ProgramManagement.Accounts.Authens.Services
{
    public interface IAuthensService
    {
        Task<LoginResponseModel> AuthenLogin(AuthensLoginDto request);
        Task<bool> Logout(string accessToken);
       
    }    
}
