using Lssctc.ProgramManagement.Quizzes.DTOs;
using Lssctc.ProgramManagement.Quizzes.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;

namespace Lssctc.ProgramManagement.Quizzes.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class QuizzesController : ControllerBase
    {
        private readonly IQuizService _service;

        public QuizzesController(IQuizService service)
        {
            _service = service;
        }

        [HttpGet]
        [Authorize(Roles = "Admin, Instructor")]
        public async Task<IActionResult> GetQuizzes(
            [FromQuery] int pageIndex = 1, 
            [FromQuery] int pageSize = 10,
            [FromQuery] string? searchTerm = null,
            [FromQuery] string? sortBy = null,
            [FromQuery] string? sortDirection = null)
        {
            try
            {
                // Extract instructor ID from JWT claims if user is Instructor (not Admin)
                int? instructorId = null;
                if (User.IsInRole("Instructor"))
                {
                    instructorId = GetInstructorIdFromClaims();
                }
                
                var result = await _service.GetQuizzes(pageIndex, pageSize, instructorId, searchTerm, sortBy, sortDirection);
                return Ok(new { status = 200, message = "Get quizzes", data = result });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { status = 500, message = ex.Message, type = ex.GetType().Name });
            }
        }

        [HttpGet("{id}")]
        [Authorize(Roles = "Admin, Instructor")]
        public async Task<IActionResult> GetQuizById(int id)
        {
            try
            {
                // Extract instructor ID from JWT claims if user is Instructor (not Admin)
                int? instructorId = null;
                if (User.IsInRole("Instructor"))
                {
                    instructorId = GetInstructorIdFromClaims();
                }

                var dto = await _service.GetQuizById(id, instructorId);
                if (dto == null) 
                    return NotFound(new { status = 404, message = $"Quiz with ID {id} not found", type = "NotFound" });
                return Ok(new { status = 200, message = "Get quiz", data = dto });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { status = 500, message = ex.Message, type = ex.GetType().Name });
            }
        }

        [HttpGet("{id}/for-trainee")]
        [Authorize(Roles = "Trainee, Admin")]
        public async Task<IActionResult> GetQuizForTrainee(int id)
        {
            try
            {
                var dto = await _service.GetQuizDetailForTrainee(id);
                if (dto == null) 
                    return NotFound(new { status = 404, message = "Quiz not found for trainee", type = "NotFound" });
                return Ok(new { status = 200, message = "Get quiz for trainee", data = dto });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { status = 500, message = ex.Message, type = ex.GetType().Name });
            }
        }

        [HttpGet("for-trainee/activity/{activityId}")]
        [Authorize(Roles = "Trainee, Admin")]
        [ProducesResponseType(typeof(QuizTraineeDetailDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetQuizForTraineeByActivityId(int activityId)
        {
            try
            {
                var dto = await _service.GetQuizDetailForTraineeByActivityIdAsync(activityId);
                if (dto == null) 
                    return NotFound(new { status = 404, message = "No quiz found for this activity", type = "NotFound" });
                return Ok(new { status = 200, message = "Get quiz for trainee by activity", data = dto });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { status = 404, message = ex.Message, type = "NotFound" });
            }
            catch (ValidationException ex)
            {
                return BadRequest(new { status = 400, message = ex.Message, type = "ValidationException" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { status = 500, message = ex.Message, type = ex.GetType().Name });
            }
        }

        [HttpPut("{id}/with-questions")]
        [Authorize(Roles = "Admin, Instructor")]
        public async Task<IActionResult> UpdateQuizWithQuestions(int id, [FromBody] UpdateQuizWithQuestionsDto dto)
        {
            try
            {
                if (!ModelState.IsValid) 
                    return BadRequest(new { status = 400, message = "Invalid model state", type = "ValidationException", errors = ModelState });
                
                // Extract instructor ID from JWT claims if user is Instructor (not Admin)
                int? instructorId = null;
                if (User.IsInRole("Instructor"))
                {
                    instructorId = GetInstructorIdFromClaims();
                }

                var ok = await _service.UpdateQuizWithQuestionsAsync(id, dto, instructorId);
                if (!ok) 
                    return NotFound(new { status = 404, message = $"Quiz with ID {id} not found", type = "NotFound" });
                return Ok(new { status = 200, message = "Update quiz with questions successfully" });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { status = 404, message = ex.Message, type = "NotFound" });
            }
            catch (ValidationException ex)
            {
                return BadRequest(new { status = 400, message = ex.Message, type = "ValidationException" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { status = 500, message = ex.Message, type = ex.GetType().Name });
            }
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin, Instructor")]
        public async Task<IActionResult> DeleteQuiz(int id)
        {
            try
            {
                // Extract instructor ID from JWT claims if user is Instructor (not Admin)
                int? instructorId = null;
                if (User.IsInRole("Instructor"))
                {
                    instructorId = GetInstructorIdFromClaims();
                }

                var ok = await _service.DeleteQuizById(id, instructorId);
                if (!ok) 
                    return NotFound(new { status = 404, message = $"Quiz with ID {id} not found", type = "NotFound" });
                return Ok(new { status = 200, message = "Delete quiz successfully" });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { status = 404, message = ex.Message, type = "NotFound" });
            }
            catch (ValidationException ex)
            {
                return BadRequest(new { status = 400, message = ex.Message, type = "ValidationException" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { status = 500, message = ex.Message, type = ex.GetType().Name });
            }
        }

        [HttpPost("with-questions")]
        [Authorize(Roles = "Admin, Instructor")]
        public async Task<IActionResult> CreateQuizWithQuestions([FromBody] CreateQuizWithQuestionsDto dto)
        {
            try
            {
                if (!ModelState.IsValid) 
                    return BadRequest(new { status = 400, message = "Invalid model state", type = "ValidationException", errors = ModelState });
                
                // Extract instructor ID from JWT claims
                var instructorId = GetInstructorIdFromClaims();
                
                var quizId = await _service.CreateQuizWithQuestions(dto, instructorId);
                return Ok(new { status = 200, message = "Create quiz successfully", data = new { quizId } });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { status = 404, message = ex.Message, type = "NotFound" });
            }
            catch (ValidationException ex)
            {
                return BadRequest(new { status = 400, message = ex.Message, type = "ValidationException" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { status = 500, message = ex.Message, type = ex.GetType().Name });
            }
        }


        [HttpPost("import-excel")]
        [Authorize(Roles = "Admin, Instructor")]
        public async Task<IActionResult> ImportQuizFromExcel([FromForm] ImportQuizExcelDto input)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(new { status = 400, message = "Invalid model state", type = "ValidationException", errors = ModelState });

                
                var instructorId = GetInstructorIdFromClaims();

                var quizId = await _service.CreateQuizFromExcel(input, instructorId);

                return Ok(new { status = 200, message = "Import quiz from excel successfully", data = new { quizId } });
            }
            catch (ValidationException ex)
            {
                // Bắt lỗi validation (ví dụ: sai tổng điểm, thiếu file...)
                return BadRequest(new { status = 400, message = ex.Message, type = "ValidationException" });
            }
            catch (UnauthorizedAccessException ex)
            {
                // Bắt lỗi auth từ GetInstructorIdFromClaims
                return Unauthorized(new { status = 401, message = ex.Message, type = "Unauthorized" });
            }
            catch (Exception ex)
            {
                // Lỗi hệ thống khác
                return StatusCode(500, new { status = 500, message = ex.Message, type = ex.GetType().Name });
            }
        }


        [HttpPost("add-to-activity")]
        [Authorize(Roles = "Admin, Instructor")]
        public async Task<IActionResult> AddQuizToActivity([FromBody] CreateActivityQuizDto dto)
        {
            try
            {
                if (!ModelState.IsValid) 
                    return BadRequest(new { status = 400, message = "Invalid model state", type = "ValidationException", errors = ModelState });
                var activityQuizId = await _service.AddQuizToActivity(dto);
                return Ok(new { status = 200, message = "Add quiz to activity successfully", data = new { activityQuizId } });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { status = 404, message = ex.Message, type = "NotFound" });
            }
            catch (ValidationException ex)
            {
                return BadRequest(new { status = 400, message = ex.Message, type = "ValidationException" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { status = 500, message = ex.Message, type = ex.GetType().Name });
            }
        }

        [HttpGet("activity/{activityId}/quizzes")]
        [Authorize(Roles = "Admin, Instructor")]
        public async Task<IActionResult> GetQuizzesByActivityId(int activityId)
        {
            try
            {
                var quizzes = await _service.GetQuizzesByActivityId(activityId);
                return Ok(new { status = 200, message = "Get quizzes by activity successfully", data = quizzes });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { status = 404, message = ex.Message, type = "NotFound" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { status = 500, message = ex.Message, type = ex.GetType().Name });
            }
        }

        [HttpDelete("activity/{activityId}/quiz/{quizId}")]
        [Authorize(Roles = "Admin, Instructor")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> RemoveQuizFromActivity(int activityId, int quizId)
        {
            try
            {
                await _service.RemoveQuizFromActivityAsync(activityId, quizId);
                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                // This catches the "activity in use" error
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = ex.Message });
            }
        }

        [HttpPut("activity/{activityId}")]
        [Authorize(Roles = "Admin, Instructor")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateQuizInActivity(int activityId, [FromBody] UpdateActivityQuizDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            try
            {
                await _service.UpdateQuizInActivityAsync(activityId, dto);
                return Ok(new { message = "Quiz assignment updated successfully." });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                // This catches "activity in use" or "not a quiz activity" errors
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = ex.Message });
            }
        }

        private int GetInstructorIdFromClaims()
        {
            // Try to get instructorId claim first, otherwise use NameIdentifier
            var instructorIdClaim = User.FindFirstValue("instructorId") ?? User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (int.TryParse(instructorIdClaim, out int instructorId))
            {
                return instructorId;
            }

            throw new UnauthorizedAccessException("Instructor ID claim is missing or invalid.");
        }
    }
}
