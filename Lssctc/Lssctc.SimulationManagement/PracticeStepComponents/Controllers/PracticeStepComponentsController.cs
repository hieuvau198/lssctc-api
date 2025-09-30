using Lssctc.SimulationManagement.PracticeStepComponents.Dtos;
using Lssctc.SimulationManagement.PracticeStepComponents.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Lssctc.SimulationManagement.PracticeStepComponents.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PracticeStepComponentsController : ControllerBase
    {
        private readonly IPracticeStepComponentService _service;

        public PracticeStepComponentsController(IPracticeStepComponentService service)
        {
            _service = service;
        }

        [HttpGet("{stepId}")]
        public async Task<ActionResult<List<PracticeStepComponentDto>>> Get(int stepId)
        {
            var result = await _service.GetByPracticeStepIdAsync(stepId);
            return Ok(result);
        }

        [HttpPost]
        public async Task<ActionResult<PracticeStepComponentDto>> Create([FromBody] CreatePracticeStepComponentDto dto)
        {
            
            var result = await _service.AssignSimulationComponentAsync(dto);
            return Ok(result);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<PracticeStepComponentDto>> UpdateOrder(int id, [FromBody] UpdatePracticeStepComponentDto dto)
        {
            var result = await _service.UpdateOrderAsync(id, dto);
            if (result == null)
                return NotFound();
            return Ok(result);
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(int id)
        {
            var success = await _service.RemoveAsync(id);
            if (!success)
                return NotFound();
            return NoContent();
        }
    }
}
