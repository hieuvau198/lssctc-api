using Lssctc.Share.Enums;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Lssctc.ProgramManagement.Accounts.Helpers
{
    public static class JwtHandler
    {
        public static (string Token, int ExpiresInSeconds) GenerateJwtToken(
            string username,
            int userId,
            int? userRole,
            IConfiguration configuration)
        {
            var issuer = configuration["JwtConfig:Issuer"];
            var audience = configuration["JwtConfig:Audience"];
            var key = configuration["JwtConfig:Key"] ?? "JWT Key Not Found";
            var tokenValidityMins = configuration.GetValue<int>("JwtConfig:TokenValidityInMinutes");

            if (tokenValidityMins <= 0)
            {
                tokenValidityMins = 180;
            }

            var tokenExpriryTimeStamp = DateTime.UtcNow.AddMinutes(tokenValidityMins);
            var jti = Guid.NewGuid().ToString();

            string roleName = "Unknown";
            if (userRole.HasValue && Enum.IsDefined(typeof(UserRoleEnum), userRole.Value))
            {
                roleName = ((UserRoleEnum)userRole.Value).ToString();
            }

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(JwtRegisteredClaimNames.Name, username),
                    new Claim(JwtRegisteredClaimNames.Jti, jti),
                    new Claim(ClaimTypes.NameIdentifier, userId.ToString()), // This is the User's ID
                    new Claim(ClaimTypes.Role, roleName) // This now correctly uses "Admin", "Trainee", etc.
                }),
                Expires = tokenExpriryTimeStamp,
                Issuer = issuer,
                Audience = audience,
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)), SecurityAlgorithms.HmacSha256Signature)
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);
            var jwtToken = tokenHandler.WriteToken(token);

            return (Token: jwtToken, ExpiresInSeconds: tokenValidityMins * 60);
        }
    }
}
