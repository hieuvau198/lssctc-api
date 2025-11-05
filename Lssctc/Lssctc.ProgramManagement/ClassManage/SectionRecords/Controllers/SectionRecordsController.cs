using Lssctc.ProgramManagement.ClassManage.SectionRecords.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Lssctc.ProgramManagement.ClassManage.SectionRecords.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SectionRecordsController : ControllerBase
    {
        private readonly ISectionRecordsService _sectionRecordsService;

        public SectionRecordsController(ISectionRecordsService sectionRecordsService)
        {
            _sectionRecordsService = sectionRecordsService;
        }

        [HttpGet("my-records/class/{classId}")]
        [Authorize(Roles = "Trainee")]
        public async Task<IActionResult> GetMySectionRecords(int classId)
        {
            try
            {
                var traineeId = GetTraineeIdFromClaims();
                var result = await _sectionRecordsService.GetSectionRecordsAsync(classId, traineeId);
                return Ok(result);
            }
            catch (UnauthorizedAccessException ex) { return Unauthorized(new { message = ex.Message }); }
            catch (Exception) { return StatusCode(500, new { message = "An unexpected error occurred." }); }
        }

        [HttpGet("my-records/class/{classId}/paged")]
        [Authorize(Roles = "Trainee")]
        public async Task<IActionResult> GetMySectionRecordsPaged(int classId, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            try
            {
                var traineeId = GetTraineeIdFromClaims();
                var result = await _sectionRecordsService.GetSectionRecordsPagedAsync(classId, traineeId, pageNumber, pageSize);
                return Ok(result);
            }
            catch (UnauthorizedAccessException ex) { return Unauthorized(new { message = ex.Message }); }
            catch (Exception) { return StatusCode(500, new { message = "An unexpected error occurred." }); }
        }

        [HttpGet("class/{classId}/section/{sectionId}")]
        [Authorize(Roles = "Admin, Instructor")]
        public async Task<IActionResult> GetSectionRecordsBySection(int classId, int sectionId)
        {
            try
            {
                var result = await _sectionRecordsService.GetSectionRecordsBySectionAsync(classId, sectionId);
                return Ok(result);
            }
            catch (Exception) { return StatusCode(500, new { message = "An unexpected error occurred." }); }
        }

        [HttpGet("class/{classId}/section/{sectionId}/paged")]
        [Authorize(Roles = "Admin, Instructor")]
        public async Task<IActionResult> GetSectionRecordsBySectionPaged(int classId, int sectionId, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            try
            {
                var result = await _sectionRecordsService.GetSectionRecordsBySectionPagedAsync(classId, sectionId, pageNumber, pageSize);
                return Ok(result);
            }
            catch (Exception) { return StatusCode(500, new { message = "An unexpected error occurred." }); }
        }

        private int GetTraineeIdFromClaims()
        {
            var traineeIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (int.TryParse(traineeIdClaim, out int traineeId))
            {
                return traineeId;
            }

            throw new UnauthorizedAccessException("Trainee ID claim is missing or invalid.");
        }
    }
}
