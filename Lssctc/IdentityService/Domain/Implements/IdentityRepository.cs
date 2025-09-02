using IdentityService.Domain.Contexts;
using IdentityService.Domain.Entities;
using IdentityService.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace IdentityService.Domain.Implements
{
    public class IdentityRepository : IIdentityRepository
    {
        private readonly IdentityDbContext _context;

        public IdentityRepository(IdentityDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<User>> GetAllUsersAsync()
        {
            return await _context.Users
                .Include(u => u.Role)
                .ToListAsync();
        }


        public async Task<User?> GetUserByEmailAsync(string email)
        {
            return await _context.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.Email == email && u.IsDeleted != true);
        }

        public async Task<User?> GetUserByUsernameAsync(string username)
        {
            return await _context.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.Username == username && u.IsDeleted != true);
        }

        public async Task<User?> GetUserByIdAsync(int id)
        {
            return await _context.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.Id == id && u.IsDeleted != true);
        }

        public async Task<User> CreateUserAsync(User user)
        {
            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            return user;
        }

        public async Task<User> UpdateUserAsync(User user)
        {
            user.UpdatedAt = DateTime.UtcNow;
            _context.Users.Update(user);
            await _context.SaveChangesAsync();
            return user;
        }

        public async Task<bool> EmailExistsAsync(string email)
        {
            return await _context.Users.AnyAsync(u => u.Email == email && u.IsDeleted != true);
        }

        public async Task<bool> UsernameExistsAsync(string username)
        {
            return await _context.Users.AnyAsync(u => u.Username == username && u.IsDeleted != true);
        }

        public async Task<Role?> GetRoleByIdAsync(byte roleId)
        {
            return await _context.Roles.FindAsync(roleId);
        }

        // For simplicity, storing refresh tokens in a separate table or in-memory cache
        // In production, consider using Redis or a proper RefreshToken entity
        private static readonly Dictionary<int, (string Token, DateTime ExpiresAt)> _refreshTokens = new();

        public async Task SaveRefreshTokenAsync(int userId, string refreshToken, DateTime expiresAt)
        {
            _refreshTokens[userId] = (refreshToken, expiresAt);
            await Task.CompletedTask;
        }

        public async Task<string?> GetRefreshTokenAsync(int userId)
        {
            if (_refreshTokens.TryGetValue(userId, out var tokenData) && tokenData.ExpiresAt > DateTime.UtcNow)
            {
                return tokenData.Token;
            }
            return null;
        }

        public async Task RevokeRefreshTokenAsync(int userId)
        {
            _refreshTokens.Remove(userId);
            await Task.CompletedTask;
        }

        public async Task<bool> ValidateRefreshTokenAsync(int userId, string refreshToken)
        {
            if (_refreshTokens.TryGetValue(userId, out var tokenData))
            {
                return tokenData.Token == refreshToken && tokenData.ExpiresAt > DateTime.UtcNow;
            }
            return false;
        }
    }
}
