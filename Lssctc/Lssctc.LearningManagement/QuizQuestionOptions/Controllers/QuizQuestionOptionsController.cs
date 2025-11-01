using Lssctc.LearningManagement.QuizQuestionOptions.DTOs;
using Lssctc.LearningManagement.QuizQuestionOptions.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace Lssctc.LearningManagement.QuizQuestionOptions.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class QuizQuestionOptionsController : ControllerBase
    {
        IQuizQuestionOptionsService _quizService;
        public QuizQuestionOptionsController(IQuizQuestionOptionsService quizService)
        {
            _quizService = quizService;
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

        [HttpPut("options/{optionId:int}")]
        public async Task<IActionResult> UpdateOption(
            [FromRoute] int optionId,
            [FromBody] UpdateQuizQuestionOptionDto dto)
        {
            try
            {
                await _quizService.UpdateOption(optionId, dto);
                return NoContent();
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

        [HttpDelete("options/{optionId:int}")]
        public async Task<IActionResult> DeleteOption([FromRoute] int optionId)
        {
            try
            {
                await _quizService.DeleteOption(optionId);
                return NoContent();
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
    }
}
