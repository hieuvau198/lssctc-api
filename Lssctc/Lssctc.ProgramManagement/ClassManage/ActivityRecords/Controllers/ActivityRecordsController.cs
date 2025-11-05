using Lssctc.ProgramManagement.ClassManage.ActivityRecords.Dtos;
using Lssctc.ProgramManagement.ClassManage.ActivityRecords.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Lssctc.ProgramManagement.ClassManage.ActivityRecords.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ActivityRecordsController : ControllerBase
    {
        private readonly IActivityRecordsService _activityRecordsService;

        public ActivityRecordsController(IActivityRecordsService activityRecordsService)
        {
            _activityRecordsService = activityRecordsService;
        }

        [HttpGet("my-records/class/{classId}/section/{sectionId}")]
        [Authorize(Roles = "Trainee")]
        public async Task<IActionResult> GetMyActivityRecords(int classId, int sectionId)
        {
            try
            {
                var traineeId = GetUserIdFromClaims();
                var result = await _activityRecordsService.GetActivityRecordsAsync(classId, sectionId, traineeId);
                return Ok(result);
            }
            catch (UnauthorizedAccessException ex) { return Unauthorized(new { message = ex.Message }); }
            catch (Exception) { return StatusCode(500, new { message = "An unexpected error occurred." }); }
        }

        [HttpPost("my-records/submit")]
        [Authorize(Roles = "Trainee")]
        public async Task<IActionResult> SubmitActivity(SubmitActivityRecordDto dto)
        {
            try
            {
                var traineeId = GetUserIdFromClaims();
                var result = await _activityRecordsService.SubmitActivityAsync(traineeId, dto);
                return Ok(result);
            }
            catch (UnauthorizedAccessException ex) { return Unauthorized(new { message = ex.Message }); }
            catch (KeyNotFoundException ex) { return NotFound(new { message = ex.Message }); }
            catch (InvalidOperationException ex) { return BadRequest(new { message = ex.Message }); }
            catch (Exception) { return StatusCode(500, new { message = "An unexpected error occurred." }); }
        }


        [HttpGet("class/{classId}/section/{sectionId}/activity/{activityId}")]
        [Authorize(Roles = "Admin, Instructor")]
        public async Task<IActionResult> GetActivityRecordsByActivity(int classId, int sectionId, int activityId)
        {
            try
            {
                var result = await _activityRecordsService.GetActivityRecordsByActivityAsync(classId, sectionId, activityId);
                return Ok(result);
            }
            catch (Exception) { return StatusCode(500, new { message = "An unexpected error occurred." }); }
        }

        [HttpPost("{activityRecordId}/feedback")]
        [Authorize(Roles = "Admin, Instructor")]
        public async Task<IActionResult> AddFeedback(int activityRecordId, InstructorFeedbackDto dto)
        {
            try
            {
                var instructorId = GetUserIdFromClaims();
                var result = await _activityRecordsService.AddFeedbackAsync(activityRecordId, instructorId, dto);
                return CreatedAtAction(nameof(GetFeedbacks), new { activityRecordId = result.ActivityRecordId }, result);
            }
            catch (UnauthorizedAccessException ex) { return Unauthorized(new { message = ex.Message }); }
            catch (KeyNotFoundException ex) { return NotFound(new { message = ex.Message }); }
            catch (ArgumentException ex) { return BadRequest(new { message = ex.Message }); }
            catch (Exception) { return StatusCode(500, new { message = "An unexpected error occurred." }); }
        }

        [HttpGet("{activityRecordId}/feedback")]
        [Authorize(Roles = "Admin, Instructor, Trainee")]
        public async Task<IActionResult> GetFeedbacks(int activityRecordId)
        {
            try
            {
                var result = await _activityRecordsService.GetFeedbacksAsync(activityRecordId);
                return Ok(result);
            }
            catch (KeyNotFoundException ex) { return NotFound(new { message = ex.Message }); }
            catch (Exception) { return StatusCode(500, new { message = "An unexpected error occurred." }); }
        }

        private int GetUserIdFromClaims()
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (int.TryParse(userIdClaim, out int userId))
            {
                return userId;
            }

            throw new UnauthorizedAccessException("User ID claim is missing or invalid.");
        }
    }
}
