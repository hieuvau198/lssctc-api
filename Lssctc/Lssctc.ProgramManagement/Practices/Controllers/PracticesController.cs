using Lssctc.ProgramManagement.Practices.Dtos;
using Lssctc.ProgramManagement.Practices.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Lssctc.ProgramManagement.Practices.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PracticesController : ControllerBase
    {
        private readonly IPracticesService _practicesService;
        private readonly ITraineePracticesService _traineePracticesService;

        public PracticesController(IPracticesService practicesService, ITraineePracticesService traineePracticesService)
        {
            _practicesService = practicesService;
            _traineePracticesService = traineePracticesService;
        }

        private IActionResult ErrorResponse(string errorCode, string errorMessage, int statusCode, object? errorDetails = null)
        {
            return StatusCode(statusCode, new
            {
                success = false,
                error = new { code = errorCode, message = errorMessage, details = errorDetails, timestamp = DateTime.UtcNow }
            });
        }

        private IActionResult SuccessResponse<T>(T data, string? message = null)
        {
            return Ok(new { success = true, message, data });
        }

        #region Practices CRUD

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var tasks = await _practicesService.GetAllPracticesAsync();
                return SuccessResponse(tasks);
            }
            catch (Exception ex)
            {
                return ErrorResponse("FETCH_PRACTICES_ERROR", "Failed to fetch practices", 500, new { exceptionMessage = ex.Message });
            }
        }

        [HttpGet("paged")]
        public async Task<IActionResult> GetPaged([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            try
            {
                if (pageNumber < 1) return ErrorResponse("INVALID_PAGE_NUMBER", "Page number must be > 0", 400);
                if (pageSize < 1 || pageSize > 100) return ErrorResponse("INVALID_PAGE_SIZE", "Page size 1-100", 400);

                var result = await _practicesService.GetPracticesAsync(pageNumber, pageSize);
                return SuccessResponse(result);
            }
            catch (Exception ex)
            {
                return ErrorResponse("FETCH_PAGED_ERROR", "Failed to fetch paged practices", 500, new { exceptionMessage = ex.Message });
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                if (id <= 0) return ErrorResponse("INVALID_ID", "ID must be positive", 400);
                var task = await _practicesService.GetPracticeByIdAsync(id);
                if (task == null) return ErrorResponse("NOT_FOUND", $"Practice {id} not found", 404);
                return SuccessResponse(task);
            }
            catch (Exception ex)
            {
                return ErrorResponse("GET_ERROR", "Failed to retrieve practice", 500, new { exceptionMessage = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreatePracticeDto dto)
        {
            try
            {
                if (!ModelState.IsValid) return BadRequest(ModelState);
                var created = await _practicesService.CreatePracticeAsync(dto);
                return CreatedAtAction(nameof(GetById), new { id = created.Id }, new { success = true, data = created });
            }
            catch (ArgumentException ex)
            {
                return ErrorResponse("INVALID_DATA", ex.Message, 400);
            }
            catch (Exception ex)
            {
                return ErrorResponse("CREATE_ERROR", "Failed to create practice", 500, new { exceptionMessage = ex.Message });
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdatePracticeDto dto)
        {
            try
            {
                if (id <= 0) return ErrorResponse("INVALID_ID", "ID must be positive", 400);
                if (!ModelState.IsValid) return BadRequest(ModelState);

                var updated = await _practicesService.UpdatePracticeAsync(id, dto);
                return SuccessResponse(updated, "Practice updated successfully");
            }
            catch (KeyNotFoundException ex) { return ErrorResponse("NOT_FOUND", ex.Message, 404); }
            catch (ArgumentException ex) { return ErrorResponse("INVALID_DATA", ex.Message, 400); }
            catch (Exception ex) { return ErrorResponse("UPDATE_ERROR", "Failed to update", 500, new { exceptionMessage = ex.Message }); }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                if (id <= 0) return ErrorResponse("INVALID_ID", "ID must be positive", 400);
                await _practicesService.DeletePracticeAsync(id);
                return NoContent();
            }
            catch (KeyNotFoundException ex) { return ErrorResponse("NOT_FOUND", ex.Message, 404); }
            catch (InvalidOperationException ex) { return ErrorResponse("CONFLICT", ex.Message, 400); }
            catch (Exception ex) { return ErrorResponse("DELETE_ERROR", "Failed to delete", 500, new { exceptionMessage = ex.Message }); }
        }

        #endregion

        #region Activity Practices & Practice Tasks

        [HttpGet("practice/{practiceId}")]
        public async Task<IActionResult> GetTasksByPractice(int practiceId)
        {
            try
            {
                if (practiceId <= 0) return ErrorResponse("INVALID_ID", "ID positive", 400);
                var tasks = await _practicesService.GetPracticesByActivityAsync(practiceId);
                return SuccessResponse(tasks);
            }
            catch (Exception ex) { return ErrorResponse("FETCH_ERROR", "Failed to fetch", 500, new { ex.Message }); }
        }

        [HttpPost("practice/{practiceId}/add/{taskId}")]
        public async Task<IActionResult> AddTaskToPractice(int practiceId, int taskId)
        {
            try
            {
                if (practiceId <= 0 || taskId <= 0) return ErrorResponse("INVALID_ID", "IDs positive", 400);
                await _practicesService.AddPracticeToActivityAsync(practiceId, taskId);
                return SuccessResponse(new { practiceId, taskId }, "Added successfully");
            }
            catch (KeyNotFoundException ex) { return ErrorResponse("NOT_FOUND", ex.Message, 404); }
            catch (InvalidOperationException ex) { return ErrorResponse("INVALID_OP", ex.Message, 400); }
            catch (Exception ex) { return ErrorResponse("ADD_ERROR", "Failed to add", 500, new { ex.Message }); }
        }

        [HttpDelete("practice/{practiceId}/remove/{taskId}")]
        public async Task<IActionResult> RemoveTaskFromPractice(int practiceId, int taskId)
        {
            try
            {
                if (practiceId <= 0 || taskId <= 0) return ErrorResponse("INVALID_ID", "IDs positive", 400);
                await _practicesService.RemovePracticeFromActivityAsync(practiceId, taskId);
                return SuccessResponse(new { practiceId, taskId }, "Removed successfully");
            }
            catch (KeyNotFoundException ex) { return ErrorResponse("NOT_FOUND", ex.Message, 404); }
            catch (Exception ex) { return ErrorResponse("REMOVE_ERROR", "Failed to remove", 500, new { ex.Message }); }
        }

        [HttpGet("activity/{activityId}")]
        public async Task<IActionResult> GetPracticesByActivity(int activityId)
        {
            try
            {
                if (activityId <= 0) return ErrorResponse("INVALID_ID", "ID positive", 400);
                var practices = await _practicesService.GetPracticesByActivityAsync(activityId);
                return SuccessResponse(practices);
            }
            catch (Exception ex) { return ErrorResponse("FETCH_ERROR", "Failed to fetch", 500, new { ex.Message }); }
        }

        [HttpPost("activity/{activityId}/add/{practiceId}")]
        public async Task<IActionResult> AddPracticeToActivity(int activityId, int practiceId)
        {
            try
            {
                if (activityId <= 0 || practiceId <= 0) return ErrorResponse("INVALID_ID", "IDs positive", 400);
                await _practicesService.AddPracticeToActivityAsync(activityId, practiceId);
                return SuccessResponse(new { activityId, practiceId }, "Assigned successfully");
            }
            catch (KeyNotFoundException ex) { return ErrorResponse("NOT_FOUND", ex.Message, 404); }
            catch (InvalidOperationException ex) { return ErrorResponse("INVALID_OP", ex.Message, 400); }
            catch (Exception ex) { return ErrorResponse("ASSIGN_ERROR", "Failed to assign", 500, new { ex.Message }); }
        }

        [HttpDelete("activity/{activityId}/remove/{practiceId}")]
        public async Task<IActionResult> RemovePracticeFromActivity(int activityId, int practiceId)
        {
            try
            {
                if (activityId <= 0 || practiceId <= 0) return ErrorResponse("INVALID_ID", "IDs positive", 400);
                await _practicesService.RemovePracticeFromActivityAsync(activityId, practiceId);
                return SuccessResponse(new { activityId, practiceId }, "Removed successfully");
            }
            catch (KeyNotFoundException ex) { return ErrorResponse("NOT_FOUND", ex.Message, 404); }
            catch (Exception ex) { return ErrorResponse("REMOVE_ERROR", "Failed to remove", 500, new { ex.Message }); }
        }

        #endregion

        #region Trainee Practices

        [HttpGet("trainee/class/{classId}")]
        [Authorize(Roles = "Trainee")]
        public async Task<IActionResult> GetPracticesForTrainee(int classId)
        {
            try
            {
                if (classId <= 0) return ErrorResponse("INVALID_ID", "ID positive", 400);
                var traineeId = GetTraineeId();
                var result = await _traineePracticesService.GetPracticesForTraineeAsync(traineeId, classId);
                return SuccessResponse(result);
            }
            catch (UnauthorizedAccessException ex) { return ErrorResponse("UNAUTHORIZED", ex.Message, 401); }
            catch (KeyNotFoundException ex) { return ErrorResponse("NOT_FOUND", ex.Message, 404); }
            catch (Exception ex) { return ErrorResponse("FETCH_ERROR", "Failed to fetch", 500, new { ex.Message }); }
        }

        [HttpGet("trainee/activity-record/{activityRecordId}")]
        [Authorize(Roles = "Trainee")]
        public async Task<IActionResult> GetPracticeForTraineeByActivityId(int activityRecordId)
        {
            try
            {
                if (activityRecordId <= 0) return ErrorResponse("INVALID_ID", "ID positive", 400);
                var traineeId = GetTraineeId();
                var result = await _traineePracticesService.GetPracticeForTraineeByActivityIdAsync(traineeId, activityRecordId);
                if (result == null) return ErrorResponse("NOT_FOUND", "Practice not found", 404);
                return SuccessResponse(result);
            }
            catch (UnauthorizedAccessException ex) { return ErrorResponse("UNAUTHORIZED", ex.Message, 401); }
            catch (KeyNotFoundException ex) { return ErrorResponse("NOT_FOUND", ex.Message, 404); }
            catch (Exception ex) { return ErrorResponse("FETCH_ERROR", "Failed to fetch", 500, new { ex.Message }); }
        }

        #endregion

        private int GetTraineeId()
        {
            var claim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (int.TryParse(claim, out int id)) return id;
            throw new UnauthorizedAccessException("Trainee ID invalid");
        }
    }
}