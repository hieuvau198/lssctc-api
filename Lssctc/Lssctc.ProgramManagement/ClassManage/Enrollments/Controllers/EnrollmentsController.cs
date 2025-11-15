using Lssctc.ProgramManagement.ClassManage.Enrollments.Dtos;
using Lssctc.ProgramManagement.ClassManage.Enrollments.Services;
using Lssctc.Share.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Lssctc.ProgramManagement.ClassManage.Enrollments.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class EnrollmentsController : ControllerBase
    {
        private readonly IEnrollmentsService _enrollmentsService;

        public EnrollmentsController(IEnrollmentsService enrollmentsService)
        {
            _enrollmentsService = enrollmentsService;
        }

        #region Trainee Enrollments

        [HttpPost("my-enrollments")]
        [Authorize(Roles = "Trainee")]
        [ProducesResponseType(typeof(EnrollmentDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> EnrollInClass([FromBody] CreateEnrollmentDto dto)
        {
            try
            {
                var traineeId = GetTraineeIdFromClaims();
                var result = await _enrollmentsService.EnrollInClassAsync(traineeId, dto);

                // Return 201 Created with a link to get the new resource
                return CreatedAtAction(nameof(GetMyEnrollmentById), new { enrollmentId = result.Id }, result);
            }
            catch (UnauthorizedAccessException ex) { return Unauthorized(new { message = ex.Message }); }
            catch (KeyNotFoundException ex) { return NotFound(new { message = ex.Message }); }
            catch (InvalidOperationException ex) { return BadRequest(new { message = ex.Message }); }
            catch (ArgumentException ex) { return BadRequest(new { message = ex.Message }); }
            catch (Exception) { return StatusCode(500, new { message = "An unexpected error occurred." }); }
        }

        [HttpDelete("my-enrollments/class/{classId}")]
        [Authorize(Roles = "Trainee")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> WithdrawFromClass(int classId)
        {
            try
            {
                var traineeId = GetTraineeIdFromClaims();
                await _enrollmentsService.WithdrawFromClassAsync(traineeId, classId);
                return NoContent();
            }
            catch (UnauthorizedAccessException ex) { return Unauthorized(new { message = ex.Message }); }
            catch (KeyNotFoundException ex) { return NotFound(new { message = ex.Message }); }
            catch (InvalidOperationException ex) { return BadRequest(new { message = ex.Message }); }
            catch (Exception) { return StatusCode(500, new { message = "An unexpected error occurred." }); }
        }

        [HttpGet("my-enrollments")]
        [Authorize(Roles = "Trainee")]
        [ProducesResponseType(typeof(IEnumerable<EnrollmentDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetMyEnrollments()
        {
            try
            {
                var traineeId = GetTraineeIdFromClaims();
                var result = await _enrollmentsService.GetMyEnrollmentsAsync(traineeId);
                return Ok(result);
            }
            catch (UnauthorizedAccessException ex) { return Unauthorized(new { message = ex.Message }); }
            catch (Exception) { return StatusCode(500, new { message = "An unexpected error occurred." }); }
        }

        [HttpGet("my-enrollments/paged")]
        [Authorize(Roles = "Trainee")]
        [ProducesResponseType(typeof(PagedResult<EnrollmentDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetMyEnrollmentsPaged([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            try
            {
                var traineeId = GetTraineeIdFromClaims();
                var result = await _enrollmentsService.GetMyEnrollmentsAsync(traineeId, pageNumber, pageSize);
                return Ok(result);
            }
            catch (UnauthorizedAccessException ex) { return Unauthorized(new { message = ex.Message }); }
            catch (Exception) { return StatusCode(500, new { message = "An unexpected error occurred." }); }
        }

        [HttpGet("my-enrollments/{enrollmentId}", Name = "GetMyEnrollmentById")]
        [Authorize(Roles = "Trainee")]
        [ProducesResponseType(typeof(EnrollmentDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetMyEnrollmentById(int enrollmentId)
        {
            try
            {
                var traineeId = GetTraineeIdFromClaims();
                var result = await _enrollmentsService.GetMyEnrollmentByIdAsync(traineeId, enrollmentId);
                if (result == null) return NotFound();
                return Ok(result);
            }
            catch (UnauthorizedAccessException ex) { return Unauthorized(new { message = ex.Message }); }
            catch (Exception) { return StatusCode(500, new { message = "An unexpected error occurred." }); }
        }

        [HttpGet("my-enrollments/class/{classId}")]
        [Authorize(Roles = "Trainee")]
        [ProducesResponseType(typeof(EnrollmentDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetMyEnrollmentByClassId(int classId)
        {
            try
            {
                var traineeId = GetTraineeIdFromClaims();
                var result = await _enrollmentsService.GetMyEnrollmentByClassIdAsync(traineeId, classId);
                if (result == null) return NotFound();
                return Ok(result);
            }
            catch (UnauthorizedAccessException ex) { return Unauthorized(new { message = ex.Message }); }
            catch (Exception) { return StatusCode(500, new { message = "An unexpected error occurred." }); }
        }

        #endregion

        #region === Admin/Instructor Endpoints ===

        [HttpGet("class/{classId}")]
        [Authorize(Roles = "Admin, Instructor")]
        [ProducesResponseType(typeof(PagedResult<EnrollmentDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetEnrollmentsForClass(int classId, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            try
            {
                var result = await _enrollmentsService.GetEnrollmentsForClassAsync(classId, pageNumber, pageSize);
                return Ok(result);
            }
            catch (Exception) { return StatusCode(500, new { message = "An unexpected error occurred." }); }
        }

        [HttpPut("{enrollmentId}/approve")]
        [Authorize(Roles = "Admin, Instructor")]
        [ProducesResponseType(typeof(EnrollmentDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> ApproveEnrollment(int enrollmentId)
        {
            try
            {
                var result = await _enrollmentsService.ApproveEnrollmentAsync(enrollmentId);
                return Ok(result);
            }
            catch (KeyNotFoundException ex) { return NotFound(new { message = ex.Message }); }
            catch (InvalidOperationException ex) { return BadRequest(new { message = ex.Message }); }
            catch (Exception) { return StatusCode(500, new { message = "An unexpected error occurred." }); }
        }

        [HttpPut("{enrollmentId}/reject")]
        [Authorize(Roles = "Admin, Instructor")]
        [ProducesResponseType(typeof(EnrollmentDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> RejectEnrollment(int enrollmentId)
        {
            try
            {
                var result = await _enrollmentsService.RejectEnrollmentAsync(enrollmentId);
                return Ok(result);
            }
            catch (KeyNotFoundException ex) { return NotFound(new { message = ex.Message }); }
            catch (InvalidOperationException ex) { return BadRequest(new { message = ex.Message }); }
            catch (Exception) { return StatusCode(500, new { message = "An unexpected error occurred." }); }
        }

        [HttpPost]
        [Authorize(Roles = "Admin, Instructor")]
        [ProducesResponseType(typeof(EnrollmentDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> AddTraineeToClass([FromBody] InstructorAddTraineeDto dto)
        {
            try
            {
                var result = await _enrollmentsService.AddTraineeToClassAsync(dto);
                return Ok(result); // Or Created... if you have a "GetByIdForAdmin" endpoint
            }
            catch (KeyNotFoundException ex) { return NotFound(new { message = ex.Message }); }
            catch (InvalidOperationException ex) { return BadRequest(new { message = ex.Message }); }
            catch (ArgumentException ex) { return BadRequest(new { message = ex.Message }); }
            catch (Exception) { return StatusCode(500, new { message = "An unexpected error occurred." }); }
        }

        [HttpDelete("{enrollmentId}")]
        [Authorize(Roles = "Admin, Instructor")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> RemoveTraineeFromClass(int enrollmentId)
        {
            try
            {
                await _enrollmentsService.RemoveTraineeFromClassAsync(enrollmentId);
                return NoContent();
            }
            catch (KeyNotFoundException ex) { return NotFound(new { message = ex.Message }); }
            catch (InvalidOperationException ex) { return BadRequest(new { message = ex.Message }); }
            catch (Exception) { return StatusCode(500, new { message = "An unexpected error occurred." }); }
        }

        #endregion

        #region === Private Helpers ===

        private int GetTraineeIdFromClaims()
        {
            // This assumes your auth token has a claim for TraineeId or uses the standard NameIdentifier.
            // Adjust "traineeId" or ClaimTypes.NameIdentifier to match your token's claims.
            var traineeIdClaim = User.FindFirstValue("traineeId") ?? User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (int.TryParse(traineeIdClaim, out int traineeId))
            {
                return traineeId;
            }

            throw new UnauthorizedAccessException("Trainee ID claim is missing or invalid.");
        }

        #endregion
    }
}
