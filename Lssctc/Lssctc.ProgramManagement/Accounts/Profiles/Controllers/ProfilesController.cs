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
                    return NotFound(new { Message = "Không tìm thấy hồ sơ học viên." });
                }

                return Ok(profile);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { Message = "Đã xảy ra lỗi không mong muốn: " + ex.Message });
            }
        }

        [HttpGet("trainee/by-user/{userId:int}")]
        public async Task<ActionResult<TraineeProfileWithUserDto>> GetTraineeProfileByUserId(int userId)
        {
            try
            {
                var profile = await _profilesService.GetTraineeProfileByUserId(userId);
                if (profile == null)
                {
                    return NotFound(new { Message = "Không tìm thấy hồ sơ học viên hoặc người dùng." });
                }

                return Ok(profile);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { Message = "Đã xảy ra lỗi không mong muốn: " + ex.Message });
            }
        }

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
                if (ex.Message == "Không tìm thấy người dùng." || ex.Message == "Không tìm thấy thông tin học viên cho người dùng này.")
                {
                    return NotFound(new { Message = ex.Message });
                }
                if (ex.Message == "Số điện thoại đã tồn tại trong hệ thống.")
                {
                    return BadRequest(new { Message = ex.Message });
                }
                return StatusCode(StatusCodes.Status500InternalServerError, new { Message = "Đã xảy ra lỗi không mong muốn: " + ex.Message });
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
                if (ex.Message == "Không tìm thấy học viên." || ex.Message == "Hồ sơ học viên đã tồn tại.")
                {
                    return BadRequest(new { Message = ex.Message });
                }
                return StatusCode(StatusCodes.Status500InternalServerError, new { Message = "Đã xảy ra lỗi không mong muốn: " + ex.Message });
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
                if (ex.Message == "Không tìm thấy hồ sơ học viên.")
                {
                    return NotFound(new { Message = ex.Message });
                }
                return StatusCode(StatusCodes.Status500InternalServerError, new { Message = "Đã xảy ra lỗi không mong muốn: " + ex.Message });
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
                    return NotFound(new { Message = "Không tìm thấy hồ sơ học viên." });
                }
                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { Message = "Đã xảy ra lỗi không mong muốn: " + ex.Message });
            }
        }
    }
}