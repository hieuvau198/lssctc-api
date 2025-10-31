using Lssctc.LearningManagement.Learnings.LearningsClasses.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Lssctc.LearningManagement.Learnings.LearningsClasses.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LearningsClassesController : ControllerBase
    {
        private readonly ILearningsClassService _lcService;
        public LearningsClassesController(ILearningsClassService learningsClassService)
        {
            _lcService = learningsClassService;
        }

        #region Classes

        [HttpGet("classes/trainee/{traineeId:int}")]
        public async Task<IActionResult> GetAllClassesByTraineeId(int traineeId)
        {
            if (traineeId <= 0)
                return BadRequest("Invalid trainee ID.");

            var result = await _lcService.GetAllClassesByTraineeId(traineeId);

            if (result == null || !result.Any())
                return NotFound($"No classes found for trainee with ID {traineeId}.");

            return Ok(result);
        }

        [HttpGet("class/{classId:int}/trainee/{traineeId:int}")]
        public async Task<IActionResult> GetClassByClassIdAndTraineeId(int classId, int traineeId)
        {
            if (classId <= 0 || traineeId <= 0)
                return BadRequest("Invalid class or trainee ID.");

            var result = await _lcService.GetClassByClassIdAndTraineeId(classId, traineeId);

            if (result == null)
                return NotFound($"Class with ID {classId} for trainee {traineeId} not found.");

            return Ok(result);
        }

        [HttpGet("classes/trainee/{traineeId:int}/paged")]
        public async Task<IActionResult> GetClassesByTraineeIdPaged(int traineeId, int pageIndex = 1, int pageSize = 10)
        {
            if (traineeId <= 0)
                return BadRequest("Invalid trainee ID.");

            if (pageIndex <= 0 || pageSize <= 0)
                return BadRequest("Page index and size must be greater than 0.");

            var result = await _lcService.GetClassesByTraineeIdPaged(traineeId, pageIndex, pageSize);

            if (result == null || !result.Items.Any())
                return NotFound($"No classes found for trainee with ID {traineeId} on page {pageIndex}.");

            return Ok(result);
        }

        #endregion
    }
}
