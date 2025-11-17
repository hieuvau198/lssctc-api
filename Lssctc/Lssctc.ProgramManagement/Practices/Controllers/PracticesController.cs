using Lssctc.ProgramManagement.Practices.Dtos;
using Lssctc.ProgramManagement.Practices.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Lssctc.ProgramManagement.Practices.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PracticesController : ControllerBase
    {
        private readonly IPracticesService _tasksService;

        public PracticesController(IPracticesService tasksService)
        {
            _tasksService = tasksService;
        }

        #region Tasks CRUD

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var tasks = await _tasksService.GetAllPracticesAsync();
                return Ok(tasks);
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
        }

        [HttpGet("paged")]
        public async Task<IActionResult> GetPaged([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            try
            {
                var result = await _tasksService.GetPracticesAsync(pageNumber, pageSize);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                var task = await _tasksService.GetPracticeByIdAsync(id);
                if (task == null) return NotFound(new { Message = "Practice not found." });
                return Ok(task);
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreatePracticeDto dto)
        {
            try
            {
                if (!ModelState.IsValid) return BadRequest(ModelState);
                var created = await _tasksService.CreatePracticeAsync(dto);
                return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdatePracticeDto dto)
        {
            try
            {
                if (!ModelState.IsValid) return BadRequest(ModelState);
                var updated = await _tasksService.UpdatePracticeAsync(id, dto);
                return Ok(updated);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                await _tasksService.DeletePracticeAsync(id);
                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { Message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
        }

        #endregion

        #region Practice Tasks

        [HttpGet("practice/{practiceId}")]
        public async Task<IActionResult> GetTasksByPractice(int practiceId)
        {
            try
            {
                var tasks = await _tasksService.GetPracticesByActivityAsync(practiceId);
                return Ok(tasks);
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
        }

        [HttpPost("practice/{practiceId}/add/{taskId}")]
        public async Task<IActionResult> AddTaskToPractice(int practiceId, int taskId)
        {
            try
            {
                await _tasksService.AddPracticeToActivityAsync(practiceId, taskId);
                return Ok(new { Message = "Task added to practice successfully." });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { Message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
        }

        [HttpDelete("practice/{practiceId}/remove/{taskId}")]
        public async Task<IActionResult> RemoveTaskFromPractice(int practiceId, int taskId)
        {
            try
            {
                await _tasksService.RemovePracticeFromActivityAsync(practiceId, taskId);
                return Ok(new { Message = "Task removed from practice successfully." });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { Message = ex.Message });
            }
            catch (InvalidOperationException ex) // Added for lock check
            {
                return BadRequest(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
        }

        #endregion

        // ... (Trainee Practices region remains unchanged as it already used new { message = ... }) ...
        #region Trainee Practices
        [HttpGet("trainee/class/{classId}")]
        [Authorize(Roles = "Trainee")] // Trainee can only see their own
        [ProducesResponseType(typeof(IEnumerable<TraineePracticeDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetPracticesForTrainee(int classId)
        {
            try
            {
                var traineeId = GetTraineeIdFromClaims();
                var result = await _tasksService.GetPracticesForTraineeAsync(traineeId, classId);
                return Ok(result);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [HttpGet("trainee/activity-record/{activityRecordId}")]
        [Authorize(Roles = "Trainee")]
        [ProducesResponseType(typeof(TraineePracticeDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetPracticeForTraineeByActivityId(int activityRecordId)
        {
            try
            {
                var traineeId = GetTraineeIdFromClaims();
                var result = await _tasksService.GetPracticeForTraineeByActivityIdAsync(traineeId, activityRecordId);

                // The service will throw KeyNotFoundException if not found
                return Ok(result);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                // Log the exception in a real application
                return StatusCode(500, new { message = ex.Message });
            }
        }

        #endregion

        #region Helpers

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