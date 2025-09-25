using Lssctc.Share.Common;
using Lssctc.SimulationManagement.Practices.Dtos;
using Lssctc.SimulationManagement.Practices.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Lssctc.SimulationManagement.Practices.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PracticesController : ControllerBase
    {
        private readonly IPracticeService _practiceService;

        public PracticesController(IPracticeService practiceService)
        {
            _practiceService = practiceService;
        }

        [HttpGet]
        public async Task<ActionResult<PagedResult<PracticeDto>>> GetAll([FromQuery] PracticeQueryDto query)
        {
            try
            {
                var result = await _practiceService.GetAllAsync(query);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving practices.", detail = ex.Message });
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<PracticeDto>> GetById(int id)
        {
            try
            {
                if (id <= 0)
                    return BadRequest(new { message = "Invalid practice ID." });

                var practice = await _practiceService.GetByIdAsync(id);

                if (practice == null)
                    return NotFound(new { message = $"Practice with ID {id} not found." });

                return Ok(practice);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving the practice.", detail = ex.Message });
            }
        }

        [HttpPost]
        public async Task<ActionResult<PracticeDto>> Create([FromBody] CreatePracticeDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var practice = await _practiceService.CreateAsync(dto);

                return CreatedAtAction(
                    nameof(GetById),
                    new { id = practice.Id },
                    practice);
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while creating the practice.", detail = ex.Message });
            }
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<PracticeDto>> Update(int id, [FromBody] UpdatePracticeDto dto)
        {
            try
            {
                if (id <= 0)
                    return BadRequest(new { message = "Invalid practice ID." });

                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var practice = await _practiceService.UpdateAsync(id, dto);

                if (practice == null)
                    return NotFound(new { message = $"Practice with ID {id} not found." });

                return Ok(practice);
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while updating the practice.", detail = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(int id)
        {
            try
            {
                if (id <= 0)
                    return BadRequest(new { message = "Invalid practice ID." });

                var success = await _practiceService.DeleteAsync(id);

                if (!success)
                    return NotFound(new { message = $"Practice with ID {id} not found." });

                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while deleting the practice.", detail = ex.Message });
            }
        }

        [HttpHead("{id}")]
        public async Task<ActionResult> Exists(int id)
        {
            try
            {
                if (id <= 0)
                    return BadRequest();

                var exists = await _practiceService.ExistsAsync(id);

                return exists ? Ok() : NotFound();
            }
            catch (Exception)
            {
                return StatusCode(500);
            }
        }
    }
}
