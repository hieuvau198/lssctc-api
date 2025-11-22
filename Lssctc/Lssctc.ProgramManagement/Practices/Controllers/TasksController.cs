using Lssctc.ProgramManagement.Practices.Dtos;
using Lssctc.ProgramManagement.Practices.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Lssctc.ProgramManagement.Practices.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TasksController : ControllerBase
    {
        private readonly ITasksService _tasksService;

        public TasksController(ITasksService tasksService)
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
                var tasks = await _tasksService.GetAllTasksAsync();
                return SuccessResponse(tasks);
            }
            catch (Exception ex)
            {
                return ErrorResponse(
                    "FETCH_TASKS_ERROR",
                    "Failed to fetch tasks",
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

                var result = await _tasksService.GetTasksAsync(pageNumber, pageSize);
                return SuccessResponse(result);
            }
            catch (Exception ex)
            {
                return ErrorResponse(
                    "FETCH_TASKS_PAGED_ERROR",
                    "Failed to fetch paged tasks",
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
                        "INVALID_TASK_ID",
                        "Task ID must be a positive number",
                        StatusCodes.Status400BadRequest
                    );
                }

                var task = await _tasksService.GetTaskByIdAsync(id);
                if (task == null)
                {
                    return ErrorResponse(
                        "TASK_NOT_FOUND",
                        $"Task with ID {id} not found",
                        StatusCodes.Status404NotFound
                    );
                }
                return SuccessResponse(task);
            }
            catch (Exception ex)
            {
                return ErrorResponse(
                    "GET_TASK_ERROR",
                    "Failed to retrieve task",
                    StatusCodes.Status500InternalServerError,
                    new { exceptionMessage = ex.Message }
                );
            }
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Create([FromBody] CreateTaskDto dto)
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
                        "Task data validation failed",
                        StatusCodes.Status400BadRequest,
                        new { validationErrors = errors }
                    );
                }

                var created = await _tasksService.CreateTaskAsync(dto);
                return CreatedAtAction(nameof(GetById), new { id = created.Id }, new
                {
                    success = true,
                    message = "Task created successfully",
                    data = created
                });
            }
            catch (ArgumentException ex)
            {
                return ErrorResponse(
                    "INVALID_TASK_DATA",
                    "Invalid task data provided",
                    StatusCodes.Status400BadRequest,
                    new { exceptionMessage = ex.Message }
                );
            }
            catch (Exception ex)
            {
                return ErrorResponse(
                    "CREATE_TASK_ERROR",
                    "Failed to create task",
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
        public async Task<IActionResult> Update(int id, [FromBody] UpdateTaskDto dto)
        {
            try
            {
                if (id <= 0)
                {
                    return ErrorResponse(
                        "INVALID_TASK_ID",
                        "Task ID must be a positive number",
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
                        "Task data validation failed",
                        StatusCodes.Status400BadRequest,
                        new { validationErrors = errors }
                    );
                }

                var updated = await _tasksService.UpdateTaskAsync(id, dto);
                return SuccessResponse(updated, "Task updated successfully");
            }
            catch (KeyNotFoundException ex)
            {
                return ErrorResponse(
                    "TASK_NOT_FOUND",
                    $"Task with ID {id} not found",
                    StatusCodes.Status404NotFound
                );
            }
            catch (ArgumentException ex)
            {
                return ErrorResponse(
                    "INVALID_TASK_DATA",
                    "Invalid task data provided",
                    StatusCodes.Status400BadRequest,
                    new { exceptionMessage = ex.Message }
                );
            }
            catch (Exception ex)
            {
                return ErrorResponse(
                    "UPDATE_TASK_ERROR",
                    "Failed to update task",
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
                        "INVALID_TASK_ID",
                        "Task ID must be a positive number",
                        StatusCodes.Status400BadRequest
                    );
                }

                await _tasksService.DeleteTaskAsync(id);
                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                return ErrorResponse(
                    "TASK_NOT_FOUND",
                    $"Task with ID {id} not found",
                    StatusCodes.Status404NotFound
                );
            }
            catch (InvalidOperationException ex)
            {
                return ErrorResponse(
                    "TASK_DELETE_CONFLICT",
                    "Cannot delete task: " + ex.Message,
                    StatusCodes.Status400BadRequest
                );
            }
            catch (Exception ex)
            {
                return ErrorResponse(
                    "DELETE_TASK_ERROR",
                    "Failed to delete task",
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

                var tasks = await _tasksService.GetTasksByPracticeAsync(practiceId);
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

        [HttpPost("practice/{practiceCode}")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> CreateTaskByPracticeCode(string practiceCode, [FromBody] CreateTaskDto dto)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(practiceCode))
                {
                    return ErrorResponse(
                        "INVALID_PRACTICE_CODE",
                        "Practice code is required",
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
                        "Task data validation failed",
                        StatusCodes.Status400BadRequest,
                        new { validationErrors = errors }
                    );
                }

                var created = await _tasksService.CreateTaskByPracticeAsync(practiceCode, dto);
                return CreatedAtAction(nameof(GetById), new { id = created.Id }, new
                {
                    success = true,
                    message = "Task created and added to practice successfully",
                    data = created
                });
            }
            catch (KeyNotFoundException ex)
            {
                return ErrorResponse(
                    "PRACTICE_NOT_FOUND",
                    "Practice not found: " + ex.Message,
                    StatusCodes.Status404NotFound
                );
            }
            catch (ArgumentException ex)
            {
                return ErrorResponse(
                    "INVALID_TASK_DATA",
                    "Invalid task data provided",
                    StatusCodes.Status400BadRequest,
                    new { exceptionMessage = ex.Message }
                );
            }
            catch (Exception ex)
            {
                return ErrorResponse(
                    "CREATE_TASK_BY_PRACTICE_ERROR",
                    "Failed to create task for practice",
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

                await _tasksService.AddTaskToPracticeAsync(practiceId, taskId);
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

                await _tasksService.RemoveTaskFromPracticeAsync(practiceId, taskId);
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
    }
}
