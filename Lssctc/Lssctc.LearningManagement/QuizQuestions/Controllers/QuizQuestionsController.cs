using Lssctc.LearningManagement.QuizQuestions.DTOs;
using Lssctc.LearningManagement.QuizQuestions.Services;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace Lssctc.LearningManagement.QuizQuestions.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class QuizQuestionsController : ControllerBase
    {
        private readonly IQuizQuestionService _quizQuestionService;

        public QuizQuestionsController(IQuizQuestionService quizQuestionService)
        {
            _quizQuestionService = quizQuestionService;
        }

        /// <summary>
        /// Create a new question for a specific quiz
        /// </summary>
        /// <param name="quizId">The quiz ID</param>
        /// <param name="dto">The question data</param>
        /// <returns>Created question ID</returns>
        [HttpPost("quiz/{quizId:int}/questions")]
        public async Task<IActionResult> CreateQuestionByQuizId([FromRoute] int quizId, [FromBody] CreateQuizQuestionDto dto)
        {
            try
            {
                var questionId = await _quizQuestionService.CreateQuestionByQuizId(quizId, dto);
                return CreatedAtAction(
                    nameof(GetQuestionById), 
                    new { questionId }, 
                    new { id = questionId }
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
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "An internal server error occurred." });
            }
        }

        /// <summary>
        /// Get paginated questions for a specific quiz
        /// </summary>
        /// <param name="quizId">The quiz ID</param>
        /// <param name="page">Page number (default: 1)</param>
        /// <param name="pageSize">Page size (default: 20)</param>
        /// <returns>Paginated list of questions</returns>
        [HttpGet("quiz/{quizId:int}/questions")]
        public async Task<IActionResult> GetQuestionsByQuizIdPaged(
            [FromRoute] int quizId,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20)
        {
            try
            {
                // Validate pagination parameters
                if (page < 1)
                    return BadRequest(new { error = "Page must be greater than 0." });

                if (pageSize < 1 || pageSize > 100)
                    return BadRequest(new { error = "PageSize must be between 1 and 100." });

                var result = await _quizQuestionService.GetQuestionsByQuizIdPaged(quizId, page, pageSize);
                return Ok(result);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "An internal server error occurred." });
            }
        }

        /// <summary>
        /// Get a specific question by ID
        /// </summary>
        /// <param name="questionId">The question ID</param>
        /// <returns>Question details</returns>
        [HttpGet("questions/{questionId:int}")]
        public async Task<IActionResult> GetQuestionById([FromRoute] int questionId)
        {
            try
            {
                var question = await _quizQuestionService.GetQuestionById(questionId);
                if (question == null)
                    return NotFound(new { error = $"Question with ID {questionId} not found." });

                return Ok(question);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "An internal server error occurred." });
            }
        }

        /// <summary>
        /// Update a specific question
        /// </summary>
        /// <param name="questionId">The question ID</param>
        /// <param name="dto">The updated question data</param>
        /// <returns>Success or error response</returns>
        [HttpPut("questions/{questionId:int}")]
        public async Task<IActionResult> UpdateQuestionById([FromRoute] int questionId, [FromBody] UpdateQuizQuestionDto dto)
        {
            try
            {
                var success = await _quizQuestionService.UpdateQuestionById(questionId, dto);
                if (!success)
                    return NotFound(new { error = $"Question with ID {questionId} not found." });

                return NoContent();
            }
            catch (ValidationException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "An internal server error occurred." });
            }
        }

        /// <summary>
        /// Delete a specific question
        /// </summary>
        /// <param name="questionId">The question ID</param>
        /// <returns>Success or error response</returns>
        [HttpDelete("questions/{questionId:int}")]
        public async Task<IActionResult> DeleteQuestionById([FromRoute] int questionId)
        {
            try
            {
                var success = await _quizQuestionService.DeleteQuestionById(questionId);
                if (!success)
                    return NotFound(new { error = $"Question with ID {questionId} not found." });

                return NoContent();
            }
            catch (ValidationException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "An internal server error occurred." });
            }
        }
    }
}
