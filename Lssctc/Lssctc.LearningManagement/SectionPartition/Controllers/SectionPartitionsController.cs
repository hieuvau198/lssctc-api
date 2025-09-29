using Lssctc.LearningManagement.SectionPartition.DTOs;
using Lssctc.LearningManagement.SectionPartition.Services;
using Lssctc.Share.Common;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace Lssctc.LearningManagement.SectionPartition.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SectionPartitionsController : ControllerBase
    {
        private readonly ISectionPartitionService _svc;

        public SectionPartitionsController(ISectionPartitionService svc)
        {
            _svc = svc;
        }

        // GET: /api/SectionPartitions?pageIndex=1&pageSize=20
        [HttpGet]
        public async Task<IActionResult> GetSectionPartitionsPaged(
            [FromQuery] int pageIndex = 1,
            [FromQuery] int pageSize = 20)
        {
            var page = await _svc.GetSectionPartitionsPaged(pageIndex, pageSize);

            // Cho phép FE đọc total qua header
            Response.Headers["X-Total-Count"] = page.TotalCount.ToString();
            Response.Headers["Access-Control-Expose-Headers"] = "X-Total-Count";

            return Ok(page); // Trả thẳng PagedResult<SectionPartitionDto>
        }

        // GET: /api/SectionPartitions/5
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById([FromRoute] int id)
        {
            var dto = await _svc.GetSectionPartitionById(id);
            if (dto is null)
                return NotFound(new { message = $"SectionPartition {id} not found." });

            return Ok(dto);
        }

        // POST: /api/SectionPartitions
        [HttpPost]
        public async Task<IActionResult> CreateSectionPartition([FromBody] CreateSectionPartitionDto dto)
        {
            try
            {
                var id = await _svc.CreateSectionPartition(dto);
                return CreatedAtAction(nameof(GetById), new { id }, new { id });
            }
            catch (ValidationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { message = ex.Message });
            }
        }

        // PUT: /api/SectionPartitions/5
        [HttpPut("{id:int}")]
        public async Task<IActionResult> UpdateSectionPartition([FromRoute] int id, [FromBody] UpdateSectionPartitionDto dto)
        {
            try
            {
                var ok = await _svc.UpdateSectionPartition(id, dto);
                if (!ok)
                    return NotFound(new { message = $"SectionPartition {id} not found." });

                return Ok(new { message = "Update section partition successfully." });
            }
            catch (ValidationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { message = ex.Message });
            }
        }

        // DELETE: /api/SectionPartitions/5
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteSectionPartition([FromRoute] int id)
        {
            try
            {
                var ok = await _svc.DeleteSectionPartition(id);
                if (!ok)
                    return NotFound(new { message = $"SectionPartition {id} not found." });

                return Ok(new { message = "Delete section partition successfully." });
            }
            catch (ValidationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { message = ex.Message });
            }
        }
    }
}
