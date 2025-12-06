using Lssctc.ProgramManagement.Activities.Dtos;
using Lssctc.ProgramManagement.Activities.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Lssctc.ProgramManagement.Activities.Controllers
{
    [Route("api/activity-sessions")]
    [ApiController]
    [Authorize]
    public class ActivitySessionsController : ControllerBase
    {
        private readonly IActivitySessionService _sessionService;

        public ActivitySessionsController(IActivitySessionService sessionService)
        {
            _sessionService = sessionService;
        }

        /// <summary>
        /// API tạo một ActivitySession mới (Task 3).
        /// Role: Admin, Instructor
        /// </summary>
        [HttpPost]
        [Authorize(Roles = "Admin, Instructor")]
        public async Task<IActionResult> CreateSession([FromBody] CreateActivitySessionDto dto)
        {
            try
            {
                if (!ModelState.IsValid) return BadRequest(ModelState);
                var result = await _sessionService.CreateActivitySessionAsync(dto);
                return CreatedAtAction(nameof(GetSessionById), new { sessionId = result.Id }, result);
            }
            catch (InvalidOperationException ex) { return BadRequest(new { message = ex.Message }); }
            catch (KeyNotFoundException ex) { return NotFound(new { message = ex.Message }); }
            catch (Exception) { return StatusCode(500, new { message = "An unexpected error occurred." }); }
        }

        /// <summary>
        /// API chỉnh sửa ActivitySession (Task 4 - kích hoạt/thiết lập thời gian).
        /// Role: Admin, Instructor
        /// </summary>
        [HttpPut("{sessionId}")]
        [Authorize(Roles = "Admin, Instructor")]
        public async Task<IActionResult> UpdateSession(int sessionId, [FromBody] UpdateActivitySessionDto dto)
        {
            try
            {
                if (!ModelState.IsValid) return BadRequest(ModelState);
                var result = await _sessionService.UpdateActivitySessionAsync(sessionId, dto);
                return Ok(result);
            }
            catch (KeyNotFoundException ex) { return NotFound(new { message = ex.Message }); }
            catch (Exception) { return StatusCode(500, new { message = "An unexpected error occurred." }); }
        }

        /// <summary>
        /// Lấy thông tin chi tiết một Activity Session.
        /// </summary>
        [HttpGet("{sessionId}")]
        [Authorize(Roles = "Admin, Instructor")]
        public async Task<IActionResult> GetSessionById(int sessionId)
        {
            try
            {
                var result = await _sessionService.GetActivitySessionByIdAsync(sessionId);
                return Ok(result);
            }
            catch (KeyNotFoundException ex) { return NotFound(new { message = ex.Message }); }
            catch (Exception) { return StatusCode(500, new { message = "An unexpected error occurred." }); }
        }

        /// <summary>
        /// Lấy danh sách tất cả Activity Sessions cho một lớp.
        /// </summary>
        [HttpGet("class/{classId}")]
        [Authorize(Roles = "Admin, Instructor")]
        public async Task<IActionResult> GetSessionsByClassId(int classId)
        {
            try
            {
                var result = await _sessionService.GetActivitySessionsByClassIdAsync(classId);
                return Ok(result);
            }
            catch (Exception) { return StatusCode(500, new { message = "An unexpected error occurred." }); }
        }
    }
}