using LearnerService.Application.Dtos;
using LearnerService.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace LearnerService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LearnersController : ControllerBase
    {
        private readonly ILearnersService _learnersService;

        public LearnersController(ILearnersService learnersService)
        {
            _learnersService = learnersService;
        }

        [HttpGet]
        public async Task<IActionResult> GetLearners([FromQuery] LearnerQueryParameters parameters)
        {
            var result = await _learnersService.GetLearnersAsync(parameters);
            return Ok(result);
        }

        [HttpGet("{userId:int}")]
        public async Task<IActionResult> GetLearner(int userId)
        {
            var learner = await _learnersService.GetLearnerByIdAsync(userId);
            if (learner == null)
                return NotFound();
            return Ok(learner);
        }

        [HttpPost]
        public async Task<IActionResult> CreateLearner([FromBody] CreateLearnerDto dto)
        {
            var created = await _learnersService.CreateLearnerAsync(dto);
            return CreatedAtAction(nameof(GetLearner), new { userId = created.UserId }, created);
        }

        [HttpPut("{userId:int}")]
        public async Task<IActionResult> UpdateLearner(int userId, [FromBody] UpdateLearnerDto dto)
        {
            var updated = await _learnersService.UpdateLearnerAsync(userId, dto);
            if (updated == null)
                return NotFound();
            return Ok(updated);
        }

        [HttpDelete("{userId:int}")]
        public async Task<IActionResult> DeleteLearner(int userId)
        {
            var deleted = await _learnersService.DeleteLearnerAsync(userId);
            if (!deleted)
                return NotFound();
            return NoContent();
        }
    }
}
