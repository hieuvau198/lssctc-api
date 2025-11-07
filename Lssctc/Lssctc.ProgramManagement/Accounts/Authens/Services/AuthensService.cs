using Lssctc.ProgramManagement.Accounts.Authens.Dtos;
using Lssctc.ProgramManagement.Accounts.Helpers;
using Lssctc.Share.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace Lssctc.ProgramManagement.Accounts.Authens.Services;

public class AuthensService : IAuthensService
{
    private readonly IUnitOfWork _uow;
    private readonly IConfiguration _configuration;
    private readonly IDistributedCache _cache;
    private readonly IGoogleOAuthService _googleAuthService;

    public AuthensService(IUnitOfWork uow, IConfiguration configuration, IDistributedCache cache, IGoogleOAuthService googleAuthService)
    {
        _uow = uow;
        _configuration = configuration;
        _cache = cache;
        _googleAuthService = googleAuthService;
    }

    public async Task<LoginResponseModel> LoginWithUsername(LoginUsernameDto request)
    {
        if (string.IsNullOrEmpty(request.Username) || string.IsNullOrEmpty(request.Password))
        {
            throw new Exception("Username and password are required.");
        }

        var user = await _uow.UserRepository.GetAllAsQueryable()
            .FirstOrDefaultAsync(x => x.Username == request.Username);

        if (user == null)
        {
            throw new Exception("Invalid username or password.");
        }

        // Check for hashed password first
        if (!PasswordHashHandler.VerifyPassword(request.Password, user.Password))
        {
            // If hash fails, check for plain text fallback
            if (user.Password == request.Password)
            {
                user.Password = PasswordHashHandler.HashPassword(request.Password);
                await _uow.SaveChangesAsync();
            }
            else
            {
                throw new Exception("Invalid username or password.");
            }
        }

        var (token, expiresIn) = JwtHandler.GenerateJwtToken(
            user.Username,
            user.Id,
            user.Role,
            _configuration);

        return new LoginResponseModel
        {
            UserName = request.Username,
            AccessToken = token,
            ExpiresIn = expiresIn
        };
    }

    public async Task<LoginResponseModel> LoginWithEmail(LoginEmailDto request)
    {
        if (string.IsNullOrEmpty(request.Email) || string.IsNullOrEmpty(request.Password))
        {
            throw new Exception("Email and password are required.");
        }

        var user = await _uow.UserRepository.GetAllAsQueryable()
            .FirstOrDefaultAsync(x => x.Email.ToLower() == request.Email.ToLower() && !x.IsDeleted);

        if (user == null)
        {
            throw new Exception("Invalid email or password.");
        }

        if (!PasswordHashHandler.VerifyPassword(request.Password, user.Password))
        {
            if (user.Password == request.Password)
            {
                user.Password = PasswordHashHandler.HashPassword(request.Password);
                await _uow.SaveChangesAsync();
            }
            else
            {
                throw new Exception("Invalid email or password.");
            }
        }

        var (token, expiresIn) = JwtHandler.GenerateJwtToken(
            user.Username,
            user.Id,
            user.Role,
            _configuration);

        return new LoginResponseModel
        {
            UserName = user.Username,
            AccessToken = token,
            ExpiresIn = expiresIn
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

        var jti = jwt.Id;
        if (string.IsNullOrEmpty(jti))
        {
            jti = jwt.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Jti)?.Value;
        }

        if (string.IsNullOrEmpty(jti))
        {
            jti = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(accessToken));
        }

        var expires = jwt.ValidTo; // UTC
        var ttl = expires - DateTime.UtcNow;
        if (ttl <= TimeSpan.Zero)
        {
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

    public async Task<LoginResponseModel> LoginWithGoogle(string accessToken)
    {
        // Get info user from access token Google
        var googleUserInfo = await _googleAuthService.ValidateTokenAsync(accessToken);
        // If return == null => Exception
        if (googleUserInfo == null)
            throw new Exception("Invalid email or password.");

        // Check email have exit
        //var account = await _dpUnitOfWork.AccountRepositories.GetByEmailAsync(googleUserInfo.Email);

        throw new Exception("");
    }
}