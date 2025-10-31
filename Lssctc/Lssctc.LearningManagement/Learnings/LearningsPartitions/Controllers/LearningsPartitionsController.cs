using Lssctc.LearningManagement.Learnings.LearningsPartitions.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Lssctc.LearningManagement.Learnings.LearningsPartitions.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LearningsPartitionsController : ControllerBase
    {
        private readonly ILearningsSectionPartitionService _lspService;
        public LearningsPartitionsController(ILearningsSectionPartitionService learningsSectionPartitionService)
        {
            _lspService = learningsSectionPartitionService;
        }
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

    }
}
