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
        private readonly ILearningsSectionService _learningsSectionService;
        private readonly ILearningsSectionPartitionService _learningsSectionPartitionService;

        public LearningsController(
            ILearningsClassService learningsClassService, 
            ILearningsSectionService learningsSectionService, 
            ILearningsSectionPartitionService learningsSectionPartitionService)
        {
            _learningsClassService = learningsClassService;
            _learningsSectionService = learningsSectionService;
            _learningsSectionPartitionService = learningsSectionPartitionService;
        }

        #region Classes

        
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
        /// Get details of a specific class by its ID and trainee ID.
        /// </summary>
        /// <param name="classId">The class ID.</param>
        /// <param name="traineeId">The trainee ID.</param>
        /// <returns>LearningsClassDto</returns>
        [HttpGet("class/{classId:int}/trainee/{traineeId:int}")]
        public async Task<IActionResult> GetClassByClassIdAndTraineeId(int classId, int traineeId)
        {
            if (classId <= 0 || traineeId <= 0)
                return BadRequest("Invalid class or trainee ID.");

            var result = await _learningsClassService.GetClassByClassIdAndTraineeId(classId, traineeId);

            if (result == null)
                return NotFound($"Class with ID {classId} for trainee {traineeId} not found.");

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

        #endregion

        #region Sections
        /// <summary>
        /// Get all sections for a specific class and trainee.
        /// </summary>
        [HttpGet("sections/class/{classId:int}/trainee/{traineeId:int}")]
        public async Task<IActionResult> GetAllSectionsByClassIdAndTraineeId(int classId, int traineeId)
        {
            if (classId <= 0 || traineeId <= 0)
                return BadRequest("Invalid class or trainee ID.");

            var result = await _learningsSectionService.GetAllSectionsByClassIdAndTraineeId(classId, traineeId);
            if (result == null || !result.Any())
                return NotFound($"No sections found for class ID {classId} and trainee ID {traineeId}.");

            return Ok(result);
        }

        /// <summary>
        /// Get a specific section by section ID and trainee ID.
        /// </summary>
        [HttpGet("section/{sectionId:int}/trainee/{traineeId:int}")]
        public async Task<IActionResult> GetSectionBySectionIdAndTraineeId(int sectionId, int traineeId)
        {
            if (sectionId <= 0 || traineeId <= 0)
                return BadRequest("Invalid section or trainee ID.");

            var result = await _learningsSectionService.GetSectionBySectionIdAndTraineeId(sectionId, traineeId);
            if (result == null)
                return NotFound($"Section with ID {sectionId} for trainee {traineeId} not found.");

            return Ok(result);
        }

        /// <summary>
        /// Get paginated sections for a class and trainee.
        /// </summary>
        [HttpGet("sections/class/{classId:int}/trainee/{traineeId:int}/paged")]
        public async Task<IActionResult> GetSectionsByClassIdAndTraineeIdPaged(
            int classId, int traineeId, int pageIndex = 1, int pageSize = 10)
        {
            if (classId <= 0 || traineeId <= 0)
                return BadRequest("Invalid class or trainee ID.");
            if (pageIndex <= 0 || pageSize <= 0)
                return BadRequest("Page index and size must be greater than 0.");

            var result = await _learningsSectionService.GetSectionsByClassIdAndTraineeIdPaged(classId, traineeId, pageIndex, pageSize);
            if (result == null || !result.Items.Any())
                return NotFound($"No sections found for class ID {classId} and trainee ID {traineeId} on page {pageIndex}.");

            return Ok(result);
        }
        #endregion

        #region Section Partitions

        /// <summary>
        /// Get all section partitions for a specific section and trainee.
        /// </summary>
        [HttpGet("section/{sectionId:int}/trainee/{traineeId:int}/partitions")]
        public async Task<IActionResult> GetAllSectionPartitionsBySectionIdAndTraineeId(int sectionId, int traineeId)
        {
            if (sectionId <= 0 || traineeId <= 0)
                return BadRequest("Invalid section or trainee ID.");

            var result = await _learningsSectionPartitionService.GetAllSectionPartitionsBySectionIdAndTraineeId(sectionId, traineeId);
            if (result == null || !result.Any())
                return NotFound($"No section partitions found for section ID {sectionId} and trainee ID {traineeId}.");

            return Ok(result);
        }

        /// <summary>
        /// Get details of a specific section partition by partition ID and trainee ID.
        /// </summary>
        [HttpGet("partition/{partitionId:int}/trainee/{traineeId:int}")]
        public async Task<IActionResult> GetSectionPartitionByPartitionIdAndTraineeId(int partitionId, int traineeId)
        {
            if (partitionId <= 0 || traineeId <= 0)
                return BadRequest("Invalid partition or trainee ID.");

            var result = await _learningsSectionPartitionService.GetSectionPartitionByPartitionIdAndTraineeId(partitionId, traineeId);
            if (result == null)
                return NotFound($"Section partition with ID {partitionId} for trainee {traineeId} not found.");

            return Ok(result);
        }

        /// <summary>
        /// Get paginated section partitions for a section and trainee.
        /// </summary>
        [HttpGet("section/{sectionId:int}/trainee/{traineeId:int}/partitions/paged")]
        public async Task<IActionResult> GetSectionPartitionsBySectionIdAndTraineeIdPaged(
            int sectionId, int traineeId, int pageIndex = 1, int pageSize = 10)
        {
            if (sectionId <= 0 || traineeId <= 0)
                return BadRequest("Invalid section or trainee ID.");
            if (pageIndex <= 0 || pageSize <= 0)
                return BadRequest("Page index and size must be greater than 0.");

            var result = await _learningsSectionPartitionService.GetSectionPartitionsBySectionIdAndTraineeIdPaged(sectionId, traineeId, pageIndex, pageSize);
            if (result == null || !result.Items.Any())
                return NotFound($"No section partitions found for section ID {sectionId} and trainee ID {traineeId} on page {pageIndex}.");

            return Ok(result);
        }

        #endregion

    }
}
