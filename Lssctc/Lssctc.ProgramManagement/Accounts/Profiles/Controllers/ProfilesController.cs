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
