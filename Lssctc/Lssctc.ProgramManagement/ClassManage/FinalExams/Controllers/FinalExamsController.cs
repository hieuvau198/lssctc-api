using Lssctc.ProgramManagement.ClassManage.FinalExams.Dtos;
using Lssctc.ProgramManagement.ClassManage.FinalExams.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Lssctc.ProgramManagement.ClassManage.FinalExams.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class FinalExamsController : ControllerBase
    {
        private readonly IFinalExamsService _finalExamsService;
        private readonly IFinalExamPartialService _partialService;
        private readonly IFinalExamSeService _seService;
        private readonly IFETemplateService _templateService;

        public FinalExamsController(
            IFinalExamsService finalExamsService,
            IFinalExamPartialService partialService,
            IFinalExamSeService seService,
            IFETemplateService templateService)
        {
            _finalExamsService = finalExamsService;
            _partialService = partialService;
            _seService = seService;
            _templateService = templateService;
        }

        private int GetUserIdFromClaims()
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (int.TryParse(userIdClaim, out int userId))
            {
                return userId;
            }

            throw new UnauthorizedAccessException("User ID claim is missing or invalid.");
        }

        #region FinalExam (Admin/Instructor)

        [HttpPost]
        [Authorize(Roles = "Admin, Instructor")]
        public async Task<IActionResult> CreateFinalExam([FromBody] CreateFinalExamDto dto)
        {
            try
            {
                var result = await _finalExamsService.CreateFinalExamAsync(dto);
                return CreatedAtAction(nameof(GetFinalExam), new { id = result.Id }, result);
            }
            catch (InvalidOperationException ex) { return BadRequest(new { message = ex.Message }); }
            catch (Exception ex) { return StatusCode(500, new { message = ex.Message }); }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetFinalExam(int id)
        {
            try
            {
                return Ok(await _finalExamsService.GetFinalExamByIdAsync(id));
            }
            catch (KeyNotFoundException ex) { return NotFound(new { message = ex.Message }); }
            catch (Exception ex) { return StatusCode(500, new { message = ex.Message }); }
        }

        [HttpGet("partial/{partialId}")]
        [Authorize(Roles = "Admin, Instructor")]
        public async Task<IActionResult> GetPartial(int partialId)
        {
            try
            {
                return Ok(await _partialService.GetFinalExamPartialByIdAsync(partialId));
            }
            catch (KeyNotFoundException ex) { return NotFound(new { message = ex.Message }); }
            catch (Exception ex) { return StatusCode(500, new { message = ex.Message }); }
        }

        [HttpGet("class/{classId}")]
        [Authorize(Roles = "Admin, Instructor")]
        public async Task<IActionResult> GetByClassForInstructor(int classId)
        {
            try
            {
                var result = await _finalExamsService.GetFinalExamsByClassAsync(classId);
                return Ok(result);
            }
            catch (Exception ex) { return StatusCode(500, new { message = ex.Message }); }
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin, Instructor")]
        public async Task<IActionResult> UpdateFinalExam(int id, [FromBody] UpdateFinalExamDto dto)
        {
            try
            {
                return Ok(await _finalExamsService.UpdateFinalExamAsync(id, dto));
            }
            catch (KeyNotFoundException ex) { return NotFound(new { message = ex.Message }); }
            catch (Exception ex) { return StatusCode(500, new { message = ex.Message }); }
        }

        [HttpGet("class/{classId}/config")]
        [Authorize]
        public async Task<IActionResult> GetClassExamConfig(int classId)
        {
            try
            {
                var result = await _partialService.GetClassExamConfigAsync(classId);
                return Ok(result);
            }
            catch (KeyNotFoundException ex) { return NotFound(new { message = ex.Message }); }
            catch (Exception ex) { return StatusCode(500, new { message = ex.Message }); }
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin, Instructor")]
        public async Task<IActionResult> DeleteFinalExam(int id)
        {
            try
            {
                await _finalExamsService.DeleteFinalExamAsync(id);
                return NoContent();
            }
            catch (KeyNotFoundException ex) { return NotFound(new { message = ex.Message }); }
            catch (Exception ex) { return StatusCode(500, new { message = ex.Message }); }
        }

        [HttpPost("{id}/generate-code")]
        [Authorize(Roles = "Admin, Instructor")]
        public async Task<IActionResult> GenerateCode(int id)
        {
            try
            {
                var code = await _finalExamsService.GenerateExamCodeAsync(id);
                return Ok(new { examCode = code });
            }
            catch (KeyNotFoundException ex) { return NotFound(new { message = ex.Message }); }
            catch (Exception ex) { return StatusCode(500, new { message = ex.Message }); }
        }

        [HttpPost("class/{classId}/finish")]
        [Authorize(Roles = "Admin, Instructor")]
        public async Task<IActionResult> FinishClassExam(int classId)
        {
            try
            {
                await _finalExamsService.FinishFinalExamAsync(classId);
                return Ok(new { message = "Class exams finalized." });
            }
            catch (KeyNotFoundException ex) { return NotFound(new { message = ex.Message }); }
            catch (Exception ex) { return StatusCode(500, new { message = ex.Message }); }
        }

        [HttpPut("class/{classId}/weights")]
        [Authorize(Roles = "Admin, Instructor")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateClassWeights(int classId, [FromBody] UpdateClassWeightsDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            try
            {
                await _finalExamsService.UpdateClassExamWeightsAsync(classId, dto);
                return Ok(new { message = "Class exam weights updated successfully." });
            }
            catch (InvalidOperationException ex) { return BadRequest(new { message = ex.Message }); }
            catch (Exception ex) { return StatusCode(500, new { message = ex.Message }); }
        }

        #endregion

        #region Final Exam Templates (New)

        [HttpGet("class/{classId}/templates")]
        [Authorize(Roles = "Admin, Instructor")]
        public async Task<IActionResult> GetExamTemplates(int classId)
        {
            try
            {
                var result = await _templateService.GetTemplatesByClassIdAsync(classId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [HttpPost("class/{classId}/reset")]
        public async Task<IActionResult> ResetClassFinalExam(int classId)
        {
            try
            {
                await _templateService.ResetFinalExamAsync(classId);
                return Ok(new { message = "Class final exams reset successfully (templates created/defaults applied)." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        #endregion

        #region Trainee View

        [HttpGet("my-history")]
        [Authorize(Roles = "Trainee")]
        public async Task<IActionResult> GetMyExams()
        {
            try
            {
                var userId = GetUserIdFromClaims();
                var result = await _finalExamsService.GetFinalExamsByTraineeAsync(userId);
                return Ok(result);
            }
            catch (UnauthorizedAccessException ex) { return Unauthorized(new { message = ex.Message }); }
            catch (Exception ex) { return StatusCode(500, new { message = ex.Message }); }
        }

        [HttpGet("class/{classId}/my-exam")]
        [Authorize(Roles = "Trainee")]
        public async Task<IActionResult> GetMyExamInClass(int classId)
        {
            try
            {
                var userId = GetUserIdFromClaims();
                var result = await _finalExamsService.GetMyFinalExamByClassAsync(classId, userId);
                if (result == null) return NotFound(new { message = "Exam not found for this class." });
                return Ok(result);
            }
            catch (UnauthorizedAccessException ex) { return Unauthorized(new { message = ex.Message }); }
            catch (Exception ex) { return StatusCode(500, new { message = ex.Message }); }
        }

        [HttpGet("class/{classId}/my-final-se-practices")]
        [Authorize(Roles = "Trainee")]
        public async Task<IActionResult> GetMySimulationExamPractices(int classId)
        {
            try
            {
                var userId = GetUserIdFromClaims();
                var result = await _seService.GetMySimulationExamPartialsByClassAsync(classId, userId);
                return Ok(result);
            }
            catch (KeyNotFoundException ex) { return NotFound(new { message = ex.Message }); }
            catch (UnauthorizedAccessException ex) { return Forbid(ex.Message); }
            catch (Exception ex) { return StatusCode(500, new { message = ex.Message }); }
        }

        [HttpPost("partial/{partialId}/validate-se-code")]
        [Authorize(Roles = "Trainee")]
        public async Task<IActionResult> ValidateSeCodeAndStartSe(int partialId, [FromBody] ValidateExamCodeDto dto)
        {
            try
            {
                var userId = GetUserIdFromClaims();
                var result = await _seService.ValidateSeCodeAndStartSimulationExamAsync(partialId, dto.ExamCode, userId);
                return Ok(result);
            }
            catch (KeyNotFoundException ex) { return NotFound(new { message = ex.Message }); }
            catch (UnauthorizedAccessException ex) { return Unauthorized(new { message = ex.Message }); }
            catch (InvalidOperationException ex) { return BadRequest(new { message = ex.Message }); }
            catch (ArgumentException ex) { return BadRequest(new { message = ex.Message }); }
            catch (Exception ex) { return StatusCode(500, new { message = ex.Message }); }
        }

        [HttpGet("partial/{partialId}/my-detail")]
        [Authorize(Roles = "Trainee")]
        public async Task<IActionResult> GetMyPartialDetail(int partialId)
        {
            try
            {
                var userId = GetUserIdFromClaims();
                var result = await _partialService.GetFinalExamPartialByIdForTraineeAsync(partialId, userId);
                return Ok(result);
            }
            catch (KeyNotFoundException ex) { return NotFound(new { message = ex.Message }); }
            catch (UnauthorizedAccessException ex) { return Forbid(ex.Message); }
            catch (Exception ex) { return StatusCode(500, new { message = ex.Message }); }
        }

        [HttpPost("partial/{partialId}/start-te")]
        [Authorize(Roles = "Trainee")]
        public async Task<IActionResult> StartTe(int partialId, [FromBody] GetTeQuizRequestDto request)
        {
            try
            {
                var userId = GetUserIdFromClaims();
                var quizContent = await _partialService.GetTeQuizContentAsync(partialId, request.ExamCode, userId);
                return Ok(quizContent);
            }
            catch (UnauthorizedAccessException ex) { return Unauthorized(new { message = ex.Message }); }
            catch (KeyNotFoundException ex) { return NotFound(new { message = ex.Message }); }
            catch (ArgumentException ex) { return BadRequest(new { message = ex.Message }); }
            catch (Exception ex) { return StatusCode(500, new { message = ex.Message }); }
        }

        [HttpPost("partial/{partialId}/start-se")]
        [Authorize(Roles = "Trainee")]
        public async Task<IActionResult> StartSe(int partialId)
        {
            try
            {
                var userId = GetUserIdFromClaims();
                var result = await _seService.StartSimulationExamAsync(partialId, userId);
                return Ok(result);
            }
            catch (KeyNotFoundException ex) { return NotFound(new { message = ex.Message }); }
            catch (UnauthorizedAccessException ex) { return Unauthorized(new { message = ex.Message }); }
            catch (InvalidOperationException ex) { return BadRequest(new { message = ex.Message }); }
            catch (ArgumentException ex) { return BadRequest(new { message = ex.Message }); }
            catch (Exception ex) { return StatusCode(500, new { message = ex.Message }); }
        }

        [HttpGet("partial/{partialId}/my-pe-submission")]
        [Authorize(Roles = "Trainee")]
        public async Task<IActionResult> GetMyPeSubmission(int partialId)
        {
            try
            {
                var userId = GetUserIdFromClaims();
                var checklist = await _partialService.GetPeSubmissionChecklistForTraineeAsync(partialId, userId);
                return Ok(checklist);
            }
            catch (KeyNotFoundException ex) { return NotFound(new { message = ex.Message }); }
            catch (UnauthorizedAccessException ex) { return Forbid(ex.Message); }
            catch (ArgumentException ex) { return BadRequest(new { message = ex.Message }); }
            catch (Exception ex) { return StatusCode(500, new { message = ex.Message }); }
        }

        #endregion

        #region Partials Config

        [HttpPost("partial")]
        [Authorize(Roles = "Admin, Instructor")]
        public async Task<IActionResult> CreatePartial([FromBody] CreateFinalExamPartialDto dto)
        {
            try
            {
                return Ok(await _partialService.CreateFinalExamPartialAsync(dto));
            }
            catch (KeyNotFoundException ex) { return NotFound(new { message = ex.Message }); }
            catch (ArgumentException ex) { return BadRequest(new { message = ex.Message }); }
            catch (Exception ex) { return StatusCode(500, new { message = ex.Message }); }
        }

        [HttpPost("class-partial")]
        [Authorize(Roles = "Admin, Instructor")]
        public async Task<IActionResult> CreateClassPartial([FromBody] CreateClassPartialDto dto)
        {
            try
            {
                var result = await _partialService.CreatePartialsForClassAsync(dto);
                return Ok(new { message = $"Successfully added {dto.Type} exam to {result.Count()} students.", data = result });
            }
            catch (KeyNotFoundException ex) { return NotFound(new { message = ex.Message }); }
            catch (ArgumentException ex) { return BadRequest(new { message = ex.Message }); }
            catch (Exception ex) { return StatusCode(500, new { message = ex.Message }); }
        }

        [HttpPut("class-partial-config")]
        [Authorize(Roles = "Admin, Instructor")]
        public async Task<IActionResult> UpdateClassPartialConfig([FromBody] UpdateClassPartialConfigDto dto)
        {
            try
            {
                await _partialService.UpdatePartialsConfigForClassAsync(dto);
                return Ok(new { message = "Updated partial configuration and checklist templates for the class." });
            }
            catch (KeyNotFoundException ex) { return NotFound(new { message = ex.Message }); }
            catch (ArgumentException ex) { return BadRequest(new { message = ex.Message }); }
            catch (Exception ex) { return StatusCode(500, new { message = ex.Message }); }
        }

        [HttpPut("partial/{id}")]
        [Authorize(Roles = "Admin, Instructor")]
        public async Task<IActionResult> UpdatePartial(int id, [FromBody] UpdateFinalExamPartialDto dto)
        {
            try
            {
                return Ok(await _partialService.UpdateFinalExamPartialAsync(id, dto));
            }
            catch (KeyNotFoundException ex) { return NotFound(new { message = ex.Message }); }
            catch (ArgumentException ex) { return BadRequest(new { message = ex.Message }); }
            catch (Exception ex) { return StatusCode(500, new { message = ex.Message }); }
        }

        [HttpDelete("partial/{id}")]
        [Authorize(Roles = "Admin, Instructor")]
        public async Task<IActionResult> DeletePartial(int id)
        {
            try
            {
                await _partialService.DeleteFinalExamPartialAsync(id);
                return NoContent();
            }
            catch (KeyNotFoundException ex) { return NotFound(new { message = ex.Message }); }
            catch (Exception ex) { return StatusCode(500, new { message = ex.Message }); }
        }

        [HttpPost("partial/{id}/allow-retake")]
        [Authorize(Roles = "Admin, Instructor")]
        public async Task<IActionResult> AllowRetake(int id, [FromBody] AllowRetakeDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            try
            {
                var result = await _partialService.AllowPartialRetakeAsync(id, dto.Note);
                return Ok(result);
            }
            catch (KeyNotFoundException ex) { return NotFound(new { message = ex.Message }); }
            catch (ArgumentException ex) { return BadRequest(new { message = ex.Message }); }
            catch (Exception ex) { return StatusCode(500, new { message = ex.Message }); }
        }

        #endregion

        #region Submissions

        [HttpPost("submit/se-task/{partialId}")]
        [Authorize(Roles = "Trainee")]
        public async Task<IActionResult> SubmitSeTask(int partialId, [FromBody] SubmitSeTaskDto dto)
        {
            try
            {
                var userId = GetUserIdFromClaims();
                return Ok(await _seService.SubmitSeTaskByCodeAsync(partialId, dto.TaskCode, userId, dto));
            }
            catch (UnauthorizedAccessException ex) { return Unauthorized(new { message = ex.Message }); }
            catch (KeyNotFoundException ex) { return NotFound(new { message = ex.Message }); }
            catch (ArgumentException ex) { return BadRequest(new { message = ex.Message }); }
            catch (Exception ex) { return StatusCode(500, new { message = ex.Message }); }
        }

        [HttpPost("submit/se-final/{partialId}")]
        [Authorize(Roles = "Trainee")]
        public async Task<IActionResult> SubmitSeFinal(int partialId, [FromBody] SubmitSeFinalDto dto)
        {
            try
            {
                var userId = GetUserIdFromClaims();
                return Ok(await _seService.SubmitSeFinalAsync(partialId, userId, dto));
            }
            catch (UnauthorizedAccessException ex) { return Unauthorized(new { message = ex.Message }); }
            catch (KeyNotFoundException ex) { return NotFound(new { message = ex.Message }); }
            catch (ArgumentException ex) { return BadRequest(new { message = ex.Message }); }
            catch (Exception ex) { return StatusCode(500, new { message = ex.Message }); }
        }

        [HttpPost("submit/pe/{partialId}")]
        [Authorize(Roles = "Admin, Instructor")]
        public async Task<IActionResult> SubmitPe(int partialId, [FromBody] SubmitPeDto dto)
        {
            try
            {
                return Ok(await _partialService.SubmitPeAsync(partialId, dto));
            }
            catch (KeyNotFoundException ex) { return NotFound(new { message = ex.Message }); }
            catch (ArgumentException ex) { return BadRequest(new { message = ex.Message }); }
            catch (Exception ex) { return StatusCode(500, new { message = ex.Message }); }
        }

        [HttpPost("submit/te/{partialId}")]
        [Authorize(Roles = "Trainee")]
        public async Task<IActionResult> SubmitTe(int partialId, [FromBody] SubmitTeDto dto)
        {
            try
            {
                var userId = GetUserIdFromClaims();
                var result = await _partialService.SubmitTeAsync(partialId, userId, dto);
                return Ok(result);
            }
            catch (UnauthorizedAccessException) { return Forbid(); }
            catch (KeyNotFoundException ex) { return NotFound(new { message = ex.Message }); }
            catch (ArgumentException ex) { return BadRequest(new { message = ex.Message }); }
            catch (InvalidOperationException ex) { return BadRequest(new { message = ex.Message }); }
            catch (Exception ex) { return StatusCode(500, new { message = "An error occurred.", detail = ex.Message }); }
        }

        [HttpPost("submit/se/{partialId}")]
        public async Task<IActionResult> SubmitSe(int partialId, [FromBody] SubmitSeDto dto)
        {
            try
            {
                return Ok(await _seService.SubmitSeAsync(partialId, dto));
            }
            catch (KeyNotFoundException ex) { return NotFound(new { message = ex.Message }); }
            catch (Exception ex) { return StatusCode(500, new { message = ex.Message }); }
        }

        #endregion

        #region Simulation Exam Tracking

        [HttpGet("simulation-detail/{partialId}")]
        [Authorize]
        [ProducesResponseType(typeof(SimulationExamDetailDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetSimulationDetail(int partialId)
        {
            try
            {
                var result = await _seService.GetSimulationExamDetailAsync(partialId);
                return Ok(result);
            }
            catch (KeyNotFoundException ex) { return NotFound(new { message = ex.Message }); }
            catch (ArgumentException ex) { return BadRequest(new { message = ex.Message }); }
            catch (Exception ex) { return StatusCode(500, new { message = ex.Message }); }
        }

        [HttpGet("class/{classId}/simulation-results")]
        [Authorize(Roles = "Admin, Instructor, SimulationManager")]
        [ProducesResponseType(typeof(IEnumerable<ClassSimulationResultDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetClassSimulationResults(int classId)
        {
            try
            {
                var result = await _seService.GetClassSimulationResultsAsync(classId);
                return Ok(result);
            }
            catch (Exception ex) { return StatusCode(500, new { message = ex.Message }); }
        }

        #endregion
    }
}