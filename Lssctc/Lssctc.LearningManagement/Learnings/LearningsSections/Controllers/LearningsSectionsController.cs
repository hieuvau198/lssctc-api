using Lssctc.LearningManagement.Learnings.LearningsSections.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Lssctc.LearningManagement.Learnings.LearningsSections.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LearningsSectionsController : ControllerBase
    {
        private readonly ILearningsSectionService _lsService;
        public LearningsSectionsController(ILearningsSectionService learningsSectionService)
        {
            _lsService = learningsSectionService;
        }
        #region Sections
        [HttpGet("sections/class/{classId:int}/trainee/{traineeId:int}")]
        public async Task<IActionResult> GetAllSectionsByClassIdAndTraineeId(int classId, int traineeId)
        {
            if (classId <= 0 || traineeId <= 0)
                return BadRequest("Invalid class or trainee ID.");

            var result = await _lsService.GetAllSectionsByClassIdAndTraineeId(classId, traineeId);
            if (result == null || !result.Any())
                return NotFound($"No sections found for class ID {classId} and trainee ID {traineeId}.");

            return Ok(result);
        }

        [HttpGet("section/{sectionId:int}/trainee/{traineeId:int}")]
        public async Task<IActionResult> GetSectionBySectionIdAndTraineeId(int sectionId, int traineeId)
        {
            if (sectionId <= 0 || traineeId <= 0)
                return BadRequest("Invalid section or trainee ID.");

            var result = await _lsService.GetSectionBySectionIdAndTraineeId(sectionId, traineeId);
            if (result == null)
                return NotFound($"Section with ID {sectionId} for trainee {traineeId} not found.");

            return Ok(result);
        }

        [HttpGet("sections/class/{classId:int}/trainee/{traineeId:int}/paged")]
        public async Task<IActionResult> GetSectionsByClassIdAndTraineeIdPaged(
            int classId, int traineeId, int pageIndex = 1, int pageSize = 10)
        {
            if (classId <= 0 || traineeId <= 0)
                return BadRequest("Invalid class or trainee ID.");
            if (pageIndex <= 0 || pageSize <= 0)
                return BadRequest("Page index and size must be greater than 0.");

            var result = await _lsService.GetSectionsByClassIdAndTraineeIdPaged(classId, traineeId, pageIndex, pageSize);
            if (result == null || !result.Items.Any())
                return NotFound($"No sections found for class ID {classId} and trainee ID {traineeId} on page {pageIndex}.");

            return Ok(result);
        }
        #endregion
    }
}
