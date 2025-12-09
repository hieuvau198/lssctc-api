using Lssctc.ProgramManagement.ClassManage.FinalExams.Dtos;
using Lssctc.ProgramManagement.ClassManage.FinalExams.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims; // Cần thiết để lấy thông tin user từ Claim

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

        // Phương thức mới để lấy UserId từ Claims (ClaimTypes.NameIdentifier)
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
            try { return Ok(await _service.GetFinalExamByIdAsync(id)); }
            catch (KeyNotFoundException ex) { return NotFound(new { message = ex.Message }); }
        }

        /// <summary>
        /// API cho Admin/Giảng viên xem danh sách Final Exam của một lớp học.
        /// </summary>
        /// <param name="classId">ID của lớp học</param>
        [HttpGet("class/{classId}")]
        [Authorize(Roles = "Admin, Instructor")]
        public async Task<IActionResult> GetByClassForInstructor(int classId)
        {
            var result = await _service.GetFinalExamsByClassAsync(classId);
            return Ok(result);
        }

        /// <summary>
        /// API cho Admin/Giảng viên cập nhật thông tin một Final Exam (Pass/Fail, TotalMarks).
        /// </summary>
        /// <param name="id">ID của Final Exam</param>
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin, Instructor")]
        public async Task<IActionResult> UpdateFinalExam(int id, [FromBody] UpdateFinalExamDto dto)
        {
            try { return Ok(await _service.UpdateFinalExamAsync(id, dto)); }
            catch (KeyNotFoundException ex) { return NotFound(new { message = ex.Message }); }
        }

        [HttpGet("class/{classId}/config")]
        [Authorize]
        public async Task<IActionResult> GetClassExamConfig(int classId)
        {
            var result = await _service.GetClassExamConfigAsync(classId);
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
            await _service.DeleteFinalExamAsync(id);
            return NoContent();
        }

        /// <summary>
        /// API tạo một Exam Code ngẫu nhiên cho một Final Exam (Dùng cho TE).
        /// </summary>
        [HttpPost("{id}/generate-code")]
        [Authorize(Roles = "Admin, Instructor")]
        public async Task<IActionResult> GenerateCode(int id)
        {
            try
            {
                var code = await _service.GenerateExamCodeAsync(id);
                return Ok(new { examCode = code });
            }
            catch (KeyNotFoundException ex) { return NotFound(new { message = ex.Message }); }
        }

        /// <summary>
        /// API dành cho Giảng viên confirm một Final Exam đã kết thúc cho toàn bộ lớp học.
        /// </summary>
        [HttpPost("class/{classId}/finish")]
        [Authorize(Roles = "Admin, Instructor")]
        public async Task<IActionResult> FinishClassExam(int classId)
        {
            await _service.FinishFinalExamAsync(classId);
            return Ok(new { message = "Class exams finalized." });
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
            try
            {
                var userId = GetUserIdFromClaims(); // Sử dụng phương thức mới
                var result = await _service.GetFinalExamsByTraineeAsync(userId);
                return Ok(result);
            }
            catch (UnauthorizedAccessException)
            {
                return Unauthorized();
            }
        }

        /// <summary>
        /// API cho Học viên xem Final Exam của chính mình trong lớp học.
        /// </summary>
        /// <param name="classId">ID của lớp học</param>
        [HttpGet("class/{classId}/my-exam")]
        [Authorize(Roles = "Trainee")]
        public async Task<IActionResult> GetMyExamInClass(int classId)
        {
            try
            {
                var userId = GetUserIdFromClaims(); // Sử dụng phương thức mới
                var result = await _service.GetMyFinalExamByClassAsync(classId, userId);
                if (result == null) return NotFound(new { message = "Exam not found for this class." });
                return Ok(result);
            }
            catch (UnauthorizedAccessException)
            {
                return Unauthorized();
            }
        }

        /// <summary>
        /// API cho Học viên xem chi tiết cấu hình một phần thi (TE/SE/PE) của chính mình.
        /// </summary>
        /// <param name="partialId">ID của phần thi</param>
        [HttpGet("partial/{partialId}/my-detail")]
        [Authorize(Roles = "Trainee")]
        public async Task<IActionResult> GetMyPartialDetail(int partialId)
        {
            int userId;
            try
            {
                userId = GetUserIdFromClaims(); // Sử dụng phương thức mới
            }
            catch (UnauthorizedAccessException)
            {
                return Unauthorized();
            }

            try
            {
                var result = await _service.GetFinalExamPartialByIdForTraineeAsync(partialId, userId);
                return Ok(result);
            }
            catch (KeyNotFoundException ex) { return NotFound(new { message = ex.Message }); }
            catch (UnauthorizedAccessException) { return Forbid(); }
        }

        /// <summary>
        /// API cho Học viên bắt đầu bài thi lý thuyết (TE) bằng Exam Code, ghi lại thời gian bắt đầu và trả về nội dung Quiz.
        /// </summary>
        /// <param name="partialId">ID của phần thi Theory</param>
        [HttpPost("partial/{partialId}/start-te")]
        [Authorize(Roles = "Trainee")]
        public async Task<IActionResult> StartTe(int partialId, [FromBody] GetTeQuizRequestDto request)
        {
            int userId;
            try
            {
                userId = GetUserIdFromClaims(); // Sử dụng phương thức mới
            }
            catch (UnauthorizedAccessException)
            {
                return Unauthorized();
            }

            try
            {
                // Truyền userId vào service để kiểm tra bảo mật
                var quizContent = await _service.GetTeQuizContentAsync(partialId, request.ExamCode, userId);
                return Ok(quizContent);
            }
            catch (UnauthorizedAccessException) { return Unauthorized(new { message = "Invalid Exam Code or Exam does not belong to user." }); }
            catch (KeyNotFoundException ex) { return NotFound(new { message = ex.Message }); }
            catch (ArgumentException ex) { return BadRequest(new { message = ex.Message }); }
        }

        /// <summary>
        /// API cho Học viên bắt đầu bài thi mô phỏng (SE), ghi lại thời gian bắt đầu.
        /// </summary>
        /// <param name="partialId">ID của phần thi Simulation</param>
        [HttpPost("partial/{partialId}/start-se")]
        [Authorize(Roles = "Trainee")]
        public async Task<IActionResult> StartSe(int partialId)
        {
            int userId;
            try
            {
                userId = GetUserIdFromClaims(); // Sử dụng phương thức mới
            }
            catch (UnauthorizedAccessException)
            {
                return Unauthorized();
            }

            try
            {
                var result = await _service.StartSimulationExamAsync(partialId, userId);
                return Ok(result);
            }
            catch (Exception ex) { return BadRequest(new { message = ex.Message }); }
        }

        /// <summary>
        /// API cho Học viên xem chi tiết kết quả chấm điểm (Checklist) của bài thi Thực hành (PE) đã được Giảng viên chấm.
        /// </summary>
        /// <param name="partialId">ID của phần thi Practical</param>
        [HttpGet("partial/{partialId}/my-pe-submission")]
        [Authorize(Roles = "Trainee")]
        public async Task<IActionResult> GetMyPeSubmission(int partialId)
        {
            int userId;
            try
            {
                userId = GetUserIdFromClaims(); // Sử dụng phương thức mới
            }
            catch (UnauthorizedAccessException)
            {
                return Unauthorized();
            }

            try
            {
                var checklist = await _service.GetPeSubmissionChecklistForTraineeAsync(partialId, userId);
                return Ok(checklist);
            }
            catch (KeyNotFoundException ex) { return NotFound(new { message = ex.Message }); }
            catch (UnauthorizedAccessException) { return Forbid(); }
            catch (InvalidOperationException ex) { return BadRequest(new { message = ex.Message }); }
        }
        #endregion


        #region Partials Config

        /// <summary>
        /// API tạo một phần thi (Partial) cho Final Exam.
        /// </summary>
        [HttpPost("partial")]
        [Authorize(Roles = "Admin, Instructor")]
        public async Task<IActionResult> CreatePartial([FromBody] CreateFinalExamPartialDto dto)
        {
            return Ok(await _service.CreateFinalExamPartialAsync(dto));
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
            catch (Exception ex) { return BadRequest(new { message = ex.Message }); }
        }

        /// <summary>
        /// API cập nhật cấu hình (trọng số, thời gian, checklist PE) cho TẤT CẢ các partial cùng loại trong một lớp.
        /// </summary>
        [HttpPut("class-partial-config")]
        [Authorize(Roles = "Admin, Instructor")]
        public async Task<IActionResult> UpdateClassPartialConfig([FromBody] UpdateClassPartialConfigDto dto)
        {
            await _service.UpdatePartialsConfigForClassAsync(dto);
            return Ok(new { message = "Updated partial configuration for the class." });
        }

        /// <summary>
        /// API cập nhật thông tin một phần thi.
        /// </summary>
        /// <param name="id">ID của Partial Exam</param>
        [HttpPut("partial/{id}")]
        [Authorize(Roles = "Admin, Instructor")]
        public async Task<IActionResult> UpdatePartial(int id, [FromBody] UpdateFinalExamPartialDto dto)
        {
            try { return Ok(await _service.UpdateFinalExamPartialAsync(id, dto)); }
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
        /// API chấm điểm bài thi Thực hành (PE) theo Checklist. (Dành cho Giảng viên).
        /// </summary>
        /// <param name="partialId">ID của phần thi Practical</param>
        [HttpPost("submit/pe/{partialId}")]
        [Authorize(Roles = "Admin, Instructor")]
        public async Task<IActionResult> SubmitPe(int partialId, [FromBody] SubmitPeDto dto)
        {
            try { return Ok(await _service.SubmitPeAsync(partialId, dto)); }
            catch (Exception ex) { return BadRequest(new { message = ex.Message }); }
        }

        /// <summary>
        /// API submit bài làm Lý thuyết (TE) của Học viên.
        /// </summary>
        /// <param name="partialId">ID của phần thi Theory</param>
        [HttpPost("submit/te/{partialId}")]
        [Authorize(Roles = "Trainee")]
        public async Task<IActionResult> SubmitTe(int partialId, [FromBody] SubmitTeDto dto)
        {
            int userId;
            try
            {
                userId = GetUserIdFromClaims(); // Sử dụng phương thức mới
            }
            catch (UnauthorizedAccessException)
            {
                return Unauthorized();
            }

            try { return Ok(await _service.SubmitTeAsync(partialId, userId, dto)); }
            catch (Exception ex) { return BadRequest(new { message = ex.Message }); }
        }

        /// <summary>
        /// API submit điểm bài thi Mô phỏng (SE).
        /// </summary>
        /// <param name="partialId">ID của phần thi Simulation</param>
        [HttpPost("submit/se/{partialId}")]
        public async Task<IActionResult> SubmitSe(int partialId, [FromBody] SubmitSeDto dto)
        {
            try { return Ok(await _service.SubmitSeAsync(partialId, dto)); }
            catch (Exception ex) { return BadRequest(new { message = ex.Message }); }
        }
        #endregion
    }
}