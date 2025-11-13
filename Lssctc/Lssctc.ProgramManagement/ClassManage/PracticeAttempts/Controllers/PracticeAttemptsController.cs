using Lssctc.ProgramManagement.ClassManage.PracticeAttempts.Services;
using Lssctc.ProgramManagement.ClassManage.PracticeAttempts.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Lssctc.ProgramManagement.ClassManage.PracticeAttempts.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class PracticeAttemptsController : ControllerBase
    {
        private readonly IPracticeAttemptsService _practiceAttemptsService;

        public PracticeAttemptsController(IPracticeAttemptsService practiceAttemptsService)
        {
            _practiceAttemptsService = practiceAttemptsService;
        }

        #region Trainee Endpoints

        /// <summary>
        /// Get all practice attempts for a trainee's activity record
        /// </summary>
        /// <param name="traineeId">Trainee ID</param>
        /// <param name="activityRecordId">The activity record ID</param>
        /// <returns>List of all practice attempts ordered by attempt date (newest first)</returns>
        [HttpGet("attempts")]
        [Authorize(Roles = "Trainee, Admin")]
        public async Task<IActionResult> GetPracticeAttempts([FromQuery] int traineeId, [FromQuery] int activityRecordId)
        {
            try
            {
                var result = await _practiceAttemptsService.GetPracticeAttempts(traineeId, activityRecordId);
                return Ok(result);
            }
            catch (UnauthorizedAccessException ex) { return Unauthorized(new { message = ex.Message }); }
            catch (KeyNotFoundException ex) { return NotFound(new { message = ex.Message }); }
            catch (Exception ex) 
            { 
                Console.WriteLine($"Error: {ex.Message}");
                Console.WriteLine($"StackTrace: {ex.StackTrace}");
                return StatusCode(500, new { message = "An unexpected error occurred.", details = ex.Message }); 
            }
        }

        /// <summary>
        /// Get practice attempts with pagination for a trainee's activity record
        /// </summary>
        /// <param name="traineeId">Trainee ID</param>
        /// <param name="activityRecordId">The activity record ID</param>
        /// <param name="pageNumber">Page number (default: 1)</param>
        /// <param name="pageSize">Page size (default: 10)</param>
        /// <returns>Paged list of practice attempts</returns>
        [HttpGet("attempts/paged")]
        [Authorize(Roles = "Trainee, Admin")]
        public async Task<IActionResult> GetPracticeAttemptsPaged(
            [FromQuery] int traineeId, 
            [FromQuery] int activityRecordId, 
            [FromQuery] int pageNumber = 1, 
            [FromQuery] int pageSize = 10)
        {
            try
            {
                var result = await _practiceAttemptsService.GetPracticeAttemptsPaged(traineeId, activityRecordId, pageNumber, pageSize);
                return Ok(result);
            }
            catch (UnauthorizedAccessException ex) { return Unauthorized(new { message = ex.Message }); }
            catch (KeyNotFoundException ex) { return NotFound(new { message = ex.Message }); }
            catch (Exception ex) 
            { 
                Console.WriteLine($"Error: {ex.Message}");
                Console.WriteLine($"StackTrace: {ex.StackTrace}");
                return StatusCode(500, new { message = "An unexpected error occurred.", details = ex.Message }); 
            }
        }

        /// <summary>
        /// Get the latest (current) practice attempt for a trainee's activity record
        /// </summary>
        /// <param name="traineeId">Trainee ID</param>
        /// <param name="activityRecordId">The activity record ID</param>
        /// <returns>The current practice attempt or 404 if none exists</returns>
        [HttpGet("attempts/latest")]
        [Authorize(Roles = "Trainee, Admin")]
        public async Task<IActionResult> GetLatestPracticeAttempt([FromQuery] int traineeId, [FromQuery] int activityRecordId)
        {
            try
            {
                var result = await _practiceAttemptsService.GetLatestPracticeAttempt(traineeId, activityRecordId);
                if (result == null)
                    return NotFound(new { message = "No practice attempt found." });
                return Ok(result);
            }
            catch (UnauthorizedAccessException ex) { return Unauthorized(new { message = ex.Message }); }
            catch (KeyNotFoundException ex) { return NotFound(new { message = ex.Message }); }
            catch (Exception ex) 
            { 
                Console.WriteLine($"Error: {ex.Message}");
                Console.WriteLine($"StackTrace: {ex.StackTrace}");
                return StatusCode(500, new { message = "An unexpected error occurred.", details = ex.Message }); 
            }
        }

        /// <summary>
        /// Get all practice attempts for a trainee by practice ID
        /// </summary>
        /// <param name="traineeId">Trainee ID</param>
        /// <param name="practiceId">Practice ID</param>
        /// <returns>List of all practice attempts for the specific practice</returns>
        [HttpGet("by-practice")]
        [Authorize(Roles = "Trainee, Admin")]
        public async Task<IActionResult> GetPracticeAttemptsByPractice([FromQuery] int traineeId, [FromQuery] int practiceId)
        {
            try
            {
                var result = await _practiceAttemptsService.GetPracticeAttemptsByPractice(traineeId, practiceId);
                return Ok(result);
            }
            catch (KeyNotFoundException ex) { return NotFound(new { message = ex.Message }); }
            catch (Exception ex) 
            { 
                Console.WriteLine($"Error: {ex.Message}");
                Console.WriteLine($"StackTrace: {ex.StackTrace}");
                return StatusCode(500, new { message = "An unexpected error occurred.", details = ex.Message }); 
            }
        }

        /// <summary>
        /// Get practice attempts with pagination for a trainee by practice ID
        /// </summary>
        /// <param name="traineeId">Trainee ID</param>
        /// <param name="practiceId">Practice ID</param>
        /// <param name="pageNumber">Page number (default: 1)</param>
        /// <param name="pageSize">Page size (default: 10)</param>
        /// <returns>Paged list of practice attempts</returns>
        [HttpGet("by-practice/paged")]
        [Authorize(Roles = "Trainee, Admin")]
        public async Task<IActionResult> GetPracticeAttemptsByPracticePaged(
            [FromQuery] int traineeId, 
            [FromQuery] int practiceId, 
            [FromQuery] int pageNumber = 1, 
            [FromQuery] int pageSize = 10)
        {
            try
            {
                var result = await _practiceAttemptsService.GetPracticeAttemptsByPracticePaged(traineeId, practiceId, pageNumber, pageSize);
                return Ok(result);
            }
            catch (KeyNotFoundException ex) { return NotFound(new { message = ex.Message }); }
            catch (Exception ex) 
            { 
                Console.WriteLine($"Error: {ex.Message}");
                Console.WriteLine($"StackTrace: {ex.StackTrace}");
                return StatusCode(500, new { message = "An unexpected error occurred.", details = ex.Message }); 
            }
        }

        /// <summary>
        /// Get a specific practice attempt by ID
        /// </summary>
        /// <param name="id">Practice attempt ID</param>
        /// <returns>Practice attempt details</returns>
        [HttpGet("{id}")]
        [Authorize(Roles = "Trainee, Admin, Instructor")]
        public async Task<IActionResult> GetPracticeAttemptById(int id)
        {
            try
            {
                var result = await _practiceAttemptsService.GetPracticeAttemptById(id);
                if (result == null)
                    return NotFound(new { message = "Practice attempt not found." });
                return Ok(result);
            }
            catch (Exception ex) 
            { 
                Console.WriteLine($"Error: {ex.Message}");
                Console.WriteLine($"StackTrace: {ex.StackTrace}");
                return StatusCode(500, new { message = "An unexpected error occurred.", details = ex.Message }); 
            }
        }

        /// <summary>
        /// Create a new practice attempt with tasks
        /// </summary>
        /// <param name="createDto">Practice attempt data with trainee ID, class ID, practice ID, score, and tasks</param>
        /// <returns>Created practice attempt with ID</returns>
        [HttpPost]
        [Authorize(Roles = "Trainee, Admin")]
        [ProducesResponseType(typeof(PracticeAttemptDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> CreatePracticeAttempt([FromBody] CreatePracticeAttemptDto createDto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                // --- ADDED/MODIFIED LINES ---
                var traineeId = GetTraineeIdFromClaims(); // Get ID from token
                var result = await _practiceAttemptsService.CreatePracticeAttempt(traineeId, createDto); // Pass ID to service
                                                                                                         // --- END OF CHANGES ---

                return CreatedAtAction(nameof(GetPracticeAttemptById), new { id = result.Id }, result);
            }
            catch (KeyNotFoundException ex) { return NotFound(new { message = ex.Message }); }
            catch (ArgumentException ex) { return BadRequest(new { message = ex.Message }); }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                Console.WriteLine($"StackTrace: {ex.StackTrace}");
                return StatusCode(500, new { message = "An unexpected error occurred.", details = ex.Message });
            }
        }

        #endregion

        // --- ADDED HELPER METHOD (if not already present from previous request) ---
        private int GetTraineeIdFromClaims()
        {
            var traineeIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (int.TryParse(traineeIdClaim, out int traineeId))
            {
                return traineeId;
            }
            throw new UnauthorizedAccessException("Trainee ID claim is missing or invalid.");
        }
    }
}
