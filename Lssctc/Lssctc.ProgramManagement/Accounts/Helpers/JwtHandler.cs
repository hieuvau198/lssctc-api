using Lssctc.Share.Enums;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Lssctc.ProgramManagement.Accounts.Helpers
{
    public static class JwtHandler
    {
        // We return a simple tuple with the token string and its validity in seconds
        public static (string Token, int ExpiresInSeconds) GenerateJwtToken(
            string username,
            int userId,
            int? userRole,
            string? fullname,   // <--- Added parameter
            string? email,      // <--- Added parameter
            string? avatarUrl,  // <--- Added parameter
            IConfiguration configuration)
        {
            var issuer = configuration["JwtConfig:Issuer"];
            var audience = configuration["JwtConfig:Audience"];
            var key = configuration["JwtConfig:Key"] ?? "JWT Key Not Found";
            var tokenValidityMins = configuration.GetValue<int>("JwtConfig:TokenValidityInMinutes");

            // Default to 60 minutes if config is missing or invalid
            if (tokenValidityMins <= 0)
            {
                tokenValidityMins = 180;
            }

            var tokenExpriryTimeStamp = DateTime.UtcNow.AddMinutes(tokenValidityMins);
            var jti = Guid.NewGuid().ToString();

            // Get the string name (e.g., "Trainee") from the integer value (e.g., 4)
            string roleName = "Unknown";
            if (userRole.HasValue && Enum.IsDefined(typeof(UserRoleEnum), userRole.Value))
            {
                roleName = ((UserRoleEnum)userRole.Value).ToString();
            }

            // Create the list of claims
            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Name, username),
                new Claim(JwtRegisteredClaimNames.Jti, jti),
                new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
                new Claim(ClaimTypes.Role, roleName)
            };

            // --- NEW CLAIMS ADDED HERE ---
            if (!string.IsNullOrEmpty(email))
            {
                claims.Add(new Claim(ClaimTypes.Email, email));
            }

            if (!string.IsNullOrEmpty(fullname))
            {
                claims.Add(new Claim("FullName", fullname));
            }

            if (!string.IsNullOrEmpty(avatarUrl))
            {
                claims.Add(new Claim("AvatarUrl", avatarUrl));
            }
            // -----------------------------

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
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