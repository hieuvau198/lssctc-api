using AutoMapper;
using IdentityService.Application.Dtos;
using IdentityService.Application.Interfaces;
using IdentityService.Domain.Entities;
using IdentityService.Domain.Interfaces;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace IdentityService.Application.Services
{
    public class AuthService : IAuthService
    {
        private readonly IIdentityRepository _repository;
        private readonly IMapper _mapper;
        private readonly IConfiguration _configuration;

        public AuthService(IIdentityRepository repository, IMapper mapper, IConfiguration configuration)
        {
            _repository = repository;
            _mapper = mapper;
            _configuration = configuration;
        }

        public async Task<AuthResponseDto> LoginAsync(LoginRequestDto request)
        {
            var user = await _repository.GetUserByEmailAsync(request.Email);

            if (user == null || !VerifyPassword(request.Password, user.Password))
            {
                throw new UnauthorizedAccessException("Invalid email or password");
            }

            var token = GenerateJwtToken(user);
            var refreshToken = GenerateRefreshToken();
            var expiresAt = DateTime.UtcNow.AddHours(1); // JWT expires in 1 hour

            await _repository.SaveRefreshTokenAsync(user.Id, refreshToken, DateTime.UtcNow.AddDays(7));

            return new AuthResponseDto
            {
                Token = token,
                RefreshToken = refreshToken,
                ExpiresAt = expiresAt,
                User = _mapper.Map<UserDto>(user)
            };
        }

        public async Task<AuthResponseDto> RegisterAsync(RegisterRequestDto request)
        {
            // Check if email exists
            if (await _repository.EmailExistsAsync(request.Email))
            {
                throw new ArgumentException("Email already exists");
            }

            // Check if username exists
            if (await _repository.UsernameExistsAsync(request.Username))
            {
                throw new ArgumentException("Username already exists");
            }

            // Validate role exists
            var role = await _repository.GetRoleByIdAsync(request.RoleId);
            if (role == null)
            {
                throw new ArgumentException("Invalid role");
            }

            // Create user
            var user = _mapper.Map<User>(request);
            user.Password = HashPassword(request.Password);

            user = await _repository.CreateUserAsync(user);
            user.Role = role; // Set for mapping

            var token = GenerateJwtToken(user);
            var refreshToken = GenerateRefreshToken();
            var expiresAt = DateTime.UtcNow.AddHours(1);

            await _repository.SaveRefreshTokenAsync(user.Id, refreshToken, DateTime.UtcNow.AddDays(7));

            return new AuthResponseDto
            {
                Token = token,
                RefreshToken = refreshToken,
                ExpiresAt = expiresAt,
                User = _mapper.Map<UserDto>(user)
            };
        }

        public async Task<AuthResponseDto> RefreshTokenAsync(RefreshTokenRequestDto request)
        {
            var handler = new JwtSecurityTokenHandler();
            var jsonToken = handler.ReadJwtToken(request.RefreshToken);
            var userIdClaim = jsonToken?.Claims?.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier);

            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out var userId))
            {
                throw new UnauthorizedAccessException("Invalid refresh token");
            }

            if (!await _repository.ValidateRefreshTokenAsync(userId, request.RefreshToken))
            {
                throw new UnauthorizedAccessException("Invalid or expired refresh token");
            }

            var user = await _repository.GetUserByIdAsync(userId);
            if (user == null)
            {
                throw new UnauthorizedAccessException("User not found");
            }

            var newToken = GenerateJwtToken(user);
            var newRefreshToken = GenerateRefreshToken();
            var expiresAt = DateTime.UtcNow.AddHours(1);

            await _repository.SaveRefreshTokenAsync(userId, newRefreshToken, DateTime.UtcNow.AddDays(7));

            return new AuthResponseDto
            {
                Token = newToken,
                RefreshToken = newRefreshToken,
                ExpiresAt = expiresAt,
                User = _mapper.Map<UserDto>(user)
            };
        }

        public async Task<bool> LogoutAsync(int userId)
        {
            await _repository.RevokeRefreshTokenAsync(userId);
            return true;
        }

        public async Task<bool> ChangePasswordAsync(int userId, ChangePasswordDto request)
        {
            var user = await _repository.GetUserByIdAsync(userId);
            if (user == null)
            {
                throw new ArgumentException("User not found");
            }

            if (!VerifyPassword(request.CurrentPassword, user.Password))
            {
                throw new ArgumentException("Current password is incorrect");
            }

            user.Password = HashPassword(request.NewPassword);
            await _repository.UpdateUserAsync(user);

            return true;
        }

        public async Task<bool> ForgotPasswordAsync(ForgotPasswordDto request)
        {
            var user = await _repository.GetUserByEmailAsync(request.Email);
            if (user == null)
            {
                // Don't reveal if email exists, but return true for security
                return true;
            }

            // In a real application, you would:
            // 1. Generate a password reset token
            // 2. Send email with reset link
            // 3. Store token with expiration

            // For now, just return true
            return true;
        }

        public async Task<bool> ResetPasswordAsync(ResetPasswordDto request)
        {
            // In a real application, you would:
            // 1. Validate the reset token
            // 2. Check if token is not expired
            // 3. Update user password

            var user = await _repository.GetUserByEmailAsync(request.Email);
            if (user == null)
            {
                throw new ArgumentException("User not found");
            }

            // For demo purposes, assuming token validation passed
            user.Password = HashPassword(request.NewPassword);
            await _repository.UpdateUserAsync(user);

            return true;
        }

        public async Task<UserDto?> GetCurrentUserAsync(int userId)
        {
            var user = await _repository.GetUserByIdAsync(userId);
            return user != null ? _mapper.Map<UserDto>(user) : null;
        }

        public async Task<IEnumerable<UserDto>> GetAllUsersAsync()
        {
            // You should have a repository method to fetch all users.
            // If you don't yet, add one like: Task<IEnumerable<User>> GetAllUsersAsync();
            var users = await _repository.GetAllUsersAsync();
            return users.Select(u => _mapper.Map<UserDto>(u));
        }

        public async Task<UserDto> CreateUserByAdminAsync(CreateUserRequestDto request)
        {
            // Check unique fields
            if (await _repository.EmailExistsAsync(request.Email))
                throw new ArgumentException("Email already exists");

            if (await _repository.UsernameExistsAsync(request.Username))
                throw new ArgumentException("Username already exists");

            // Validate role
            var role = await _repository.GetRoleByIdAsync(request.RoleId);
            if (role == null)
                throw new ArgumentException("Invalid role");

            // Map & hash password
            var user = _mapper.Map<User>(request);
            user.Password = HashPassword(request.Password);

            // Persist
            user = await _repository.CreateUserAsync(user);
            user.Role = role;

            // Return DTO (no tokens here)
            return _mapper.Map<UserDto>(user);
        }

        private string GenerateJwtToken(User user)
        {
            var jwtSettings = _configuration.GetSection("JwtSettings");
            var key = Encoding.ASCII.GetBytes(jwtSettings["SecretKey"] ?? "your-secret-key-here-make-it-very-long-and-secure");

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Role, user.RoleId.ToString()),
                new Claim("fullname", user.Fullname)
            };

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddHours(1),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature),
                Issuer = jwtSettings["Issuer"] ?? "IdentityService",
                Audience = jwtSettings["Audience"] ?? "IdentityService"
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        private string GenerateRefreshToken()
        {
            var randomNumber = new byte[32];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomNumber);
            return Convert.ToBase64String(randomNumber);
        }

        private string HashPassword(string password)
        {
            return BCrypt.Net.BCrypt.HashPassword(password);
        }

        private bool VerifyPassword(string password, string hash)
        {
            return BCrypt.Net.BCrypt.Verify(password, hash);
        }
    }
}
