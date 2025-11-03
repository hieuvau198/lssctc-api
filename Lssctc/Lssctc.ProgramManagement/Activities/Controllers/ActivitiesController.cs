using Lssctc.ProgramManagement.Activities.Dtos;
using Lssctc.ProgramManagement.Activities.Services;
using Lssctc.Share.Common;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Lssctc.ProgramManagement.Activities.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ActivitiesController : ControllerBase
    {
        private readonly IActivitiesService _activitiesService;

        public ActivitiesController(IActivitiesService activitiesService)
        {
            _activitiesService = activitiesService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllActivities()
        {
            try
            {
                var activities = await _activitiesService.GetAllActivitiesAsync();
                return Ok(activities);
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "An unexpected error occurred.");
            }
        }

        [HttpGet("paged")]
        public async Task<ActionResult<PagedResult<ActivityDto>>> GetActivities([FromQuery] int pageNumber, [FromQuery] int pageSize)
        {
            try
            {
                var pagedResult = await _activitiesService.GetActivitiesAsync(pageNumber, pageSize);
                return Ok(pagedResult);
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "An unexpected error occurred.");
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ActivityDto>> GetActivityById(int id)
        {
            try
            {
                var activity = await _activitiesService.GetActivityByIdAsync(id);
                if (activity == null)
                {
                    return NotFound();
                }
                return Ok(activity);
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "An unexpected error occurred.");
            }
        }

        [HttpPost]
        public async Task<ActionResult<ActivityDto>> CreateActivity([FromBody] CreateActivityDto createDto)
        {
            if (createDto == null)
            {
                return BadRequest();
            }

            try
            {
                var newActivity = await _activitiesService.CreateActivityAsync(createDto);
                return CreatedAtAction(nameof(GetActivityById), new { id = newActivity.Id }, newActivity);
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "An unexpected error occurred.");
            }
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<ActivityDto>> UpdateActivity(int id, [FromBody] UpdateActivityDto updateDto)
        {
            if (updateDto == null)
            {
                return BadRequest();
            }

            try
            {
                var updatedActivity = await _activitiesService.UpdateActivityAsync(id, updateDto);
                return Ok(updatedActivity);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "An unexpected error occurred.");
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteActivity(int id)
        {
            try
            {
                await _activitiesService.DeleteActivityAsync(id);
                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "An unexpected error occurred.");
            }
        }
    }
}
