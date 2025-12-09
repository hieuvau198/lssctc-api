// File: Lssctc.ProgramManagement/Activities/Controllers/ActivitySessionsController.cs
using Lssctc.ProgramManagement.Activities.Dtos;
using Lssctc.ProgramManagement.Activities.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

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

        // ... [Keep existing Create/Update/Get APIs] ...

        /// <summary>
        /// DEBUG API: Manually triggers the generation of Activity Sessions for a class.
        /// Useful if sessions were missing or class was started before logic implementation.
        /// </summary>
        [HttpPost("test-create-sessions/{classId}")]
        [Authorize(Roles = "Admin, Instructor")]
        public async Task<IActionResult> TriggerCreateSessionsManual(int classId)
        {
            try
            {
                await _sessionService.CreateSessionsOnClassStartAsync(classId);
                return Ok(new { message = $"Successfully triggered session creation for Class ID {classId}." });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"An error occurred: {ex.Message}" });
            }
        }

        // ... [Rest of the controller] ...

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