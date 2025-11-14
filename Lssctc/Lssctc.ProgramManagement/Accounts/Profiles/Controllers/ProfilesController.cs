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
    public class ProfilesController : ControllerBase
    {
        private readonly IProfilesService _profilesService;

        public ProfilesController(IProfilesService profilesService)
        {
            _profilesService = profilesService;
        }

        [HttpGet("trainee/{traineeId:int}")]
        public async Task<ActionResult<TraineeProfileDto>> GetTraineeProfile(int traineeId)
        {
            try
            {
                var profile = await _profilesService.GetTraineeProfile(traineeId);
                if (profile == null)
                {
                    return NotFound("Trainee profile not found.");
                }

                return Ok(profile);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }

        /// <summary>
        /// Get student profile information including user information and training records by user ID
        /// </summary>
        /// <param name="userId">ID của người dùng</param>
        /// <returns>Thông tin hồ sơ học viên với thông tin người dùng</returns>
        /// <remarks>
        /// Returns a 404 Not Found response if the user or trainee profile is not found.
        /// Returns a 500 Internal Server Error response in case of an unexpected error.
        /// </remarks>
        [HttpGet("trainee/by-user/{userId:int}")]
        public async Task<ActionResult<TraineeProfileWithUserDto>> GetTraineeProfileByUserId(int userId)
        {
            try
            {
                var profile = await _profilesService.GetTraineeProfileByUserId(userId);
                if (profile == null)
                {
                    return NotFound("User or trainee profile not found.");
                }

                return Ok(profile);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }

        /// <summary>
        /// Update user information and student profile by user ID
        /// </summary>
        /// <param name="userId">ID của người dùng</param>
        /// <param name="dto">Thông tin cần cập nhật (PhoneNumber, AvatarUrl, EducationLevel, EducationImageUrl)</param>
        /// <returns>Thông tin hồ sơ học viên đã được cập nhật</returns>
        /// <remarks>
        /// Chỉ có thể cập nhật:
        /// - PhoneNumber và AvatarUrl trong User Profile
        /// - EducationLevel và EducationImageUrl trong Trainee Profile
        /// </remarks>
        [HttpPut("trainee/by-user/{userId:int}")]
        public async Task<ActionResult<TraineeProfileWithUserDto>> UpdateUserAndTraineeProfile(int userId, [FromBody] UpdateUserAndTraineeProfileDto dto)
        {
            try
            {
                var profile = await _profilesService.UpdateUserAndTraineeProfile(userId, dto);
                return Ok(profile);
            }
            catch (Exception ex)
            {
                if (ex.Message == "User not found." || ex.Message == "Trainee not found for this user.")
                {
                    return NotFound(ex.Message);
                }
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }

        [HttpPost("trainee/{traineeId:int}")]
        public async Task<ActionResult<TraineeProfileDto>> CreateTraineeProfile(int traineeId, [FromBody] CreateTraineeProfileDto dto)
        {
            try
            {
                var profile = await _profilesService.CreateTraineeProfile(traineeId, dto);
                return CreatedAtAction(nameof(GetTraineeProfile), new { traineeId = profile.Id }, profile);
            }
            catch (Exception ex)
            {
                if (ex.Message == "Trainee not found." || ex.Message == "Trainee profile already exists.")
                {
                    return BadRequest(ex.Message);
                }
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }

        [HttpPut("trainee/{traineeId:int}")]
        public async Task<IActionResult> UpdateTraineeProfile(int traineeId, [FromBody] UpdateTraineeProfileDto dto)
        {
            try
            {
                await _profilesService.UpdateTraineeProfile(traineeId, dto);
                return NoContent();
            }
            catch (Exception ex)
            {
                if (ex.Message == "Trainee profile not found.")
                {
                    return NotFound(ex.Message);
                }
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }

        [HttpDelete("trainee/{traineeId:int}")]
        public async Task<IActionResult> DeleteTraineeProfile(int traineeId)
        {
            try
            {
                var result = await _profilesService.DeleteTraineeProfile(traineeId);
                if (!result)
                {
                    return NotFound("Trainee profile not found.");
                }
                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }
    }
}
