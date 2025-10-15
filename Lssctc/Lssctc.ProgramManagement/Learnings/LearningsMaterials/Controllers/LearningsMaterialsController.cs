using Lssctc.ProgramManagement.Learnings.LearningsMaterials.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Lssctc.ProgramManagement.Learnings.LearningsMaterials.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LearningsMaterialsController : ControllerBase
    {
        private readonly ILearningsSectionMaterialService _lspmService;
        public LearningsMaterialsController(ILearningsSectionMaterialService learningsSectionMaterialService)
        {
            _lspmService = learningsSectionMaterialService;
        }
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

    }
}
