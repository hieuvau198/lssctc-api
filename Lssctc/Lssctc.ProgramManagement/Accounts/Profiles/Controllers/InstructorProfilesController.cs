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

        /// <summary>
        /// Lấy thông tin InstructorProfile theo instructorId (chỉ profile, không bao gồm user info)
        /// </summary>
        /// <param name="instructorId">ID của instructor</param>
        /// <returns>Thông tin InstructorProfile</returns>
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
        /// Cập nhật InstructorProfile theo instructorId (chỉ profile, không update user info)
        /// </summary>
        /// <param name="instructorId">ID của instructor</param>
        /// <param name="dto">Thông tin profile cần cập nhật</param>
        /// <returns>NoContent nếu thành công</returns>
        /// <remarks>
        /// Chỉ cập nhật InstructorProfile fields:
        /// - ExperienceYears, Biography, ProfessionalProfileUrl, Specialization
        /// </remarks>
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

        /// <summary>
        /// Lấy thông tin hồ sơ giảng viên theo ID người dùng - Truyền userId trực tiếp
        /// </summary>
        /// <param name="userId">ID của người dùng</param>
        /// <returns>Thông tin hồ sơ giảng viên với thông tin người dùng</returns>
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

        /// <summary>
        /// Cập nhật thông tin hồ sơ giảng viên theo ID người dùng - Truyền userId trực tiếp
        /// </summary>
        /// <param name="userId">ID của người dùng</param>
        /// <param name="dto">Thông tin cần cập nhật</param>
        /// <returns>Thông tin hồ sơ giảng viên đã được cập nhật</returns>
        /// <remarks>
        /// Có thể cập nhật:
        /// - User: Username, Email, Fullname, PhoneNumber, AvatarUrl
        /// - Instructor: InstructorCode, HireDate, IsInstructorActive
        /// - Profile: ExperienceYears, Biography, ProfessionalProfileUrl, Specialization
        /// </remarks>
        [HttpPut("instructor/by-user/{userId:int}")]
        [Authorize(Roles = "Admin, Instructor")]
        public async Task<ActionResult<InstructorProfileWithUserDto>> UpdateUserAndInstructorProfile(int userId, [FromBody] UpdateUserAndInstructorProfileDto dto)
        {
            try
            {
                var profile = await _instructorProfilesService.UpdateUserAndInstructorProfile(userId, dto);
                return Ok(profile);
            }
            catch (Exception ex)
            {
                if (ex.Message == "User not found." || ex.Message == "Instructor not found for this user.")
                {
                    return NotFound(ex.Message);
                }
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }
    }
}
