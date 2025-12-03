using Lssctc.ProgramManagement.Certificates.Dtos;
using Lssctc.ProgramManagement.Certificates.Services;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace Lssctc.ProgramManagement.Certificates.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TraineeCertificatesController : ControllerBase
    {
        private readonly ITraineeCertificatesService _service;

        public TraineeCertificatesController(ITraineeCertificatesService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            return Ok(await _service.GetAllAsync());
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var result = await _service.GetByIdAsync(id);
            if (result == null) return NotFound();
            return Ok(result);
        }

        [HttpGet("code/{code}")]
        public async Task<IActionResult> GetByCode(string code)
        {
            var result = await _service.GetByCodeAsync(code);
            if (result == null) return NotFound();
            return Ok(result);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateTraineeCertificateDto dto)
        {
            try
            {
                var result = await _service.CreateCertificateAsync(dto);
                return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
            }
            catch (System.Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _service.DeleteCertificateAsync(id);
            if (!result) return NotFound();
            return NoContent();
        }
    }
}