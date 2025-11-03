using Lssctc.ProgramManagement.Authens.Dtos;
using Lssctc.Share.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace Lssctc.ProgramManagement.Authens.Services
{
    public class AuthensService : IAuthensService
    {
        private readonly IUnitOfWork _uow;
        private readonly IConfiguration _configuration;
        private readonly IDistributedCache _cache;

        public AuthensService(IUnitOfWork uow, IConfiguration configuration, IDistributedCache cache)
        {
            _uow = uow;
            _configuration = configuration;
            _cache = cache;
        }

        public async Task<LoginResponseModel> AuthenLogin(AuthensLoginDto request)
        {
            if (string.IsNullOrEmpty(request.Username) || string.IsNullOrEmpty(request.Password))
            {

                throw new Exception("Username and password are required.");
            }
            var user = await _uow.UserRepository.GetAllAsQueryable()
                .FirstOrDefaultAsync(x => x.Username == request.Username);

            if (user == null || !PasswordHashHandler.VerifyPassword(request.Password, user.Password))
            {
                // Temporary fallback for plain text passwords (remove after hashing all passwords)
                if (user != null && user.Password == request.Password)
                {
                    // Plain text match, but should hash the password
                }
                else
                {
                    throw new Exception("Invalid username or password.");
                }
            }

            var issuer = _configuration["JwtConfig:Issuer"];
            var audience = _configuration["JwtConfig:Audience"];
            var key = _configuration["JwtConfig:Key"];
            var tokenValidityMins = _configuration.GetValue<int>("JwtConfig:TokenValidityInMinutes");
            var tokenExpriryTimeStamp = DateTime.UtcNow.AddMinutes(tokenValidityMins);

            // create a jti so we can revoke the token later without DB
            var jti = Guid.NewGuid().ToString();

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new System.Security.Claims.ClaimsIdentity(new[]
                {
                    new Claim(JwtRegisteredClaimNames.Name, request.Username),
                    new Claim(JwtRegisteredClaimNames.Jti, jti)
                }),
                Expires = tokenExpriryTimeStamp,
                Issuer = issuer,
                Audience = audience,
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(key)), SecurityAlgorithms.HmacSha256Signature)
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);
            var jwtToken = tokenHandler.WriteToken(token);

            return new LoginResponseModel
            {
                UserName = request.Username,
                AccessToken = jwtToken,
                ExpiresIn = tokenValidityMins * 60
            };
        }

        public async Task<bool> Logout(string accessToken)
        {
            if (string.IsNullOrEmpty(accessToken))
                return false;

            var handler = new JwtSecurityTokenHandler();
            JwtSecurityToken jwt;
            try
            {
                jwt = handler.ReadJwtToken(accessToken);
            }
            catch
            {
                return false;
            }

            // try get jti claim
            var jti = jwt.Id;
            if (string.IsNullOrEmpty(jti))
            {
                jti = jwt.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Jti)?.Value;
            }

            // fallback to hashed token string if no jti
            if (string.IsNullOrEmpty(jti))
            {
                jti = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(accessToken));
            }

            var expires = jwt.ValidTo; // UTC
            var ttl = expires - DateTime.UtcNow;
            if (ttl <= TimeSpan.Zero)
            {
                // token already expired
                return true;
            }

            var cacheKey = $"revoked_jti:{jti}";
            var options = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = ttl
            };

            await _cache.SetStringAsync(cacheKey, "1", options);

            return true;
        }
    }
}
