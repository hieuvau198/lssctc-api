using Lssctc.ProgramManagement.Certificates.Dtos;
using Lssctc.ProgramManagement.Certificates.Services;
using Microsoft.AspNetCore.Mvc;
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
        public async Task<IActionResult> GetAll()
        {
            return Ok(await _service.GetAllTemplatesAsync());
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateCertificateTemplateDto dto)
        {
            var result = await _service.CreateTemplateAsync(dto);
            return CreatedAtAction(nameof(GetAll), new { id = result.Id }, result);
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