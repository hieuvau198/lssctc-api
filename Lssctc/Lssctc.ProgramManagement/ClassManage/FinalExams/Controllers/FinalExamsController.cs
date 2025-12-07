using Lssctc.ProgramManagement.ClassManage.FinalExams.Dtos;
using Lssctc.ProgramManagement.ClassManage.FinalExams.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Lssctc.ProgramManagement.ClassManage.FinalExams.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class FinalExamsController : ControllerBase
    {
        private readonly IFinalExamsService _service;

        public FinalExamsController(IFinalExamsService service)
        {
            _service = service;
        }

        #region FinalExam (Admin/Instructor)

        [HttpPost]
        [Authorize(Roles = "Admin, Instructor")]
        public async Task<IActionResult> CreateFinalExam([FromBody] CreateFinalExamDto dto)
        {
            var result = await _service.CreateFinalExamAsync(dto);
            return CreatedAtAction(nameof(GetFinalExam), new { id = result.Id }, result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetFinalExam(int id)
        {
            try
            {
                var result = await _service.GetFinalExamByIdAsync(id);
                return Ok(result);
            }
            catch (KeyNotFoundException ex) { return NotFound(new { message = ex.Message }); }
        }

        [HttpGet("class/{classId}")]
        [Authorize(Roles = "Admin, Instructor")]
        public async Task<IActionResult> GetByClass(int classId)
        {
            var result = await _service.GetFinalExamsByClassAsync(classId);
            return Ok(result);
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin, Instructor")]
        public async Task<IActionResult> DeleteFinalExam(int id)
        {
            try
            {
                await _service.DeleteFinalExamAsync(id);
                return NoContent();
            }
            catch (KeyNotFoundException ex) { return NotFound(new { message = ex.Message }); }
        }
        #endregion

        #region Trainee View
        [HttpGet("my-history")]
        [Authorize(Roles = "Trainee")]
        public async Task<IActionResult> GetMyExams()
        {
            // Requires helper to get userId from claim
            var userIdStr = User.FindFirst("id")?.Value; // Adjust based on your ClaimTypes
            if (int.TryParse(userIdStr, out int userId))
            {
                var result = await _service.GetFinalExamsByTraineeAsync(userId);
                return Ok(result);
            }
            return Unauthorized();
        }
        #endregion

        #region Partials Config
        [HttpPost("partial")]
        [Authorize(Roles = "Admin, Instructor")]
        public async Task<IActionResult> CreatePartial([FromBody] CreateFinalExamPartialDto dto)
        {
            var result = await _service.CreateFinalExamPartialAsync(dto);
            return Ok(result);
        }

        [HttpPut("partial/{id}")]
        [Authorize(Roles = "Admin, Instructor")]
        public async Task<IActionResult> UpdatePartial(int id, [FromBody] UpdateFinalExamPartialDto dto)
        {
            try
            {
                var result = await _service.UpdateFinalExamPartialAsync(id, dto);
                return Ok(result);
            }
            catch (KeyNotFoundException ex) { return NotFound(new { message = ex.Message }); }
        }

        [HttpDelete("partial/{id}")]
        [Authorize(Roles = "Admin, Instructor")]
        public async Task<IActionResult> DeletePartial(int id)
        {
            await _service.DeleteFinalExamPartialAsync(id);
            return NoContent();
        }
        #endregion

        #region Submissions

        // Get Checklist Criteria for UI generation
        [HttpGet("pe-checklist-criteria")]
        public IActionResult GetPeChecklist()
        {
            return Ok(_service.GetPeChecklistCriteria());
        }

        // Submit Practical Exam (PE)
        [HttpPost("submit/pe/{partialId}")]
        [Authorize(Roles = "Admin, Instructor")] // Only instructor grades PE
        public async Task<IActionResult> SubmitPe(int partialId, [FromBody] SubmitPeDto dto)
        {
            try
            {
                var result = await _service.SubmitPeAsync(partialId, dto);
                return Ok(result);
            }
            catch (Exception ex) { return BadRequest(new { message = ex.Message }); }
        }

        // Submit Theory Exam (TE)
        [HttpPost("submit/te/{partialId}")]
        public async Task<IActionResult> SubmitTe(int partialId, [FromBody] SubmitTeDto dto)
        {
            try
            {
                var result = await _service.SubmitTeAsync(partialId, dto);
                return Ok(result);
            }
            catch (Exception ex) { return BadRequest(new { message = ex.Message }); }
        }

        // Submit Simulation Exam (SE)
        [HttpPost("submit/se/{partialId}")]
        public async Task<IActionResult> SubmitSe(int partialId, [FromBody] SubmitSeDto dto)
        {
            try
            {
                var result = await _service.SubmitSeAsync(partialId, dto);
                return Ok(result);
            }
            catch (Exception ex) { return BadRequest(new { message = ex.Message }); }
        }
        #endregion
    }
}