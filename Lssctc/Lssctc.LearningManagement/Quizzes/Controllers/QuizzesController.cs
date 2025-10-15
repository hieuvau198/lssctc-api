using Lssctc.LearningManagement.QuizQuestions.DTOs;
using Lssctc.LearningManagement.Quizzes.DTOs;
using Lssctc.LearningManagement.Quizzes.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using static Lssctc.LearningManagement.QuizQuestions.DTOs.QuizQuestionDto;

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

        // GET: /api/quizzes?pageIndex=1&pageSize=20
        [HttpGet]
        public async Task<IActionResult> GetDetailQuizzes(
            [FromQuery] int pageIndex = 1,
            [FromQuery] int pageSize = 20,
            CancellationToken ct = default)
        {
            if (pageIndex < 1) return BadRequest("pageIndex must be >= 1.");
            if (pageSize < 1 || pageSize > 200) return BadRequest("pageSize must be between 1 and 200.");

            var result = await _quizService.GetAllQuizzesPaged(pageIndex, pageSize);

            // Cho phép FE đọc header này qua CORS
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


        // add questions to quiz
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

        //==== add options to question

        // add option to a question
        [HttpPost("questions/{questionId:int}/options")]
        public async Task<IActionResult> CreateOption(
     [FromRoute] int questionId,
     [FromBody] CreateQuizQuestionOptionDto dto)
        {
            try
            {
                var id = await _quizService.CreateOption(questionId, dto);

                // Trả về CreatedAtAction với route: GetOptionById
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


        //=== = get option by id (for CreatedAtAction above)
        [HttpGet("questions/{questionId:int}/options/{optionId:int}")]
        public async Task<IActionResult> GetOptionById(
    [FromRoute] int questionId,
    [FromRoute] int optionId)
        {
            var dto = await _quizService.GetOptionById(optionId);
            return dto is null ? NotFound() : Ok(dto);
        }


        // stub để CreatedAtAction dùng được
        [HttpGet("{questionId:int}/options/{id:int}")]
        public IActionResult GetById(int questionId, int id) => Ok();

        // get only in4 of quiz for teacher 
        [HttpGet("{id:int}/questions")]
        public async Task<IActionResult> GetQuestionsPaged(
             [FromRoute] int id,
             [FromQuery] int page = 1,
             [FromQuery] int pageSize = 20)
        {
            if (id <= 0) return BadRequest("id must be a positive integer.");

            var result = await _quizService.GetQuestionsByQuizIdPaged(id, page, pageSize);
            return Ok(result);
        }

        // get quiz detail for trainee (no correct option info)
        [HttpGet("{id:int}/traineequiz-view")]
        public async Task<IActionResult> GetQuizForTrainee(int id, CancellationToken ct = default)
        {
            var dto = await _quizService.GetQuizDetailForTrainee(id, ct);
            return dto is null ? NotFound() : Ok(dto);
        }

        // GET: /api/quizzes/questions/{questionId}/options  (get option by question id)
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


        // GET /api/Quizzes/by-section-quiz/{sectionQuizId}/trainee-view
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
                // TODO: log ex
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { error = "Unexpected error.", detail = ex.Message });
            }
        }

        // create question and option by quiz id

        // POST: /api/quizzes/{quizId}/questions-with-options
        [HttpPost("{quizId:int}/questions-with-options")]
        public async Task<IActionResult> CreateQuestionWithOptions(
            [FromRoute] int quizId,
            [FromBody] CreateQuizQuestionWithOptionsDto dto)
        {
            if (quizId <= 0) return BadRequest("quizId must be a positive integer.");
            if (dto is null) return BadRequest("Body is required.");

            try
            {
                var questionId = await _quizService.CreateQuestionWithOptionsByQuizId(quizId, dto);

                // Điều hướng tới GET /api/quizzes/{id}/questions?page=1&pageSize=20 (endpoint mới)
                return CreatedAtAction(
                    nameof(GetQuestionsPaged),
                    new { id = quizId, page = 1, pageSize = 20 }, // route values khớp action đích
                    new { id = questionId }                        // body trả về
                );
            }
            catch (KeyNotFoundException ex) { return NotFound(new { error = ex.Message }); }
            catch (ValidationException ex) { return BadRequest(new { error = ex.Message }); }
        }



    }
}
