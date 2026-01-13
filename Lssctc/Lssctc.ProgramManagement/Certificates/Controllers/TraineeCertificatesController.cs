using Lssctc.ProgramManagement.Certificates.Dtos;
using Lssctc.ProgramManagement.Certificates.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Security.Claims;
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
        [ProducesResponseType(typeof(IEnumerable<TraineeCertificateResponseDto>), 200)]
        public async Task<IActionResult> GetAll()
        {
            return Ok(await _service.GetAllAsync());
        }

        [HttpGet("{id}")]
        [ProducesResponseType(typeof(TraineeCertificateResponseDto), 200)]
        public async Task<IActionResult> GetById(int id)
        {
            var result = await _service.GetByIdAsync(id);
            if (result == null) return NotFound();
            return Ok(result);
        }

        [HttpGet("code/{code}")]
        [ProducesResponseType(typeof(TraineeCertificateResponseDto), 200)]
        public async Task<IActionResult> GetByCode(string code)
        {
            var result = await _service.GetByCodeAsync(code);
            if (result == null) return NotFound();
            return Ok(result);
        }

        [HttpGet("class/{classId}")]
        [ProducesResponseType(typeof(IEnumerable<TraineeCertificateResponseDto>), 200)]
        public async Task<IActionResult> GetByClassId(int classId)
        {
            var result = await _service.GetTraineeCertificatesByClassIdAsync(classId);
            return Ok(result);
        }

        /// <summary>
        /// Get all certificates for the current authenticated trainee (from token).
        /// </summary>
        [HttpGet("my-certificates")]
        [Authorize(Roles = "Trainee")]
        [ProducesResponseType(typeof(IEnumerable<TraineeCertificateResponseDto>), 200)]
        [ProducesResponseType(401)]
        public async Task<IActionResult> GetMyCertificates()
        {
            try
            {
                var traineeId = GetTraineeIdFromClaims();
                var result = await _service.GetTraineeCertificatesByTraineeIdAsync(traineeId);
                return Ok(result);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
            catch (System.Exception)
            {
                return StatusCode(500, new { message = "An unexpected error occurred." });
            }
        }

        /// <summary>
        /// Get all certificates for a specific trainee by ID (Admin/Instructor only).
        /// </summary>
        [HttpGet("trainee/{traineeId}")]
        [Authorize(Roles = "Admin, Instructor")]
        [ProducesResponseType(typeof(IEnumerable<TraineeCertificateResponseDto>), 200)]
        public async Task<IActionResult> GetByTraineeId(int traineeId)
        {
            var result = await _service.GetTraineeCertificatesByTraineeIdAsync(traineeId);
            return Ok(result);
        }

        [HttpPost]
        [ProducesResponseType(typeof(TraineeCertificateResponseDto), 201)]
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

        #region Private Helpers

        private int GetTraineeIdFromClaims()
        {
            var traineeIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (int.TryParse(traineeIdClaim, out int traineeId))
            {
                return traineeId;
            }

            throw new UnauthorizedAccessException("Trainee ID claim is missing or invalid.");
        }

        #endregion
    }
}