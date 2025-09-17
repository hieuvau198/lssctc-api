using IdentityService.Application.Dtos;

namespace IdentityService.Application.Interfaces
{
    public interface IAuthsService
    {
        Task<AuthResponseDto> LoginAsync(LoginRequestDto request);
        Task<AuthResponseDto> RegisterAsync(RegisterRequestDto request);
        Task<AuthResponseDto> RefreshTokenAsync(RefreshTokenRequestDto request);
        Task<bool> LogoutAsync(int userId);
        Task<bool> ChangePasswordAsync(int userId, ChangePasswordDto request);
        Task<bool> ForgotPasswordAsync(ForgotPasswordDto request);
        Task<bool> ResetPasswordAsync(ResetPasswordDto request);
        Task<UserDto?> GetCurrentUserAsync(int userId);

        Task<IEnumerable<UserDto>> GetAllUsersAsync();
        Task<UserDto> CreateUserByAdminAsync(CreateUserRequestDto request);
    }
}
