using Lssctc.ProgramManagement.ClassManage.Timeslots.Dtos;
using Lssctc.ProgramManagement.ClassManage.Timeslots.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Lssctc.ProgramManagement.ClassManage.Timeslots.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class TimeslotsController : ControllerBase
    {
        private readonly ITimeslotService _timeslotService;

        public TimeslotsController(ITimeslotService timeslotService)
        {
            _timeslotService = timeslotService;
        }

        #region Timeslot

        /// <summary>
        /// API cho Admin/Giảng viên tạo một timeslot mới cho một lớp học.
        /// </summary>
        [HttpPost]
        [Authorize(Roles = "Admin, Instructor")]
        [ProducesResponseType(typeof(TimeslotDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> CreateTimeslot([FromBody] CreateTimeslotDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var creatorId = GetUserIdFromClaims();
                var result = await _timeslotService.CreateTimeslotAsync(dto, creatorId);

                return CreatedAtAction(nameof(GetTimeslotsByClassAndInstructor), new { classId = result.ClassId }, result);
            }
            catch (UnauthorizedAccessException ex) { return Unauthorized(new { message = ex.Message }); }
            catch (KeyNotFoundException ex) { return NotFound(new { message = ex.Message }); }
            catch (ArgumentException ex) { return BadRequest(new { message = ex.Message }); } // Catches duration/time errors (Issue 4)
            catch (InvalidOperationException ex) { return BadRequest(new { message = ex.Message }); }
            catch (Exception) { return StatusCode(500, new { message = "An unexpected error occurred." }); }
        }

        /// <summary>
        /// API cho Admin/Giảng viên tạo nhiều timeslot mới cho một lớp học (Issue 6).
        /// </summary>
        [HttpPost("bulk")]
        [Authorize(Roles = "Admin, Instructor")]
        [ProducesResponseType(typeof(IEnumerable<TimeslotDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> CreateListTimeslot([FromBody] CreateListTimeslotDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var creatorId = GetUserIdFromClaims();
                var result = await _timeslotService.CreateListTimeslotAsync(dto, creatorId);

                return Ok(new { message = $"Successfully created {result.Count()} timeslots.", data = result });
            }
            catch (UnauthorizedAccessException ex) { return Unauthorized(new { message = ex.Message }); }
            catch (KeyNotFoundException ex) { return NotFound(new { message = ex.Message }); }
            catch (ArgumentException ex) { return BadRequest(new { message = ex.Message }); } // Catches duration/time errors (Issue 4)
            catch (InvalidOperationException ex) { return BadRequest(new { message = ex.Message }); }
            catch (Exception) { return StatusCode(500, new { message = "An unexpected error occurred." }); }
        }
        /// <summary>
        /// API cho Admin/Giảng viên cập nhật một timeslot hiện có .
        /// </summary>
        /// <param name="timeslotId">ID của Timeslot</param>
        [HttpPut("{timeslotId}")]
        [Authorize(Roles = "Admin, Instructor")]
        [ProducesResponseType(typeof(TimeslotDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateTimeslot(int timeslotId, [FromBody] UpdateTimeslotDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var updaterId = GetUserIdFromClaims();
                var result = await _timeslotService.UpdateTimeslotAsync(timeslotId, dto, updaterId);

                return Ok(result);
            }
            catch (UnauthorizedAccessException ex) { return Unauthorized(new { message = ex.Message }); }
            catch (KeyNotFoundException ex) { return NotFound(new { message = ex.Message }); }
            catch (ArgumentException ex) { return BadRequest(new { message = ex.Message }); } // Catches duration/time errors (Issue 4)
            catch (InvalidOperationException ex) { return BadRequest(new { message = ex.Message }); }
            catch (Exception ex) { return StatusCode(500, new { message = ex.Message }); }
        }
        /// <summary>
        /// API cho Admin/Giảng viên import timeslot từ file Excel.
        /// </summary>
        [HttpPost("import/class/{classId}")]
        [Authorize(Roles = "Admin, Instructor")]
        [ProducesResponseType(typeof(IEnumerable<ImportTimeslotRecordDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> ImportTimeslots(int classId, IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest(new { message = "No file uploaded." });

            var extension = Path.GetExtension(file.FileName).ToLower();
            if (extension != ".xlsx" && extension != ".xls")
                return BadRequest(new { message = "Invalid file format. Only .xlsx and .xls files are allowed." });

            try
            {
                var creatorId = GetUserIdFromClaims();
                var result = await _timeslotService.ImportTimeslotsAsync(classId, file, creatorId);

                var failedCount = result.Count(r => r.ErrorMessage != null);
                var successCount = result.Count() - failedCount;

                return Ok(new
                {
                    message = $"Import finished. Successfully imported: {successCount}. Failed: {failedCount}. See data for details.",
                    data = result,
                    summary = new { successCount, failedCount }
                });
            }
            catch (UnauthorizedAccessException ex) { return Unauthorized(new { message = ex.Message }); }
            catch (KeyNotFoundException ex) { return NotFound(new { message = ex.Message }); }
            catch (ArgumentException ex) { return BadRequest(new { message = ex.Message }); }
            catch (InvalidOperationException ex) { return BadRequest(new { message = ex.Message }); }
            catch (Exception ex) { return StatusCode(500, new { message = "An unexpected error occurred: " + ex.Message }); }
        }

        /// <summary>
        /// API cho giảng viên xem danh sách slot dạy cho 1 lớp.
        /// </summary>
        /// <param name="classId">ID của lớp</param>
        [HttpGet("class/{classId}/instructor-view")]
        [Authorize(Roles = "Admin, Instructor")]
        [ProducesResponseType(typeof(IEnumerable<TimeslotDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetTimeslotsByClassAndInstructor(int classId)
        {
            try
            {
                var instructorId = GetUserIdFromClaims();
                var result = await _timeslotService.GetTimeslotsByClassAndInstructorAsync(classId, instructorId);
                return Ok(result);
            }
            catch (UnauthorizedAccessException ex) { return Unauthorized(new { message = ex.Message }); }
            catch (Exception) { return StatusCode(500, new { message = "An unexpected error occurred." }); }
        }

        /// <summary>
        /// API cho giảng viên xem danh sách tất cả slot dạy trong mỗi tuần.
        /// </summary>
        /// <param name="dateInWeek">Ngày bất kỳ trong tuần (luôn trả về lịch từ T2 đến CN)</param>
        [HttpGet("instructor-schedule/week")]
        [Authorize(Roles = "Admin, Instructor")]
        [ProducesResponseType(typeof(IEnumerable<TimeslotDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetInstructorTimeslotsForWeek([FromQuery] DateTime dateInWeek)
        {
            try
            {
                var instructorId = GetUserIdFromClaims();
                var result = await _timeslotService.GetTimeslotsByInstructorForWeekAsync(instructorId, dateInWeek);
                return Ok(result);
            }
            catch (UnauthorizedAccessException ex) { return Unauthorized(new { message = ex.Message }); }
            catch (Exception) { return StatusCode(500, new { message = "An unexpected error occurred." }); }
        }


        #endregion

        #region Attendance
        /// <summary>
        /// API cho giảng viên lấy danh sách học viên cần điểm danh cho 1 slot.
        /// </summary>
        /// <param name="timeslotId">ID của Timeslot</param>
        [HttpGet("{timeslotId}/attendance-list")]
        [Authorize(Roles = "Admin, Instructor")]
        [ProducesResponseType(typeof(TimeslotAttendanceDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetAttendanceListForTimeslot(int timeslotId)
        {
            try
            {
                var instructorId = GetUserIdFromClaims();
                var result = await _timeslotService.GetAttendanceListForTimeslotAsync(timeslotId, instructorId);
                return Ok(result);
            }
            catch (UnauthorizedAccessException ex) { return Unauthorized(new { message = ex.Message }); }
            catch (KeyNotFoundException ex) { return NotFound(new { message = ex.Message }); }
            catch (Exception) { return StatusCode(500, new { message = "An unexpected error occurred." }); }
        }

        /// <summary>
        /// API cho giảng viên submit danh sách điểm danh cho 1 slot.
        /// </summary>
        /// <param name="timeslotId">ID của Timeslot</param>
        [HttpPost("{timeslotId}/submit-attendance")]
        [Authorize(Roles = "Admin, Instructor")]
        [ProducesResponseType(typeof(TimeslotAttendanceDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> SubmitAttendanceForTimeslot(int timeslotId, [FromBody] SubmitAttendanceDto dto)
        {
            try
            {
                if (timeslotId != dto.TimeslotId)
                    return BadRequest(new { message = "Timeslot ID mismatch in route and body." });

                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var instructorId = GetUserIdFromClaims();
                var result = await _timeslotService.SubmitAttendanceForTimeslotAsync(timeslotId, instructorId, dto);
                return Ok(result);
            }
            catch (UnauthorizedAccessException ex) { return Unauthorized(new { message = ex.Message }); }
            catch (KeyNotFoundException ex) { return NotFound(new { message = ex.Message }); }
            catch (ArgumentException ex) { return BadRequest(new { message = ex.Message }); }
            catch (Exception ex) { return StatusCode(500, new { message = ex.Message }); }
        }


        // --- Trainee APIs (Auth: Trainee) ---

        /// <summary>
        /// API cho học viên xem danh sách slot cho 1 lớp.
        /// </summary>
        /// <param name="classId">ID của lớp</param>
        [HttpGet("class/{classId}/trainee-view")]
        [Authorize(Roles = "Trainee")]
        [ProducesResponseType(typeof(IEnumerable<TimeslotDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetTimeslotsByClassAndTrainee(int classId)
        {
            try
            {
                var traineeId = GetUserIdFromClaims();
                var result = await _timeslotService.GetTimeslotsByClassAndTraineeAsync(classId, traineeId);
                return Ok(result);
            }
            catch (UnauthorizedAccessException ex) { return Unauthorized(new { message = ex.Message }); }
            catch (Exception) { return StatusCode(500, new { message = "An unexpected error occurred." }); }
        }

        /// <summary>
        /// API cho học viên xem danh sách tất cả slot cho 1 tuần .
        /// </summary>
        /// <param name="dateInWeek">Ngày bất kỳ trong tuần (luôn trả về lịch từ T2 đến CN)</param>
        [HttpGet("trainee-schedule/week")]
        [Authorize(Roles = "Trainee")]
        [ProducesResponseType(typeof(IEnumerable<TimeslotDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetTraineeTimeslotsForWeek([FromQuery] DateTime dateInWeek)
        {
            try
            {
                var traineeId = GetUserIdFromClaims();
                var result = await _timeslotService.GetTimeslotsByTraineeForWeekAsync(traineeId, dateInWeek);
                return Ok(result);
            }
            catch (UnauthorizedAccessException ex) { return Unauthorized(new { message = ex.Message }); }
            catch (Exception) { return StatusCode(500, new { message = "An unexpected error occurred." }); }
        }
        /// <summary>
        /// API cho học viên xem lịch sử điểm danh đầy đủ của mình trong 1 lớp.
        /// </summary>
        /// <param name="classId">ID của lớp</param>
        [HttpGet("class/{classId}/my-attendance")]
        [Authorize(Roles = "Trainee")]
        [ProducesResponseType(typeof(IEnumerable<TraineeAttendanceRecordDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetMyAttendanceHistory(int classId)
        {
            try
            {
                var traineeId = GetUserIdFromClaims();
                var result = await _timeslotService.GetTraineeAttendanceHistoryAsync(classId, traineeId);
                return Ok(result);
            }
            catch (UnauthorizedAccessException ex) { return Unauthorized(new { message = ex.Message }); }
            catch (KeyNotFoundException ex) { return NotFound(new { message = ex.Message }); }
            catch (Exception) { return StatusCode(500, new { message = "An unexpected error occurred." }); }
        }
        #endregion

        #region Helper
        private int GetUserIdFromClaims()
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (int.TryParse(userIdClaim, out int userId))
            {
                return userId;
            }

            throw new UnauthorizedAccessException("User ID claim is missing or invalid.");
        }

        #endregion
    }
}