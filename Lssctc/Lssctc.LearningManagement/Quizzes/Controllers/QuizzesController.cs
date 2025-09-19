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

        //==== add options to question

        // add option to a question
        [HttpPost("{quizId:int}/questions/{questionId:int}/options")]
        public async Task<IActionResult> CreateOption(
            [FromRoute] int quizId,
            [FromRoute] int questionId,
            [FromBody] CreateQuizQuestionOptionDto dto)
        {
            try
            {
                var id = await _quizService.CreateQuizQuestionOptionAsync(quizId, questionId, dto);
                return CreatedAtAction(nameof(GetOptionById), new { quizId, questionId, optionId = id }, new { id });
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

        //=== = get option by id (for CreatedAtAction above)
        // get option by id
        [HttpGet("{quizId:int}/questions/{questionId:int}/options/{optionId:int}")]
        public async Task<IActionResult> GetOptionById(
            [FromRoute] int quizId,
            [FromRoute] int questionId,
            [FromRoute] int optionId)
        {
            try
            {
                var dto = await _quizService.GetQuizQuestionOptionByIdAsync(quizId, questionId, optionId);
                return dto is null ? NotFound() : Ok(dto);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { error = ex.Message });
            }
        }



    }
}
