using Lssctc.ProgramManagement.Certificates.Dtos;
using Lssctc.ProgramManagement.Certificates.Services;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Lssctc.ProgramManagement.Certificates.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CertificatesController : ControllerBase
    {
        private readonly ICertificatesService _service;

        public CertificatesController(ICertificatesService service)
        {
            _service = service;
        }

        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<CertificateTemplateDto>), 200)]
        public async Task<IActionResult> GetAll()
        {
            return Ok(await _service.GetAllTemplatesAsync());
        }

        [HttpGet("{id}")]
        [ProducesResponseType(typeof(CertificateTemplateDto), 200)]
        public async Task<IActionResult> GetById(int id)
        {
            var result = await _service.GetTemplateByIdAsync(id);
            if (result == null) return NotFound();
            return Ok(result);
        }

        [HttpGet("course/{courseId}")]
        [ProducesResponseType(typeof(CertificateTemplateDto), 200)]
        public async Task<IActionResult> GetByCourseId(int courseId)
        {
            var result = await _service.GetCertificateByCourseIdAsync(courseId);
            if (result == null) return NotFound("No active certificate assigned to this course.");
            return Ok(result);
        }

        [HttpGet("class/{classId}")]
        [ProducesResponseType(typeof(CertificateTemplateDto), 200)]
        public async Task<IActionResult> GetByClassId(int classId)
        {
            var result = await _service.GetCertificateByClassIdAsync(classId);
            if (result == null) return NotFound("No active certificate found for the course associated with this class.");
            return Ok(result);
        }

        [HttpPost]
        [ProducesResponseType(typeof(CertificateTemplateDto), 201)]
        public async Task<IActionResult> Create([FromBody] CreateCertificateTemplateDto dto)
        {
            var result = await _service.CreateTemplateAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
        }

        [HttpPut("{id}")]
        [ProducesResponseType(typeof(CertificateTemplateDto), 200)]
        public async Task<IActionResult> Update(int id, [FromBody] CreateCertificateTemplateDto dto)
        {
            var result = await _service.UpdateTemplateAsync(id, dto);
            if (result == null) return NotFound();
            return Ok(result);
        }

        [HttpPost("courses/{courseId}/assign/{certificateId}")]
        [ProducesResponseType(typeof(object), 200)]
        public async Task<IActionResult> AssignToCourse(int courseId, int certificateId)
        {
            var result = await _service.AssignCertificateToCourseAsync(courseId, certificateId);
            if (!result) return BadRequest("Failed to assign certificate. Verify Course ID and Certificate ID.");
            return Ok(new { Message = "Certificate assigned successfully." });
        }

        [HttpPost("courses/{courseId}/auto-assign")]
        [ProducesResponseType(typeof(object), 200)]
        public async Task<IActionResult> AutoAssignToCourse(int courseId)
        {
            var result = await _service.AutoAssignCertificateToCourseAsync(courseId);
            if (!result) return BadRequest("Failed to auto-assign certificate. Ensure at least one active certificate template exists.");
            return Ok(new { Message = "Default certificate assigned successfully." });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _service.DeleteTemplateAsync(id);
            if (!result) return NotFound();
            return NoContent();
        }
    }
}