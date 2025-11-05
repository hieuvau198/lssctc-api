using Lssctc.ProgramManagement.ClassManage.Progresses.Dtos;
using Lssctc.ProgramManagement.ClassManage.Progresses.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Lssctc.ProgramManagement.ClassManage.Progresses.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProgressController : ControllerBase
    {
        private readonly IProgressesService _progressesService;

        public ProgressController(IProgressesService progressesService)
        {
            _progressesService = progressesService;
        }

        #region === Trainee Endpoints ===

        [HttpGet("my-progress")]
        [Authorize(Roles = "Trainee")]
        public async Task<IActionResult> GetMyProgresses()
        {
            try
            {
                var traineeId = GetTraineeIdFromClaims();
                var result = await _progressesService.GetAllProgressesByTraineeIdAsync(traineeId);
                return Ok(result);
            }
            catch (UnauthorizedAccessException ex) { return Unauthorized(new { message = ex.Message }); }
            catch (Exception) { return StatusCode(500, new { message = "An unexpected error occurred." }); }
        }

        [HttpGet("my-progress/{progressId}")]
        [Authorize(Roles = "Trainee")]
        public async Task<IActionResult> GetMyProgressById(int progressId)
        {
            try
            {
                var traineeId = GetTraineeIdFromClaims();
                var result = await _progressesService.GetProgressByIdAsync(progressId);

                if (result == null)
                    return NotFound();

                if (result.TraineeId != traineeId)
                    return Forbid("You are not authorized to view this progress.");

                return Ok(result);
            }
            catch (UnauthorizedAccessException ex) { return Unauthorized(new { message = ex.Message }); }
            catch (Exception) { return StatusCode(500, new { message = "An unexpected error occurred." }); }
        }

        [HttpGet("my-progress/class/{classId}")]
        [Authorize(Roles = "Trainee")]
        public async Task<IActionResult> GetMyProgressByClass(int classId)
        {
            try
            {
                var traineeId = GetTraineeIdFromClaims();
                var result = await _progressesService.GetProgressByClassAndTraineeAsync(classId, traineeId);
                if (result == null)
                    return NotFound();
                return Ok(result);
            }
            catch (UnauthorizedAccessException ex) { return Unauthorized(new { message = ex.Message }); }
            catch (Exception) { return StatusCode(500, new { message = "An unexpected error occurred." }); }
        }

        [HttpPut("my-progress/{progressId}/start")]
        [Authorize(Roles = "Trainee")]
        public async Task<IActionResult> StartMyProgress(int progressId)
        {
            try
            {
                var traineeId = GetTraineeIdFromClaims();
                var progress = await _progressesService.GetProgressByIdAsync(progressId);

                if (progress == null)
                    return NotFound(new { message = "Progress not found." });

                if (progress.TraineeId != traineeId)
                    return Forbid("You are not authorized to start this progress.");

                var result = await _progressesService.StartProgressAsync(progressId);
                return Ok(result);
            }
            catch (UnauthorizedAccessException ex) { return Unauthorized(new { message = ex.Message }); }
            catch (KeyNotFoundException ex) { return NotFound(new { message = ex.Message }); }
            catch (InvalidOperationException ex) { return BadRequest(new { message = ex.Message }); }
            catch (Exception) { return StatusCode(500, new { message = "An unexpected error occurred." }); }
        }

        #endregion

        #region === Admin/Instructor Endpoints ===

        [HttpGet("{progressId}")]
        [Authorize(Roles = "Admin, Instructor")]
        public async Task<IActionResult> GetProgressById(int progressId)
        {
            try
            {
                var result = await _progressesService.GetProgressByIdAsync(progressId);
                if (result == null)
                    return NotFound();
                return Ok(result);
            }
            catch (Exception) { return StatusCode(500, new { message = "An unexpected error occurred." }); }
        }

        [HttpGet("class/{classId}")]
        [Authorize(Roles = "Admin, Instructor")]
        public async Task<IActionResult> GetAllProgressesByClassId(int classId)
        {
            try
            {
                var result = await _progressesService.GetAllProgressesByClassIdAsync(classId);
                return Ok(result);
            }
            catch (Exception) { return StatusCode(500, new { message = "An unexpected error occurred." }); }
        }

        [HttpGet("trainee/{traineeId}")]
        [Authorize(Roles = "Admin, Instructor")]
        public async Task<IActionResult> GetAllProgressesByTraineeId(int traineeId)
        {
            try
            {
                var result = await _progressesService.GetAllProgressesByTraineeIdAsync(traineeId);
                return Ok(result);
            }
            catch (Exception) { return StatusCode(500, new { message = "An unexpected error occurred." }); }
        }

        [HttpGet("class/{classId}/trainee/{traineeId}")]
        [Authorize(Roles = "Admin, Instructor")]
        public async Task<IActionResult> GetProgressByClassAndTrainee(int classId, int traineeId)
        {
            try
            {
                var result = await _progressesService.GetProgressByClassAndTraineeAsync(classId, traineeId);
                if (result == null)
                    return NotFound();
                return Ok(result);
            }
            catch (Exception) { return StatusCode(500, new { message = "An unexpected error occurred." }); }
        }

        [HttpPost]
        [Authorize(Roles = "Admin, Instructor")]
        public async Task<IActionResult> CreateProgress([FromBody] CreateProgressDto dto)
        {
            try
            {
                var result = await _progressesService.CreateProgressAsync(dto);
                return CreatedAtAction(nameof(GetProgressById), new { progressId = result.Id }, result);
            }
            catch (KeyNotFoundException ex) { return NotFound(new { message = ex.Message }); }
            catch (InvalidOperationException ex) { return BadRequest(new { message = ex.Message }); }
            catch (Exception) { return StatusCode(500, new { message = "An unexpected error occurred." }); }
        }

        [HttpPost("class/{classId}/trainee/{traineeId}")]
        [Authorize(Roles = "Admin, Instructor")]
        public async Task<IActionResult> CreateProgressForTrainee(int classId, int traineeId)
        {
            try
            {
                var result = await _progressesService.CreateProgressForTraineeAsync(classId, traineeId);
                return CreatedAtAction(nameof(GetProgressById), new { progressId = result.Id }, result);
            }
            catch (KeyNotFoundException ex) { return NotFound(new { message = ex.Message }); }
            catch (InvalidOperationException ex) { return BadRequest(new { message = ex.Message }); }
            catch (Exception) { return StatusCode(500, new { message = "An unexpected error occurred." }); }
        }

        [HttpPost("class/{classId}/create-all")]
        [Authorize(Roles = "Admin, Instructor")]
        public async Task<IActionResult> CreateProgressesForClass(int classId)
        {
            try
            {
                var result = await _progressesService.CreateProgressesForClassAsync(classId);
                return Ok(result);
            }
            catch (KeyNotFoundException ex) { return NotFound(new { message = ex.Message }); }
            catch (Exception) { return StatusCode(500, new { message = "An unexpected error occurred." }); }
        }

        [HttpPut("{progressId}")]
        [Authorize(Roles = "Admin, Instructor")]
        public async Task<IActionResult> UpdateProgress(int progressId, [FromBody] UpdateProgressDto dto)
        {
            try
            {
                var result = await _progressesService.UpdateProgressAsync(progressId, dto);
                return Ok(result);
            }
            catch (KeyNotFoundException ex) { return NotFound(new { message = ex.Message }); }
            catch (Exception) { return StatusCode(500, new { message = "An unexpected error occurred." }); }
        }

        [HttpPut("{progressId}/calculate-percentage")]
        [Authorize(Roles = "Admin, Instructor")]
        public async Task<IActionResult> UpdateProgressPercentage(int progressId)
        {
            try
            {
                var result = await _progressesService.UpdateProgressPercentageAsync(progressId);
                return Ok(result);
            }
            catch (KeyNotFoundException ex) { return NotFound(new { message = ex.Message }); }
            catch (Exception) { return StatusCode(500, new { message = "An unexpected error occurred." }); }
        }

        [HttpPut("{progressId}/complete")]
        [Authorize(Roles = "Admin, Instructor")]
        public async Task<IActionResult> CompleteProgress(int progressId)
        {
            try
            {
                var result = await _progressesService.CompleteProgressAsync(progressId);
                return Ok(result);
            }
            catch (KeyNotFoundException ex) { return NotFound(new { message = ex.Message }); }
            catch (InvalidOperationException ex) { return BadRequest(new { message = ex.Message }); }
            catch (Exception) { return StatusCode(500, new { message = "An unexpected error occurred." }); }
        }

        [HttpPut("{progressId}/fail")]
        [Authorize(Roles = "Admin, Instructor")]
        public async Task<IActionResult> FailProgress(int progressId)
        {
            try
            {
                var result = await _progressesService.FailProgressAsync(progressId);
                return Ok(result);
            }
            catch (KeyNotFoundException ex) { return NotFound(new { message = ex.Message }); }
            catch (InvalidOperationException ex) { return BadRequest(new { message = ex.Message }); }
            catch (Exception) { return StatusCode(500, new { message = "An unexpected error occurred." }); }
        }

        [HttpPut("class/{classId}/start-all")]
        [Authorize(Roles = "Admin, Instructor")]
        public async Task<IActionResult> StartAllProgresses(int classId)
        {
            try
            {
                var count = await _progressesService.StartAllProgressesAsync(classId);
                return Ok(new { message = $"Successfully started {count} progresses." });
            }
            catch (KeyNotFoundException ex) { return NotFound(new { message = ex.Message }); }
            catch (InvalidOperationException ex) { return BadRequest(new { message = ex.Message }); }
            catch (Exception) { return StatusCode(500, new { message = "An unexpected error occurred." }); }
        }

        [HttpPut("class/{classId}/complete-all")]
        [Authorize(Roles = "Admin, Instructor")]
        public async Task<IActionResult> CompleteAllProgresses(int classId)
        {
            try
            {
                var count = await _progressesService.CompleteAllProgressesAsync(classId);
                return Ok(new { message = $"Successfully completed {count} progresses." });
            }
            catch (KeyNotFoundException ex) { return NotFound(new { message = ex.Message }); }
            catch (InvalidOperationException ex) { return BadRequest(new { message = ex.Message }); }
            catch (Exception) { return StatusCode(500, new { message = "An unexpected error occurred." }); }
        }

        #endregion

        #region === Private Helpers ===

        private int GetTraineeIdFromClaims()
        {
            var traineeIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (int.TryParse(traineeIdClaim, out int traineeId))
            {
                return traineeId;
            }

            throw new UnauthorizedAccessException("Trainee ID claim is missing or invalid.");
        }

        #endregion
    }
}
