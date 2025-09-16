using Lssctc.LearningManagement.Quizzes.DTOs;
using Lssctc.LearningManagement.Quizzes.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

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

    }
}
