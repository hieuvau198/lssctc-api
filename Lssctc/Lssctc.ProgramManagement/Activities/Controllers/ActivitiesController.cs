using Lssctc.ProgramManagement.Activities.Dtos;
using Lssctc.ProgramManagement.Activities.Services;
using Lssctc.Share.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Lssctc.ProgramManagement.Activities.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ActivitiesController : ControllerBase
    {
        private readonly IActivitiesService _activitiesService;

        public ActivitiesController(IActivitiesService activitiesService)
        {
            _activitiesService = activitiesService;
        }

        #region Activities

        [HttpGet]
        [Authorize(Roles = "1,4")]
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
        [Authorize(Roles = "1,4")]
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
        [Authorize(Roles = "4,5,1")]
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
        [Authorize(Roles = "1,4")]
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
        [Authorize(Roles = "1,4")]
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
        [Authorize(Roles = "1,4")]
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

        #endregion

        #region Section Activities

        [HttpGet("section/{sectionId}")]
        [Authorize(Roles = "1,4")]
        public async Task<IActionResult> GetActivitiesBySectionId(int sectionId)
        {
            try
            {
                var activities = await _activitiesService.GetActivitiesBySectionIdAsync(sectionId);
                return Ok(activities);
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "An unexpected error occurred.");
            }
        }

        [HttpPost("section/{sectionId}/activity/{activityId}")]
        [Authorize(Roles = "1,4")]
        public async Task<IActionResult> AddActivityToSection(int sectionId, int activityId)
        {
            try
            {
                await _activitiesService.AddActivityToSectionAsync(sectionId, activityId);
                return Ok(new { Message = "Activity successfully added to section." });
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

        [HttpDelete("section/{sectionId}/activity/{activityId}")]
        [Authorize(Roles = "1,4")]
        public async Task<IActionResult> RemoveActivityFromSection(int sectionId, int activityId)
        {
            try
            {
                await _activitiesService.RemoveActivityFromSectionAsync(sectionId, activityId);
                return Ok(new { Message = "Activity successfully removed from section." });
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

        [HttpPut("section/{sectionId}/activity/{activityId}/order")]
        [Authorize(Roles = "1,4")]
        public async Task<IActionResult> UpdateSectionActivityOrder(int sectionId, int activityId, [FromQuery] int newOrder)
        {
            try
            {
                await _activitiesService.UpdateSectionActivityOrderAsync(sectionId, activityId, newOrder);
                return Ok(new { Message = "Section activity order updated successfully." });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (ArgumentOutOfRangeException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "An unexpected error occurred.");
            }
        }

        #endregion

    }
}
