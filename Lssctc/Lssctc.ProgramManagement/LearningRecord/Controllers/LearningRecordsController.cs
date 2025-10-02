using Lssctc.ProgramManagement.LearningRecord.DTOs;
using Lssctc.ProgramManagement.LearningRecord.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace Lssctc.ProgramManagement.LearningRecord.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LearningRecordsController : ControllerBase
    {
        private readonly ILearningRecordService _svc;

        public LearningRecordsController(ILearningRecordService svc)
        {
            _svc = svc;
        }

        // GET: /api/LearningRecords?pageIndex=1&pageSize=20
        [HttpGet]
        public async Task<IActionResult> GetPaged([FromQuery] int pageIndex = 1, [FromQuery] int pageSize = 20)
        {
            var result = await _svc.GetLearningRecords(pageIndex, pageSize);

            // để FE dễ đọc tổng số bản ghi qua header (giống QuizzesController)
            Response.Headers["X-Total-Count"] = result.TotalCount.ToString();
            Response.Headers["Access-Control-Expose-Headers"] = "X-Total-Count";

            return Ok(result);
        }

        // GET: /api/LearningRecords/all  (không phân trang, không filter)
        [HttpGet("all")]
        public async Task<IActionResult> GetAll()
        {
            var items = await _svc.GetLearningRecordsNoPagination();
            return Ok(items);
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById([FromRoute] int id)
        {
            var all = await _svc.GetLearningRecordsNoPagination();
            var item = all.FirstOrDefault(x => x.Id == id);
            return item is null ? NotFound(new { error = $"LearningRecord {id} not found." }) : Ok(item);
        }

        // POST: /api/LearningRecords
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateLearningRecordDto dto)
        {
            try
            {
                var id = await _svc.CreateLearningRecord(dto);
                return CreatedAtAction(nameof(GetById), new { id }, new { id });
            }
            catch (ValidationException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { error = ex.Message });
            }
        }


        // PUT: /api/LearningRecords/{id}
        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update([FromRoute] int id, [FromBody] UpdateLearningRecordDto dto)
        {
            try
            {
                var ok = await _svc.UpdateLearningRecord(id, dto);
                return ok ? NoContent() : NotFound(new { error = $"LearningRecord {id} not found." });
            }
            catch (ValidationException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete([FromRoute] int id)
        {
            try
            {
                var ok = await _svc.DeleteLearningRecord(id);
                return ok ? NoContent() : NotFound(new { error = $"LearningRecord {id} not found." });
            }
            catch (InvalidOperationException ex)
            {
                // đang bị ràng buộc (đã có partitions)
                return Conflict(new { error = ex.Message });
            }
        }
    }
}
