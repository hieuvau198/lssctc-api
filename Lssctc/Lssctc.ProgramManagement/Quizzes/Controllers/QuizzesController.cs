using Lssctc.ProgramManagement.Quizzes.DTOs;
using Lssctc.ProgramManagement.Quizzes.Services;
using Microsoft.AspNetCore.Mvc;

namespace Lssctc.ProgramManagement.Quizzes.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class QuizzesController : ControllerBase
    {
        private readonly IQuizService _service;

        public QuizzesController(IQuizService service)
        {
            _service = service;
        }

        [HttpGet("ping")]
        public IActionResult Ping() => Ok("pong");

        [HttpGet]
        public async Task<IActionResult> GetQuizzes([FromQuery] int pageIndex = 1, [FromQuery] int pageSize = 10)
        {
            var result = await _service.GetQuizzes(pageIndex, pageSize);
            return Ok(result);
        }

        [HttpGet("detail")]
        public async Task<IActionResult> GetDetailQuizzes([FromQuery] int pageIndex = 1, [FromQuery] int pageSize = 10)
        {
            var result = await _service.GetDetailQuizzes(pageIndex, pageSize);
            return Ok(result);
        }

        [HttpGet("debug/summary")]
        public async Task<IActionResult> GetDebugSummary([FromQuery] int pageIndex = 1, [FromQuery] int pageSize = 100)
        {
            var details = await _service.GetDetailQuizzes(pageIndex, pageSize);
            var summary = details.Items.Select(q => new
            {
                q.Id,
                q.Name,
                QuestionCount = q.Questions?.Count ?? 0,
                OptionsCount = q.Questions?.Sum(qq => qq.Options?.Count ?? 0) ?? 0
            }).ToList();

            return Ok(new { details.Page, details.PageSize, details.TotalCount, Summary = summary });
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetQuizById(int id)
        {
            var dto = await _service.GetQuizById(id);
            if (dto == null) return NotFound();
            return Ok(dto);
        }

        [HttpGet("{id}/full")]
        public async Task<IActionResult> GetQuizDetail(int id)
        {
            var dto = await _service.GetQuizDetail(id);
            if (dto == null) return NotFound();
            return Ok(dto);
        }

        [HttpGet("{id}/for-trainee")]
        public async Task<IActionResult> GetQuizForTrainee(int id)
        {
            var dto = await _service.GetQuizDetailForTrainee(id);
            if (dto == null) return NotFound();
            return Ok(dto);
        }

        [HttpPost]
        public async Task<IActionResult> CreateQuiz([FromBody] CreateQuizDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var id = await _service.CreateQuiz(dto);
            return CreatedAtAction(nameof(GetQuizById), new { id }, null);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateQuiz(int id, [FromBody] UpdateQuizDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var ok = await _service.UpdateQuizById(id, dto);
            if (!ok) return NotFound();
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteQuiz(int id)
        {
            var ok = await _service.DeleteQuizById(id);
            if (!ok) return NotFound();
            return NoContent();
        }

        [HttpPost("{quizId}/questions")]
        public async Task<IActionResult> CreateQuestionWithOptions(int quizId, [FromBody] CreateQuizQuestionWithOptionsDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var questionId = await _service.CreateQuestionWithOptionsByQuizId(quizId, dto);
            return CreatedAtAction(nameof(GetQuizDetail), new { id = quizId }, null);
        }
    }
}
