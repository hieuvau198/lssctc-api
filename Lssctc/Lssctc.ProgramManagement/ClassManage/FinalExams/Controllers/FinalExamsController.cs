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

        /// <summary>
        /// API cho Admin/Giảng viên tạo thủ công một Final Exam cho một Enrollment.
        /// (Thường được gọi tự động khi lớp học bắt đầu).
        /// </summary>
        [HttpPost]
        [Authorize(Roles = "Admin, Instructor")]
        public async Task<IActionResult> CreateFinalExam([FromBody] CreateFinalExamDto dto)
        {
            var result = await _service.CreateFinalExamAsync(dto);
            return CreatedAtAction(nameof(GetFinalExam), new { id = result.Id }, result);
        }

        /// <summary>
        /// API lấy chi tiết một Final Exam theo ID.
        /// </summary>
        /// <param name="id">ID của Final Exam</param>
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

        /// <summary>
        /// API cho Admin/Giảng viên xem danh sách Final Exam của một lớp học.
        /// </summary>
        /// <param name="classId">ID của lớp học</param>
        [HttpGet("class/{classId}")]
        [Authorize(Roles = "Admin, Instructor")]
        public async Task<IActionResult> GetByClass(int classId)
        {
            var result = await _service.GetFinalExamsByClassAsync(classId);
            return Ok(result);
        }

        /// <summary>
        /// API cho Admin/Giảng viên xóa một Final Exam.
        /// </summary>
        /// <param name="id">ID của Final Exam</param>
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

        /// <summary>
        /// API cho Học viên xem lịch sử Final Exam của chính mình.
        /// </summary>
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

        /// <summary>
        /// API tạo một phần thi (Partial) cho Final Exam (Theory, Simulation, Practical).
        /// </summary>
        [HttpPost("partial")]
        [Authorize(Roles = "Admin, Instructor")]
        public async Task<IActionResult> CreatePartial([FromBody] CreateFinalExamPartialDto dto)
        {
            var result = await _service.CreateFinalExamPartialAsync(dto);
            return Ok(result);
        }
        /// <summary>
        /// API cấu hình đề thi cho TOÀN BỘ lớp học (Tạo partial cho tất cả học viên trong lớp).
        /// </summary>
        [HttpPost("class-partial")]
        [Authorize(Roles = "Admin, Instructor")]
        public async Task<IActionResult> CreateClassPartial([FromBody] CreateClassPartialDto dto)
        {
            try
            {
                var result = await _service.CreatePartialsForClassAsync(dto);
                return Ok(new { message = $"Successfully added {dto.Type} exam to {result.Count()} students.", data = result });
            }
            catch (KeyNotFoundException ex) { return NotFound(new { message = ex.Message }); }
            catch (Exception ex) { return BadRequest(new { message = ex.Message }); }
        }
        /// <summary>
        /// API cập nhật thông tin một phần thi (Trọng số, thời gian, liên kết bài quiz/practice).
        /// </summary>
        /// <param name="id">ID của Partial Exam</param>
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

        /// <summary>
        /// API xóa một phần thi khỏi Final Exam.
        /// </summary>
        /// <param name="id">ID của Partial Exam</param>
        [HttpDelete("partial/{id}")]
        [Authorize(Roles = "Admin, Instructor")]
        public async Task<IActionResult> DeletePartial(int id)
        {
            await _service.DeleteFinalExamPartialAsync(id);
            return NoContent();
        }
        #endregion

        #region Submissions

        /// <summary>
        /// API lấy danh sách tiêu chí chấm điểm cho bài thi Thực hành (PE).
        /// </summary>
        [HttpGet("pe-checklist-criteria")]
        public IActionResult GetPeChecklist()
        {
            return Ok(_service.GetPeChecklistCriteria());
        }

        /// <summary>
        /// API chấm điểm bài thi Thực hành (PE) theo Checklist. (Dành cho Giảng viên).
        /// </summary>
        /// <param name="partialId">ID của phần thi Practical</param>
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

        /// <summary>
        /// API submit điểm bài thi Lý thuyết (TE).
        /// </summary>
        /// <param name="partialId">ID của phần thi Theory</param>
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

        /// <summary>
        /// API submit điểm bài thi Mô phỏng (SE).
        /// </summary>
        /// <param name="partialId">ID của phần thi Simulation</param>
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