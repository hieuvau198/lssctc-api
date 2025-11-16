using Lssctc.ProgramManagement.Quizzes.DTOs;
using Lssctc.ProgramManagement.Quizzes.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

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
        public async Task<IActionResult> GetQuizzes([FromQuery] int pageIndex = 1, [FromQuery] int pageSize = 10)
        {
            try
            {
                var result = await _service.GetQuizzes(pageIndex, pageSize);
                return Ok(new { status = 200, message = "Get quizzes", data = result });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { status = 500, message = ex.Message, type = ex.GetType().Name });
            }
        }

        [HttpGet("detail")]
        [Authorize(Roles = "Admin, Instructor")]
        public async Task<IActionResult> GetDetailQuizzes([FromQuery] int pageIndex = 1, [FromQuery] int pageSize = 10)
        {
            try
            {
                var result = await _service.GetDetailQuizzes(pageIndex, pageSize);
                return Ok(new { status = 200, message = "Get detail quizzes", data = result });
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
                var dto = await _service.GetQuizById(id);
                if (dto == null) 
                    return NotFound(new { status = 404, message = $"Quiz with ID {id} not found", type = "NotFound" });
                return Ok(new { status = 200, message = "Get quiz", data = dto });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { status = 500, message = ex.Message, type = ex.GetType().Name });
            }
        }

        [HttpGet("{id}/full")]
        [Authorize(Roles = "Admin, Instructor")]
        public async Task<IActionResult> GetQuizDetail(int id)
        {
            try
            {
                var dto = await _service.GetQuizDetail(id);
                if (dto == null) 
                    return NotFound(new { status = 404, message = $"Quiz detail with ID {id} not found", type = "NotFound" });
                return Ok(new { status = 200, message = "Get quiz detail", data = dto });
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

        [HttpPost]
        [Authorize(Roles = "Admin, Instructor")]
        public async Task<IActionResult> CreateQuiz([FromBody] CreateQuizDto dto)
        {
            try
            {
                if (!ModelState.IsValid) 
                    return BadRequest(new { status = 400, message = "Invalid model state", type = "ValidationException", errors = ModelState });
                var id = await _service.CreateQuiz(dto);
                return Ok(new { status = 200, message = "Create quiz successfully", data = new { id } });
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

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin, Instructor")]
        public async Task<IActionResult> UpdateQuiz(int id, [FromBody] UpdateQuizDto dto)
        {
            try
            {
                if (!ModelState.IsValid) 
                    return BadRequest(new { status = 400, message = "Invalid model state", type = "ValidationException", errors = ModelState });
                
                var ok = await _service.UpdateQuizById(id, dto);
                if (!ok) 
                    return NotFound(new { status = 404, message = $"Quiz with ID {id} not found", type = "NotFound" });
                return Ok(new { status = 200, message = "Update quiz successfully" });
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
                var ok = await _service.DeleteQuizById(id);
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

        [HttpPost("{quizId}/questions")]
        [Authorize(Roles = "Admin, Instructor")]
        public async Task<IActionResult> CreateQuestionWithOptions(int quizId, [FromBody] CreateQuizQuestionWithOptionsDto dto)
        {
            try
            {
                if (!ModelState.IsValid) 
                    return BadRequest(new { status = 400, message = "Invalid model state", type = "ValidationException", errors = ModelState });
                var questionId = await _service.CreateQuestionWithOptionsByQuizId(quizId, dto);
                return Ok(new { status = 200, message = "Create question with options successfully", data = new { questionId } });
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
                var quizId = await _service.CreateQuizWithQuestions(dto);
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
    }
}
