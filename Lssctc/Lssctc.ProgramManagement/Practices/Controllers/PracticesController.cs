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

        #region Error Response Helper
        
        /// <summary>
        /// Standard error response format for FE to easily catch and handle errors
        /// </summary>
        private IActionResult ErrorResponse(string errorCode, string errorMessage, int statusCode, object? errorDetails = null)
        {
            var response = new
            {
                success = false,
                error = new
                {
                    code = errorCode,
                    message = errorMessage,
                    details = errorDetails,
                    timestamp = DateTime.UtcNow
                }
            };
            return StatusCode(statusCode, response);
        }

        /// <summary>
        /// Success response format
        /// </summary>
        private IActionResult SuccessResponse<T>(T data, string? message = null)
        {
            return Ok(new
            {
                success = true,
                message,
                data
            });
        }

        #endregion

        #region Tasks CRUD

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var tasks = await _tasksService.GetAllPracticesAsync();
                return SuccessResponse(tasks);
            }
            catch (Exception ex)
            {
                return ErrorResponse(
                    "FETCH_PRACTICES_ERROR",
                    "Failed to fetch practices",
                    StatusCodes.Status500InternalServerError,
                    new { exceptionMessage = ex.Message }
                );
            }
        }

        [HttpGet("paged")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetPaged([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            try
            {
                // Validate pagination parameters
                if (pageNumber < 1)
                {
                    return ErrorResponse(
                        "INVALID_PAGE_NUMBER",
                        "Page number must be greater than 0",
                        StatusCodes.Status400BadRequest
                    );
                }

                if (pageSize < 1 || pageSize > 100)
                {
                    return ErrorResponse(
                        "INVALID_PAGE_SIZE",
                        "Page size must be between 1 and 100",
                        StatusCodes.Status400BadRequest
                    );
                }

                var result = await _tasksService.GetPracticesAsync(pageNumber, pageSize);
                return SuccessResponse(result);
            }
            catch (Exception ex)
            {
                return ErrorResponse(
                    "FETCH_PRACTICES_PAGED_ERROR",
                    "Failed to fetch paged practices",
                    StatusCodes.Status500InternalServerError,
                    new { exceptionMessage = ex.Message }
                );
            }
        }

        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                if (id <= 0)
                {
                    return ErrorResponse(
                        "INVALID_PRACTICE_ID",
                        "Practice ID must be a positive number",
                        StatusCodes.Status400BadRequest
                    );
                }

                var task = await _tasksService.GetPracticeByIdAsync(id);
                if (task == null)
                {
                    return ErrorResponse(
                        "PRACTICE_NOT_FOUND",
                        $"Practice with ID {id} not found",
                        StatusCodes.Status404NotFound
                    );
                }
                return SuccessResponse(task);
            }
            catch (Exception ex)
            {
                return ErrorResponse(
                    "GET_PRACTICE_ERROR",
                    "Failed to retrieve practice",
                    StatusCodes.Status500InternalServerError,
                    new { exceptionMessage = ex.Message }
                );
            }
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Create([FromBody] CreatePracticeDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var errors = ModelState
                        .Where(m => m.Value.Errors.Count > 0)
                        .Select(m => new { field = m.Key, messages = m.Value.Errors.Select(e => e.ErrorMessage) })
                        .ToList();

                    return ErrorResponse(
                        "VALIDATION_ERROR",
                        "Practice data validation failed",
                        StatusCodes.Status400BadRequest,
                        new { validationErrors = errors }
                    );
                }

                var created = await _tasksService.CreatePracticeAsync(dto);
                return CreatedAtAction(nameof(GetById), new { id = created.Id }, new
                {
                    success = true,
                    message = "Practice created successfully",
                    data = created
                });
            }
            catch (ArgumentException ex)
            {
                return ErrorResponse(
                    "INVALID_PRACTICE_DATA",
                    "Invalid practice data provided",
                    StatusCodes.Status400BadRequest,
                    new { exceptionMessage = ex.Message }
                );
            }
            catch (Exception ex)
            {
                return ErrorResponse(
                    "CREATE_PRACTICE_ERROR",
                    "Failed to create practice",
                    StatusCodes.Status500InternalServerError,
                    new { exceptionMessage = ex.Message }
                );
            }
        }

        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Update(int id, [FromBody] UpdatePracticeDto dto)
        {
            try
            {
                if (id <= 0)
                {
                    return ErrorResponse(
                        "INVALID_PRACTICE_ID",
                        "Practice ID must be a positive number",
                        StatusCodes.Status400BadRequest
                    );
                }

                if (!ModelState.IsValid)
                {
                    var errors = ModelState
                        .Where(m => m.Value.Errors.Count > 0)
                        .Select(m => new { field = m.Key, messages = m.Value.Errors.Select(e => e.ErrorMessage) })
                        .ToList();

                    return ErrorResponse(
                        "VALIDATION_ERROR",
                        "Practice data validation failed",
                        StatusCodes.Status400BadRequest,
                        new { validationErrors = errors }
                    );
                }

                var updated = await _tasksService.UpdatePracticeAsync(id, dto);
                return SuccessResponse(updated, "Practice updated successfully");
            }
            catch (KeyNotFoundException ex)
            {
                return ErrorResponse(
                    "PRACTICE_NOT_FOUND",
                    $"Practice with ID {id} not found",
                    StatusCodes.Status404NotFound
                );
            }
            catch (ArgumentException ex)
            {
                return ErrorResponse(
                    "INVALID_PRACTICE_DATA",
                    "Invalid practice data provided",
                    StatusCodes.Status400BadRequest,
                    new { exceptionMessage = ex.Message }
                );
            }
            catch (Exception ex)
            {
                return ErrorResponse(
                    "UPDATE_PRACTICE_ERROR",
                    "Failed to update practice",
                    StatusCodes.Status500InternalServerError,
                    new { exceptionMessage = ex.Message }
                );
            }
        }

        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                if (id <= 0)
                {
                    return ErrorResponse(
                        "INVALID_PRACTICE_ID",
                        "Practice ID must be a positive number",
                        StatusCodes.Status400BadRequest
                    );
                }

                await _tasksService.DeletePracticeAsync(id);
                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                return ErrorResponse(
                    "PRACTICE_NOT_FOUND",
                    $"Practice with ID {id} not found",
                    StatusCodes.Status404NotFound
                );
            }
            catch (InvalidOperationException ex)
            {
                return ErrorResponse(
                    "PRACTICE_DELETE_CONFLICT",
                    "Cannot delete practice: " + ex.Message,
                    StatusCodes.Status400BadRequest
                );
            }
            catch (Exception ex)
            {
                return ErrorResponse(
                    "DELETE_PRACTICE_ERROR",
                    "Failed to delete practice",
                    StatusCodes.Status500InternalServerError,
                    new { exceptionMessage = ex.Message }
                );
            }
        }

        #endregion

        #region Practice Tasks

        [HttpGet("practice/{practiceId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetTasksByPractice(int practiceId)
        {
            try
            {
                if (practiceId <= 0)
                {
                    return ErrorResponse(
                        "INVALID_PRACTICE_ID",
                        "Practice ID must be a positive number",
                        StatusCodes.Status400BadRequest
                    );
                }

                var tasks = await _tasksService.GetPracticesByActivityAsync(practiceId);
                return SuccessResponse(tasks);
            }
            catch (KeyNotFoundException ex)
            {
                return ErrorResponse(
                    "PRACTICE_NOT_FOUND",
                    $"Practice with ID {practiceId} not found",
                    StatusCodes.Status404NotFound
                );
            }
            catch (Exception ex)
            {
                return ErrorResponse(
                    "FETCH_PRACTICE_TASKS_ERROR",
                    "Failed to fetch practice tasks",
                    StatusCodes.Status500InternalServerError,
                    new { exceptionMessage = ex.Message }
                );
            }
        }

        [HttpPost("practice/{practiceId}/add/{taskId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> AddTaskToPractice(int practiceId, int taskId)
        {
            try
            {
                if (practiceId <= 0)
                {
                    return ErrorResponse(
                        "INVALID_PRACTICE_ID",
                        "Practice ID must be a positive number",
                        StatusCodes.Status400BadRequest
                    );
                }

                if (taskId <= 0)
                {
                    return ErrorResponse(
                        "INVALID_TASK_ID",
                        "Task ID must be a positive number",
                        StatusCodes.Status400BadRequest
                    );
                }

                await _tasksService.AddPracticeToActivityAsync(practiceId, taskId);
                return SuccessResponse(new { practiceId, taskId }, "Task added to practice successfully");
            }
            catch (KeyNotFoundException ex)
            {
                return ErrorResponse(
                    "RESOURCE_NOT_FOUND",
                    "Practice or Task not found: " + ex.Message,
                    StatusCodes.Status404NotFound
                );
            }
            catch (InvalidOperationException ex)
            {
                return ErrorResponse(
                    "INVALID_OPERATION",
                    "Cannot add task to practice: " + ex.Message,
                    StatusCodes.Status400BadRequest
                );
            }
            catch (Exception ex)
            {
                return ErrorResponse(
                    "ADD_TASK_TO_PRACTICE_ERROR",
                    "Failed to add task to practice",
                    StatusCodes.Status500InternalServerError,
                    new { exceptionMessage = ex.Message }
                );
            }
        }

        [HttpDelete("practice/{practiceId}/remove/{taskId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> RemoveTaskFromPractice(int practiceId, int taskId)
        {
            try
            {
                if (practiceId <= 0)
                {
                    return ErrorResponse(
                        "INVALID_PRACTICE_ID",
                        "Practice ID must be a positive number",
                        StatusCodes.Status400BadRequest
                    );
                }

                if (taskId <= 0)
                {
                    return ErrorResponse(
                        "INVALID_TASK_ID",
                        "Task ID must be a positive number",
                        StatusCodes.Status400BadRequest
                    );
                }

                await _tasksService.RemovePracticeFromActivityAsync(practiceId, taskId);
                return SuccessResponse(new { practiceId, taskId }, "Task removed from practice successfully");
            }
            catch (KeyNotFoundException ex)
            {
                return ErrorResponse(
                    "RESOURCE_NOT_FOUND",
                    "Practice or Task not found: " + ex.Message,
                    StatusCodes.Status404NotFound
                );
            }
            catch (InvalidOperationException ex)
            {
                return ErrorResponse(
                    "INVALID_OPERATION",
                    "Cannot remove task from practice: " + ex.Message,
                    StatusCodes.Status400BadRequest
                );
            }
            catch (Exception ex)
            {
                return ErrorResponse(
                    "REMOVE_TASK_FROM_PRACTICE_ERROR",
                    "Failed to remove task from practice",
                    StatusCodes.Status500InternalServerError,
                    new { exceptionMessage = ex.Message }
                );
            }
        }

        #endregion

        #region Trainee Practices

        [HttpGet("trainee/class/{classId}")]
        [Authorize(Roles = "Trainee")]
        [ProducesResponseType(typeof(IEnumerable<TraineePracticeDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetPracticesForTrainee(int classId)
        {
            try
            {
                if (classId <= 0)
                {
                    return ErrorResponse(
                        "INVALID_CLASS_ID",
                        "Class ID must be a positive number",
                        StatusCodes.Status400BadRequest
                    );
                }

                var traineeId = GetTraineeIdFromClaims();
                var result = await _tasksService.GetPracticesForTraineeAsync(traineeId, classId);
                return SuccessResponse(result);
            }
            catch (UnauthorizedAccessException ex)
            {
                return ErrorResponse(
                    "UNAUTHORIZED_ACCESS",
                    ex.Message,
                    StatusCodes.Status401Unauthorized
                );
            }
            catch (KeyNotFoundException ex)
            {
                return ErrorResponse(
                    "RESOURCE_NOT_FOUND",
                    "Class or Trainee not found: " + ex.Message,
                    StatusCodes.Status404NotFound
                );
            }
            catch (Exception ex)
            {
                return ErrorResponse(
                    "FETCH_TRAINEE_PRACTICES_ERROR",
                    "Failed to fetch practices for trainee",
                    StatusCodes.Status500InternalServerError,
                    new { exceptionMessage = ex.Message }
                );
            }
        }

        [HttpGet("trainee/activity-record/{activityRecordId}")]
        [Authorize(Roles = "Trainee")]
        [ProducesResponseType(typeof(TraineePracticeDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetPracticeForTraineeByActivityId(int activityRecordId)
        {
            try
            {
                if (activityRecordId <= 0)
                {
                    return ErrorResponse(
                        "INVALID_ACTIVITY_RECORD_ID",
                        "Activity Record ID must be a positive number",
                        StatusCodes.Status400BadRequest
                    );
                }

                var traineeId = GetTraineeIdFromClaims();
                var result = await _tasksService.GetPracticeForTraineeByActivityIdAsync(traineeId, activityRecordId);

                if (result == null)
                {
                    return ErrorResponse(
                        "PRACTICE_NOT_FOUND",
                        $"Practice for activity record {activityRecordId} not found",
                        StatusCodes.Status404NotFound
                    );
                }

                return SuccessResponse(result);
            }
            catch (UnauthorizedAccessException ex)
            {
                return ErrorResponse(
                    "UNAUTHORIZED_ACCESS",
                    ex.Message,
                    StatusCodes.Status401Unauthorized
                );
            }
            catch (KeyNotFoundException ex)
            {
                return ErrorResponse(
                    "RESOURCE_NOT_FOUND",
                    "Activity record or Trainee not found: " + ex.Message,
                    StatusCodes.Status404NotFound
                );
            }
            catch (Exception ex)
            {
                return ErrorResponse(
                    "FETCH_TRAINEE_PRACTICE_ERROR",
                    "Failed to fetch practice for trainee",
                    StatusCodes.Status500InternalServerError,
                    new { exceptionMessage = ex.Message }
                );
            }
        }

        #endregion

        #region Activity Practices

        [HttpGet("activity/{activityId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetPracticesByActivity(int activityId)
        {
            try
            {
                if (activityId <= 0)
                {
                    return ErrorResponse(
                        "INVALID_ACTIVITY_ID",
                        "Activity ID must be a positive number",
                        StatusCodes.Status400BadRequest
                    );
                }

                var practices = await _tasksService.GetPracticesByActivityAsync(activityId);
                return SuccessResponse(practices);
            }
            catch (Exception ex)
            {
                return ErrorResponse(
                    "FETCH_ACTIVITY_PRACTICES_ERROR",
                    "Failed to fetch practices for activity",
                    StatusCodes.Status500InternalServerError,
                    new { exceptionMessage = ex.Message }
                );
            }
        }

        [HttpPost("activity/{activityId}/add/{practiceId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> AddPracticeToActivity(int activityId, int practiceId)
        {
            try
            {
                if (activityId <= 0 || practiceId <= 0)
                {
                    return ErrorResponse(
                        "INVALID_ID",
                        "Activity ID and Practice ID must be positive numbers",
                        StatusCodes.Status400BadRequest
                    );
                }

                await _tasksService.AddPracticeToActivityAsync(activityId, practiceId);
                return SuccessResponse(new { activityId, practiceId }, "Practice assigned to activity successfully");
            }
            catch (KeyNotFoundException ex)
            {
                return ErrorResponse(
                    "RESOURCE_NOT_FOUND",
                    ex.Message,
                    StatusCodes.Status404NotFound
                );
            }
            catch (InvalidOperationException ex)
            {
                return ErrorResponse(
                    "INVALID_OPERATION",
                    ex.Message,
                    StatusCodes.Status400BadRequest
                );
            }
            catch (Exception ex)
            {
                return ErrorResponse(
                    "ASSIGN_PRACTICE_ERROR",
                    "Failed to assign practice to activity",
                    StatusCodes.Status500InternalServerError,
                    new { exceptionMessage = ex.Message }
                );
            }
        }

        [HttpDelete("activity/{activityId}/remove/{practiceId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> RemovePracticeFromActivity(int activityId, int practiceId)
        {
            try
            {
                if (activityId <= 0 || practiceId <= 0)
                {
                    return ErrorResponse(
                        "INVALID_ID",
                        "Activity ID and Practice ID must be positive numbers",
                        StatusCodes.Status400BadRequest
                    );
                }

                await _tasksService.RemovePracticeFromActivityAsync(activityId, practiceId);
                return SuccessResponse(new { activityId, practiceId }, "Practice removed from activity successfully");
            }
            catch (KeyNotFoundException ex)
            {
                return ErrorResponse(
                    "RESOURCE_NOT_FOUND",
                    ex.Message,
                    StatusCodes.Status404NotFound
                );
            }
            catch (Exception ex)
            {
                return ErrorResponse(
                    "REMOVE_PRACTICE_ERROR",
                    "Failed to remove practice from activity",
                    StatusCodes.Status500InternalServerError,
                    new { exceptionMessage = ex.Message }
                );
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