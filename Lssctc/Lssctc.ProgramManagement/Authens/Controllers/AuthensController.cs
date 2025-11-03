using Lssctc.ProgramManagement.Authens.Dtos;
using Lssctc.ProgramManagement.Authens.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Lssctc.ProgramManagement.Authens.Controllers
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

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] AuthensLoginDto request)
        {
            try
            {
                var result = await _authensService.AuthenLogin(request);
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
    }
}
