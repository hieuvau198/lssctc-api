using Lssctc.ProgramManagement.Authens.Dtos;

namespace Lssctc.ProgramManagement.Authens.Services
{
    public interface IAuthensService
    {
        Task<LoginResponseModel> AuthenLogin(AuthensLoginDto request);
        Task<bool> Logout(string accessToken);
       
    }    
}
