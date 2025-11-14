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

        [HttpPut("instructor/{instructorId:int}")]
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
