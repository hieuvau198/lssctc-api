using Lssctc.ProgramManagement.Learnings.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Lssctc.ProgramManagement.Learnings.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LearningsController : ControllerBase
    {
        private readonly ILearningsClassService _learningsClassService;

        public LearningsController(ILearningsClassService learningsClassService)
        {
            _learningsClassService = learningsClassService;
        }

        /// <summary>
        /// Get all classes for a specific trainee.
        /// </summary>
        /// <param name="traineeId">The trainee ID.</param>
        /// <returns>List of LearningsClassDto</returns>
        [HttpGet("classes/trainee/{traineeId:int}")]
        public async Task<IActionResult> GetAllClassesByTraineeId(int traineeId)
        {
            if (traineeId <= 0)
                return BadRequest("Invalid trainee ID.");

            var result = await _learningsClassService.GetAllClassesByTraineeId(traineeId);

            if (result == null || !result.Any())
                return NotFound($"No classes found for trainee with ID {traineeId}.");

            return Ok(result);
        }

        /// <summary>
        /// Get details of a specific class by its ID.
        /// </summary>
        /// <param name="classId">The class ID.</param>
        /// <returns>LearningsClassDto</returns>
        [HttpGet("class/{classId:int}")]
        public async Task<IActionResult> GetClassById(int classId)
        {
            if (classId <= 0)
                return BadRequest("Invalid class ID.");

            var result = await _learningsClassService.GetClassById(classId);

            if (result == null)
                return NotFound($"Class with ID {classId} not found.");

            return Ok(result);
        }

        /// <summary>
        /// Get paginated classes for a specific trainee.
        /// </summary>
        /// <param name="traineeId">The trainee ID.</param>
        /// <param name="pageIndex">Page index (starting from 1).</param>
        /// <param name="pageSize">Number of items per page.</param>
        /// <returns>PagedResult of LearningsClassDto</returns>
        [HttpGet("classes/trainee/{traineeId:int}/paged")]
        public async Task<IActionResult> GetClassesByTraineeIdPaged(int traineeId, int pageIndex = 1, int pageSize = 10)
        {
            if (traineeId <= 0)
                return BadRequest("Invalid trainee ID.");

            if (pageIndex <= 0 || pageSize <= 0)
                return BadRequest("Page index and size must be greater than 0.");

            var result = await _learningsClassService.GetClassesByTraineeIdPaged(traineeId, pageIndex, pageSize);

            if (result == null || !result.Items.Any())
                return NotFound($"No classes found for trainee with ID {traineeId} on page {pageIndex}.");

            return Ok(result);
        }
    }
}
