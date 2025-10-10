using Lssctc.ProgramManagement.Quizzes.DTOs;
using Lssctc.ProgramManagement.Quizzes.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace Lssctc.ProgramManagement.Quizzes.Controllers
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

        [HttpGet]
        public async Task<IActionResult> GetDetailQuizzes(
            [FromQuery] int pageIndex = 1,
            [FromQuery] int pageSize = 20,
            CancellationToken ct = default)
        {
            if (pageIndex < 1) return BadRequest("pageIndex must be >= 1.");
            if (pageSize < 1 || pageSize > 200) return BadRequest("pageSize must be between 1 and 200.");

            var result = await _quizService.GetDetailQuizzes(pageIndex, pageSize);

            Response.Headers["X-Total-Count"] = result.TotalCount.ToString();
            Response.Headers["Access-Control-Expose-Headers"] = "X-Total-Count";

            return Ok(result);
        }


        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById([FromRoute] int id)
        {
            var quiz = await _quizService.GetQuizById(id);
            return quiz is null ? NotFound() : Ok(quiz);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateQuizDto dto)
        {
            var id = await _quizService.CreateQuiz(dto);
            return CreatedAtAction(nameof(GetById), new { id }, new { id });
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update([FromRoute] int id, [FromBody] UpdateQuizDto dto)
        {
            var ok = await _quizService.UpdateQuizById(id, dto);
            return ok ? NoContent() : NotFound();
        }


        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete([FromRoute] int id)
        {
            var ok = await _quizService.DeleteQuizById(id);
            return ok ? NoContent() : NotFound();
        }


        [HttpPost("{quizId:int}/questions")]
        public async Task<IActionResult> CreateQuestion([FromRoute] int quizId, [FromBody] CreateQuizQuestionDto dto)
        {
            try
            {
                var id = await _quizService.CreateQuestionByQuizId(quizId, dto);
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


        [HttpPost("questions/{questionId:int}/options")]
        public async Task<IActionResult> CreateOption(
     [FromRoute] int questionId,
     [FromBody] CreateQuizQuestionOptionDto dto)
        {
            try
            {
                var id = await _quizService.CreateOption(questionId, dto);

                return CreatedAtAction(
                    nameof(GetOptionById),
                    new { questionId, optionId = id },
                    new { id }
                );
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


        [HttpGet("questions/{questionId:int}/options/{optionId:int}")]
        public async Task<IActionResult> GetOptionById(
    [FromRoute] int questionId,
    [FromRoute] int optionId)
        {
            var dto = await _quizService.GetOptionById(optionId);
            return dto is null ? NotFound() : Ok(dto);
        }


        [HttpGet("{questionId:int}/options/{id:int}")]
        public IActionResult GetById(int questionId, int id) => Ok();

        [HttpGet("{id:int}/questions")]
        public async Task<IActionResult> GetDetail(int id, CancellationToken ct = default)
        {
            var dto = await _quizService.GetQuizDetail(id, ct);
            return dto is null ? NotFound() : Ok(dto);
        }

        [HttpGet("{id:int}/no-answer")]
        public async Task<IActionResult> GetDetailNoAnswer(int id, CancellationToken ct = default)
        {
            var dto = await _quizService.GetQuizDetailNoAnswer(id, ct);
            return dto is null ? NotFound() : Ok(dto);
        }


        [HttpGet("{id:int}/traineequiz-view")]
        public async Task<IActionResult> GetQuizForTrainee(int id, CancellationToken ct = default)
        {
            var dto = await _quizService.GetQuizDetailForTrainee(id, ct);
            return dto is null ? NotFound() : Ok(dto);
        }

        [HttpGet("questions/{questionId:int}/options")]
        public async Task<IActionResult> GetOptionsByQuestionIdFlat(int questionId, CancellationToken ct = default)
        {
            try
            {
                var items = await _quizService.GetOptionsByQuestionId(questionId, ct);
                return Ok(new { questionId, total = items.Count, items });
            }
            catch (KeyNotFoundException ex) { return NotFound(new { error = ex.Message }); }
            catch (ValidationException ex) { return BadRequest(new { error = ex.Message }); }
        }


        [HttpGet("by-section-quiz/{sectionQuizId:int}/trainee-view")]
        public async Task<IActionResult> GetQuizTraineeViewBySectionQuiz(
            [FromRoute] int sectionQuizId,
            CancellationToken ct = default)
        {
            if (sectionQuizId <= 0)
                return BadRequest(new { error = "sectionQuizId must be a positive integer." });

            try
            {
                var dto = await _quizService.GetQuizTraineeDetailBySectionQuizIdAsync(sectionQuizId, ct);
                if (dto == null)
                    return NotFound(new { error = $"No quiz (trainee view) found for section_quiz id = {sectionQuizId}." });

                return Ok(dto);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { error = "Unexpected error.", detail = ex.Message });
            }
        }


        [HttpPost("{quizId:int}/questions-with-options")]
        public async Task<IActionResult> CreateQuestionWithOptions(
    [FromRoute] int quizId,
    [FromBody] CreateQuizQuestionWithOptionsDto dto)
        {
            try
            {
                var questionId = await _quizService.CreateQuestionWithOptionsByQuizId(quizId, dto);

                return CreatedAtAction(
                    nameof(GetDetail), 
                    new { id = quizId },
                    new { id = questionId }
                );
            }
            catch (KeyNotFoundException ex) { return NotFound(new { error = ex.Message }); }
            catch (ValidationException ex) { return BadRequest(new { error = ex.Message }); }
        }



    }
}
