using Lssctc.ProgramManagement.Accounts.Authens.Dtos;
using Lssctc.ProgramManagement.Accounts.Authens.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Lssctc.ProgramManagement.Accounts.Authens.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthensController : ControllerBase
    {
        private readonly IAuthensService _authensService;

        public AuthensController(IAuthensService authensService)
        {
            _authensService = authensService;
        }

        [HttpPost("login-username")]
        public async Task<IActionResult> LoginWithUsername([FromBody] LoginUsernameDto request)
        {
            try
            {
                var result = await _authensService.LoginWithUsername(request);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("login-email")]
        public async Task<IActionResult> LoginWithEmail([FromBody] LoginEmailDto request)
        {
            try
            {
                var result = await _authensService.LoginWithEmail(request);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            try
            {
                var authHeader = Request.Headers["Authorization"].ToString();
                if (string.IsNullOrEmpty(authHeader) || !authHeader.StartsWith("Bearer "))
                {
                    return BadRequest(new { message = "Authorization header with Bearer token is required." });
                }

                var token = authHeader[7..].Trim();
                var result = await _authensService.Logout(token);
                if (result)
                    return Ok(new { message = "Logged out" });

                return BadRequest(new { message = "Logout failed" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("login-google", Name = "LoginGoogleCommand")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> LoginGoogle([FromBody] LoginGoogleRequestDto request)
        {
            var result = await _authensService.LoginWithGoogle(request.AccessToken);

            return Ok("");
        }
    }
}
