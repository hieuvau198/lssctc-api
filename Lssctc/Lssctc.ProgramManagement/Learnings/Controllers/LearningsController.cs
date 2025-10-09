using Lssctc.ProgramManagement.Learnings.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Lssctc.ProgramManagement.Learnings.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LearningsController : ControllerBase
    {
        private readonly ILearningsClassService _lcService;
        private readonly ILearningsSectionService _lsService;
        private readonly ILearningsSectionPartitionService _lspService;
        private readonly ILearningsSectionMaterialService _lspmService;

        public LearningsController(
            ILearningsClassService learningsClassService, 
            ILearningsSectionService learningsSectionService, 
            ILearningsSectionPartitionService learningsSectionPartitionService,
            ILearningsSectionMaterialService learningsSectionMaterialService)
            
        {
            _lcService = learningsClassService;
            _lsService = learningsSectionService;
            _lspService = learningsSectionPartitionService;
            _lspmService = learningsSectionMaterialService;
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

            var result = await _lcService.GetAllClassesByTraineeId(traineeId);

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

            var result = await _lcService.GetClassByClassIdAndTraineeId(classId, traineeId);

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

            var result = await _lcService.GetClassesByTraineeIdPaged(traineeId, pageIndex, pageSize);

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

            var result = await _lsService.GetAllSectionsByClassIdAndTraineeId(classId, traineeId);
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

            var result = await _lsService.GetSectionBySectionIdAndTraineeId(sectionId, traineeId);
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

            var result = await _lsService.GetSectionsByClassIdAndTraineeIdPaged(classId, traineeId, pageIndex, pageSize);
            if (result == null || !result.Items.Any())
                return NotFound($"No sections found for class ID {classId} and trainee ID {traineeId} on page {pageIndex}.");

            return Ok(result);
        }
        #endregion

        #region Section Partitions

        /// <summary>
        /// Get all section partitions for a specific section and trainee.
        /// </summary>
        [HttpGet("partitions/section/{sectionId:int}/trainee/{traineeId:int}")]
        public async Task<IActionResult> GetAllSectionPartitionsBySectionIdAndTraineeId(int sectionId, int traineeId)
        {
            if (sectionId <= 0 || traineeId <= 0)
                return BadRequest("Invalid section or trainee ID.");

            var result = await _lspService.GetAllSectionPartitionsBySectionIdAndTraineeId(sectionId, traineeId);
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

            var result = await _lspService.GetSectionPartitionByPartitionIdAndTraineeId(partitionId, traineeId);
            if (result == null)
                return NotFound($"Section partition with ID {partitionId} for trainee {traineeId} not found.");

            return Ok(result);
        }

        /// <summary>
        /// Get paginated section partitions for a section and trainee.
        /// </summary>
        [HttpGet("partitions/section/{sectionId:int}/trainee/{traineeId:int}/paged")]
        public async Task<IActionResult> GetSectionPartitionsBySectionIdAndTraineeIdPaged(
            int sectionId, int traineeId, int pageIndex = 1, int pageSize = 10)
        {
            if (sectionId <= 0 || traineeId <= 0)
                return BadRequest("Invalid section or trainee ID.");
            if (pageIndex <= 0 || pageSize <= 0)
                return BadRequest("Page index and size must be greater than 0.");

            var result = await _lspService.GetSectionPartitionsBySectionIdAndTraineeIdPaged(sectionId, traineeId, pageIndex, pageSize);
            if (result == null || !result.Items.Any())
                return NotFound($"No section partitions found for section ID {sectionId} and trainee ID {traineeId} on page {pageIndex}.");

            return Ok(result);
        }

        #endregion

        #region Section Materials
        /// <summary>
        /// Get details of a specific section material by partition ID and trainee ID.
        /// </summary>
        [HttpGet("sectionmaterials/partition/{partitionId:int}/trainee/{traineeId:int}")]
        public async Task<IActionResult> GetSectionMaterialByPartitionIdAndTraineeId(int partitionId, int traineeId)
        {
            if (partitionId <= 0 || traineeId <= 0)
                return BadRequest("Invalid partition or trainee ID.");

            var result = await _lspmService.GetSectionMaterialByPartitionIdAndTraineeId(partitionId, traineeId);
            if (result == null)
                return NotFound($"Section material with partitionId {partitionId} for traineeId {traineeId} not found.");

            return Ok(result);
        }

        /// <summary>
        /// Mark a learning material as completed for a specific partition and trainee.
        /// </summary>
        [HttpPut("sectionmaterials/partition/{partitionId:int}/trainee/{traineeId:int}/complete")]
        public async Task<IActionResult> UpdateLearningMaterialAsCompleted(int partitionId, int traineeId)
        {
            if (partitionId <= 0 || traineeId <= 0)
                return BadRequest("Invalid partition or trainee ID.");

            var result = await _lspmService.UpdateLearningMaterialAsCompleted(partitionId, traineeId);

            if (!result)
                return StatusCode(StatusCodes.Status500InternalServerError,
                    $"Failed to update learning material as completed for partition ID {partitionId} and trainee ID {traineeId}.");

            return Ok(new { message = "Learning material marked as completed successfully." });
        }

        /// <summary>
        /// Mark a learning material as not completed for a specific partition and trainee.
        /// </summary>
        [HttpPut("sectionmaterials/partition/{partitionId:int}/trainee/{traineeId:int}/incomplete")]
        public async Task<IActionResult> UpdateLearningMaterialAsNotCompleted(int partitionId, int traineeId)
        {
            if (partitionId <= 0 || traineeId <= 0)
                return BadRequest("Invalid partition or trainee ID.");

            var result = await _lspmService.UpdateLearningMaterialAsNotCompleted(partitionId, traineeId);

            if (!result)
                return StatusCode(StatusCodes.Status500InternalServerError,
                    $"Failed to update learning material as not completed for partition ID {partitionId} and trainee ID {traineeId}.");

            return Ok(new { message = "Learning material marked as not completed successfully." });
        }


        #endregion

    }
}
