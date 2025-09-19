using Lssctc.LearningManagement.Quizzes.DTOs;
using Lssctc.LearningManagement.Quizzes.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace Lssctc.LearningManagement.Quizzes.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class QuizzesController : ControllerBase
    {
        private readonly IQuizService _quizService;

        public QuizzesController(IQuizService quizService)
        {
            _quizService = quizService;
        }

        // GET: /api/quizzes?pageIndex=1&pageSize=20&search=an%20toan
        [HttpGet]
        public async Task<IActionResult> GetPaged(
            [FromQuery] int pageIndex = 1,
            [FromQuery] int pageSize = 20,
            [FromQuery] string? search = null)
        {
            var (items, total) = await _quizService.GetPagedAsync(pageIndex, pageSize, search);

            // (Tuỳ chọn) đưa tổng vào header để client dễ đọc
            Response.Headers["X-Total-Count"] = total.ToString();

            return Ok(new
            {
                pageIndex,
                pageSize,
                total,
                items
            });
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById([FromRoute] int id)
        {
            var quiz = await _quizService.GetByIdAsync(id);
            return quiz is null ? NotFound() : Ok(quiz);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateQuizDto dto)
        {
            var id = await _quizService.CreateAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id }, new { id });
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update([FromRoute] int id, [FromBody] UpdateQuizDto dto)
        {
            var ok = await _quizService.UpdateAsync(id, dto);
            return ok ? NoContent() : NotFound();
        }


        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete([FromRoute] int id)
        {
            var ok = await _quizService.DeleteAsync(id);
            return ok ? NoContent() : NotFound();
        }


        // add questions to quiz
        [HttpPost("{quizId:int}/questions")]
        public async Task<IActionResult> CreateQuestion([FromRoute] int quizId, [FromBody] CreateQuizQuestionDto dto)
        {
            try
            {
                var id = await _quizService.CreateQuestionsAsync(quizId, dto);
                return CreatedAtAction(nameof(GetById), new { quizId, id }, new { id });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { error = ex.Message });
            }
            catch (ValidationException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }


        // create question options in bulk


        // SINGLE (append 1 item): bỏ qua DisplayOrder client, luôn nối tiếp currentMax
        [HttpPost("{questionId:int}/options")]
        public async Task<IActionResult> Create([FromRoute] int questionId, [FromBody] CreateQuizQuestionOptionDto dto)
        {
            if (!ModelState.IsValid) return ValidationProblem(ModelState);
            try
            {
                var id = await _quizService.CreateQuizQuestionOptionAsync(questionId, dto);
                return CreatedAtAction(nameof(GetById), new { questionId, id }, new { id });
            }
            catch (KeyNotFoundException ex) { return NotFound(new { error = ex.Message }); }
            catch (ValidationException ex) { return BadRequest(new { error = ex.Message }); }
        }

        // BULK: giữ thứ tự payload (1..n), nhưng gán DisplayOrder = currentMax + 1..n
        [HttpPost("{questionId:int}/options/bulk")]
        public async Task<IActionResult> CreateBulk([FromRoute] int questionId, [FromBody] CreateQuizQuestionOptionBulkDto dto)
        {
            if (!ModelState.IsValid) return ValidationProblem(ModelState);
            try
            {
                var ids = await _quizService.CreateQuizQuestionOptionsBulkAsync(questionId, dto);
                return Ok(new { count = ids.Count, ids });
            }
            catch (KeyNotFoundException ex) { return NotFound(new { error = ex.Message }); }
            catch (ValidationException ex) { return BadRequest(new { error = ex.Message }); }
        }

        // stub để CreatedAtAction dùng được
        [HttpGet("{questionId:int}/options/{id:int}")]
        public IActionResult GetById(int questionId, int id) => Ok();


    }
}
