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
            var result = await _service.GetQuizzes(pageIndex, pageSize);
            return Ok(result);
        }

        [HttpGet("detail")]
        [Authorize(Roles = "Admin, Instructor")]
        public async Task<IActionResult> GetDetailQuizzes([FromQuery] int pageIndex = 1, [FromQuery] int pageSize = 10)
        {
            var result = await _service.GetDetailQuizzes(pageIndex, pageSize);
            return Ok(result);
        }

      

        [HttpGet("{id}")]
        [Authorize(Roles = "Admin, Instructor")]
        public async Task<IActionResult> GetQuizById(int id)
        {
            var dto = await _service.GetQuizById(id);
            if (dto == null) return NotFound();
            return Ok(dto);
        }

        [HttpGet("{id}/full")]
        [Authorize(Roles = "Admin, Instructor")]
        public async Task<IActionResult> GetQuizDetail(int id)
        {
            var dto = await _service.GetQuizDetail(id);
            if (dto == null) return NotFound();
            return Ok(dto);
        }

        [HttpGet("{id}/for-trainee")]
        [Authorize(Roles = "Trainee, Admin")]
        public async Task<IActionResult> GetQuizForTrainee(int id)
        {
            var dto = await _service.GetQuizDetailForTrainee(id);
            if (dto == null) return NotFound();
            return Ok(dto);
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
                {
                    return NotFound(new { message = "No quiz found for this activity." });
                }
                return Ok(dto);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = ex.Message });
            }
        }

        [HttpPost]
        [Authorize(Roles = "Admin, Instructor")]
        public async Task<IActionResult> CreateQuiz([FromBody] CreateQuizDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var id = await _service.CreateQuiz(dto);
            return CreatedAtAction(nameof(GetQuizById), new { id }, null);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin, Instructor")]
        public async Task<IActionResult> UpdateQuiz(int id, [FromBody] UpdateQuizDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var ok = await _service.UpdateQuizById(id, dto);
            if (!ok) return NotFound();
            return NoContent();
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin, Instructor")]
        public async Task<IActionResult> DeleteQuiz(int id)
        {
            var ok = await _service.DeleteQuizById(id);
            if (!ok) return NotFound();
            return NoContent();
        }

        [HttpPost("{quizId}/questions")]
        [Authorize(Roles = "Admin, Instructor")]
        public async Task<IActionResult> CreateQuestionWithOptions(int quizId, [FromBody] CreateQuizQuestionWithOptionsDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var questionId = await _service.CreateQuestionWithOptionsByQuizId(quizId, dto);
            return CreatedAtAction(nameof(GetQuizDetail), new { id = quizId }, null);
        }

        [HttpPost("with-questions")]
        [Authorize(Roles = "Admin, Instructor")]
        public async Task<IActionResult> CreateQuizWithQuestions([FromBody] CreateQuizWithQuestionsDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var quizId = await _service.CreateQuizWithQuestions(dto);
            return CreatedAtAction(nameof(GetQuizDetail), new { id = quizId }, null);
        }

        [HttpPost("add-to-activity")]
        [Authorize(Roles = "Admin, Instructor")]
        public async Task<IActionResult> AddQuizToActivity([FromBody] CreateActivityQuizDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            try
            {
                var activityQuizId = await _service.AddQuizToActivity(dto);
                return Ok(new { activityQuizId, message = "Quiz successfully added to activity." });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (ValidationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = ex.Message });
            }
        }

        [HttpGet("activity/{activityId}/quizzes")]
        [Authorize(Roles = "Admin, Instructor")]
        public async Task<IActionResult> GetQuizzesByActivityId(int activityId)
        {
            try
            {
                var quizzes = await _service.GetQuizzesByActivityId(activityId);
                return Ok(quizzes);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = ex.Message });
            }
        }
    }
}
