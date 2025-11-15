using Lssctc.ProgramManagement.Accounts.Profiles.Dtos;
using Lssctc.ProgramManagement.Accounts.Profiles.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Lssctc.ProgramManagement.Accounts.Profiles.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class InstructorProfilesController : ControllerBase
    {
        private readonly IInstructorProfilesService _instructorProfilesService;

        public InstructorProfilesController(IInstructorProfilesService instructorProfilesService)
        {
            _instructorProfilesService = instructorProfilesService;
        }

        [HttpGet("instructor/{instructorId:int}")]
        [Authorize(Roles = "Admin, Instructor")]
        public async Task<ActionResult<InstructorProfileDto>> GetInstructorProfile(int instructorId)
        {
            try
            {
                var profile = await _instructorProfilesService.GetInstructorProfile(instructorId);
                if (profile == null)
                {
                    return NotFound("Instructor profile not found.");
                }

                return Ok(profile);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }

        /// <summary>
        /// Lấy thông tin hồ sơ giảng viên bao gồm thông tin người dùng và hồ sơ nghề nghiệp theo ID người dùng
        /// </summary>
        /// <param name="userId">ID của người dùng</param>
        /// <returns>Thông tin hồ sơ giảng viên với thông tin người dùng</returns>
        /// <remarks>
        /// Returns a 404 Not Found response if the user or instructor profile is not found.
        /// Returns a 500 Internal Server Error response in case of an unexpected error.
        /// </remarks>
        [HttpGet("instructor/by-user/{userId:int}")]
        [Authorize(Roles = "Admin, Instructor")]
        public async Task<ActionResult<InstructorProfileWithUserDto>> GetInstructorProfileByUserId(int userId)
        {
            try
            {
                var profile = await _instructorProfilesService.GetInstructorProfileByUserId(userId);
                if (profile == null)
                {
                    return NotFound("User or instructor profile not found.");
                }

                return Ok(profile);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }

        [HttpPut("instructor/{instructorId:int}")]
        [Authorize(Roles = "Admin, Instructor")]
        public async Task<IActionResult> UpdateInstructorProfile(int instructorId, [FromBody] UpdateInstructorProfileDto dto)
        {
            try
            {
                await _instructorProfilesService.UpdateInstructorProfile(instructorId, dto);
                return NoContent();
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                if (ex.Message == "Instructor not found." || 
                    ex.Message == "Instructor profile not found." ||
                    ex.Message == "At least one field must be provided for update.")
                {
                    return BadRequest(ex.Message);
                }
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }
    }
}
