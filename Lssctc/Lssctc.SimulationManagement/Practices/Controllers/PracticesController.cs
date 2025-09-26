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
        private readonly IPracticeStepService _practiceStepService;

        public PracticesController(IPracticeService practiceService, IPracticeStepService practiceStepService)
        {
            _practiceService = practiceService;
            _practiceStepService = practiceStepService;
        }

        [HttpGet]
        public async Task<ActionResult<PagedResult<PracticeDto>>> GetAll([FromQuery] PracticeQueryDto query)
        {
            try
            {
                var result = await _practiceService.GetPractices(query);
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

                var practice = await _practiceService.GetPracticeById(id);

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

                var practice = await _practiceService.CreatePractice(dto);

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

                var practice = await _practiceService.UpdatePractice(id, dto);

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

                var success = await _practiceService.DeletePractice(id);

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

                var exists = await _practiceService.ExistPractice(id);

                return exists ? Ok() : NotFound();
            }
            catch (Exception)
            {
                return StatusCode(500);
            }
        }


        // GET: api/practices/{practiceId}/steps
        [HttpGet("{practiceId}/steps")]
        public async Task<ActionResult<List<PracticeStepDto>>> GetPracticeStepsByPracticeId(int practiceId)
        {
            if (practiceId <= 0)
                return BadRequest(new { message = "Invalid practice ID." });

            var steps = await _practiceStepService.GetPracticeStepsByPracticeIdAsync(practiceId);
            return Ok(steps);
        }

        // GET: api/practices/steps/{stepId}
        [HttpGet("steps/{stepId}")]
        public async Task<ActionResult<PracticeStepDto>> GetPracticeStepById(int stepId)
        {
            if (stepId <= 0)
                return BadRequest(new { message = "Invalid step ID." });

            var step = await _practiceStepService.GetPracticeStepByIdAsync(stepId);
            if (step == null)
                return NotFound(new { message = $"PracticeStep with ID {stepId} not found." });

            return Ok(step);
        }

        // POST: api/practices/steps
        [HttpPost("steps")]
        public async Task<ActionResult<PracticeStepDto>> CreatePracticeStep([FromBody] CreatePracticeStepDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var step = await _practiceStepService.CreatePracticeStepAsync(dto);

            return CreatedAtAction(nameof(GetPracticeStepById), new { stepId = step.Id }, step);
        }

        // PUT: api/practices/steps/{stepId}
        [HttpPut("steps/{stepId}")]
        public async Task<ActionResult<PracticeStepDto>> UpdatePracticeStep(int stepId, [FromBody] UpdatePracticeStepDto dto)
        {
            if (stepId <= 0)
                return BadRequest(new { message = "Invalid step ID." });

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var step = await _practiceStepService.UpdatePracticeStepAsync(stepId, dto);
            if (step == null)
                return NotFound(new { message = $"PracticeStep with ID {stepId} not found." });

            return Ok(step);
        }

        // DELETE: api/practices/steps/{stepId}
        [HttpDelete("steps/{stepId}")]
        public async Task<ActionResult> DeletePracticeStep(int stepId)
        {
            if (stepId <= 0)
                return BadRequest(new { message = "Invalid step ID." });

            var success = await _practiceStepService.DeletePracticeStepAsync(stepId);
            if (!success)
                return NotFound(new { message = $"PracticeStep with ID {stepId} not found." });

            return NoContent();
        }

    }
}
