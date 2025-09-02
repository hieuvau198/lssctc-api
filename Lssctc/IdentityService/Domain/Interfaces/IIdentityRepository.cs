using IdentityService.Domain.Entities;

namespace IdentityService.Domain.Interfaces
{
    public interface IIdentityRepository
    {
        Task<User?> GetUserByEmailAsync(string email);
        Task<User?> GetUserByUsernameAsync(string username);
        Task<User?> GetUserByIdAsync(int id);
        Task<User> CreateUserAsync(User user);
        Task<User> UpdateUserAsync(User user);
        Task<bool> EmailExistsAsync(string email);
        Task<bool> UsernameExistsAsync(string username);
        Task<Role?> GetRoleByIdAsync(byte roleId);
        Task SaveRefreshTokenAsync(int userId, string refreshToken, DateTime expiresAt);
        Task<string?> GetRefreshTokenAsync(int userId);
        Task RevokeRefreshTokenAsync(int userId);
        Task<bool> ValidateRefreshTokenAsync(int userId, string refreshToken);
        Task<IEnumerable<User>> GetAllUsersAsync();

    }
}
