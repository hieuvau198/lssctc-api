using Lssctc.ProgramManagement.Learnings.Dtos;
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
        private readonly ILearningsSectionQuizService _lsqService;

        public LearningsController(
            ILearningsClassService learningsClassService, 
            ILearningsSectionService learningsSectionService,
            ILearningsSectionPartitionService learningsSectionPartitionService,
            ILearningsSectionMaterialService learningsSectionMaterialService,
            ILearningsSectionQuizService lsqService)

        {
            _lcService = learningsClassService;
            _lsService = learningsSectionService;
            _lspService = learningsSectionPartitionService;
            _lspmService = learningsSectionMaterialService;
            _lsqService = lsqService;
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

        #region Section Partitions

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

        #region Section Quizzes
        [HttpGet("sectionquizzes/partition/{partitionId:int}/trainee/{traineeId:int}")]
        public async Task<IActionResult> GetSectionQuizByPartitionIdAndTraineeId(int partitionId, int traineeId)
        {
            if (partitionId <= 0 || traineeId <= 0)
                return BadRequest("Invalid partition or trainee ID.");
            var result = await _lsqService.GetSectionQuizByPartitionIdAndTraineeId(partitionId, traineeId);
            if (result == null)
                return NotFound($"Section quiz with partitionId {partitionId} for traineeId {traineeId} not found.");
            return Ok(result);
        }

        [HttpPost("sectionquizzes/{partitionId:int}/trainee/{traineeId:int}/submit")]
        public async Task<IActionResult> SubmitSectionQuizAttempt(
            int partitionId,
            int traineeId,
            [FromBody] CreateLearningsSectionQuizAttemptDto attempt)
        {
            if (partitionId <= 0 || traineeId <= 0)
                return BadRequest("Invalid partitionId or traineeId.");

            if (attempt == null)
                return BadRequest("Attempt data cannot be null.");

            try
            {
                var result = await _lsqService.SubmitSectionQuizAttempt(partitionId, traineeId, attempt);
                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                // Optional: log the exception (e.g., using ILogger)
                return StatusCode(500, $"An unexpected error occurred: {ex.Message}");
            }
        }

        #endregion
    }
}
