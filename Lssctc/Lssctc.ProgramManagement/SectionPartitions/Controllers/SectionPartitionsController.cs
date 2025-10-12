using Lssctc.ProgramManagement.SectionPartitions.DTOs;
using Lssctc.ProgramManagement.SectionPartitions.Services;
using Lssctc.Share.Common;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace Lssctc.ProgramManagement.SectionPartitions.Controllers
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

        [HttpGet("all")]
        public async Task<IActionResult> GetSectionPartitionsNoPagination()
        {
            var items = await _svc.GetSectionPartitionsNoPagination();

            return Ok(items);
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
        /// <summary>
        /// get section partitions by sectionId with pagination
        /// </summary>

        // GET /api/section-partitions/by-section/5/paged?page=1&pageSize=10
        [HttpGet("by-section/{sectionId:int}/paged")]
        public async Task<IActionResult> GetBySectionPaged(int sectionId, int page = 1, int pageSize = 20)
        {
            try
            {
                var result = await _svc.GetSectionPartitionBySectionId(sectionId, page, pageSize);
                return Ok(result);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { error = ex.Message });
            }
            catch (ValidationException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
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
