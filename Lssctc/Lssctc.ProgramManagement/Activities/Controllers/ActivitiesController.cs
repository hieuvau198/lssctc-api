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
        [Authorize(Roles = "Admin, Instructor")]
        public async Task<IActionResult> GetAllActivities()
        {
            try
            {
                var activities = await _activitiesService.GetAllActivitiesAsync();
                return Ok(activities);
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { Message = "An unexpected error occurred." });
            }
        }

        [HttpGet("paged")]
        [Authorize(Roles = "Admin, Instructor")]
        public async Task<ActionResult<PagedResult<ActivityDto>>> GetActivities([FromQuery] int pageNumber, [FromQuery] int pageSize)
        {
            try
            {
                var pagedResult = await _activitiesService.GetActivitiesAsync(pageNumber, pageSize);
                return Ok(pagedResult);
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { Message = "An unexpected error occurred." });
            }
        }

        [HttpGet("{id}")]
        [Authorize(Roles = "Admin, Instructor, Trainee")]
        public async Task<ActionResult<ActivityDto>> GetActivityById(int id)
        {
            try
            {
                var activity = await _activitiesService.GetActivityByIdAsync(id);
                if (activity == null)
                {
                    return NotFound(new { Message = "Activity not found." });
                }
                return Ok(activity);
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { Message = "An unexpected error occurred." });
            }
        }

        [HttpPost]
        [Authorize(Roles = "Admin, Instructor")]
        public async Task<ActionResult<ActivityDto>> CreateActivity([FromBody] CreateActivityDto createDto)
        {
            if (createDto == null)
            {
                return BadRequest(new { Message = "Invalid activity data." });
            }

            try
            {
                var newActivity = await _activitiesService.CreateActivityAsync(createDto);
                return CreatedAtAction(nameof(GetActivityById), new { id = newActivity.Id }, newActivity);
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { Message = "An unexpected error occurred." });
            }
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin, Instructor")]
        public async Task<ActionResult<ActivityDto>> UpdateActivity(int id, [FromBody] UpdateActivityDto updateDto)
        {
            if (updateDto == null)
            {
                return BadRequest(new { Message = "Invalid activity data." });
            }

            try
            {
                var updatedActivity = await _activitiesService.UpdateActivityAsync(id, updateDto);
                return Ok(updatedActivity);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { Message = ex.Message });
            }
            catch (InvalidOperationException ex) // Added for lock check
            {
                return BadRequest(new { Message = ex.Message });
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { Message = "An unexpected error occurred." });
            }
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin, Instructor")]
        public async Task<IActionResult> DeleteActivity(int id)
        {
            try
            {
                await _activitiesService.DeleteActivityAsync(id);
                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { Message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { Message = "An unexpected error occurred." });
            }
        }

        #endregion

        #region Section Activities

        [HttpGet("section/{sectionId}")]
        [Authorize(Roles = "Admin, Instructor")]
        public async Task<IActionResult> GetActivitiesBySectionId(int sectionId)
        {
            try
            {
                var activities = await _activitiesService.GetActivitiesBySectionIdAsync(sectionId);
                return Ok(activities);
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { Message = "An unexpected error occurred." });
            }
        }

        [HttpPost("section/{sectionId}/activity/{activityId}")]
        [Authorize(Roles = "Admin, Instructor")]
        public async Task<IActionResult> AddActivityToSection(int sectionId, int activityId)
        {
            try
            {
                await _activitiesService.AddActivityToSectionAsync(sectionId, activityId);
                return Ok(new { Message = "Activity successfully added to section." });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { Message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { Message = "An unexpected error occurred." });
            }
        }

        [HttpPost("section/{sectionId}/activity/create")]
        [Authorize(Roles = "Admin, Instructor")]
        public async Task<ActionResult<ActivityDto>> CreateAndAssignActivityToSection(int sectionId, [FromBody] CreateActivityDto createDto)
        {
            if (createDto == null)
            {
                return BadRequest(new { Message = "Invalid activity data." });
            }

            try
            {
                var newActivity = await _activitiesService.CreateActivityForSectionAsync(sectionId, createDto);

                // Return 201 Created
                return CreatedAtAction(nameof(GetActivityById), new { id = newActivity.Id }, newActivity);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { Message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { Message = "An unexpected error occurred." });
            }
        }

        [HttpDelete("section/{sectionId}/activity/{activityId}")]
        [Authorize(Roles = "Admin, Instructor")]
        public async Task<IActionResult> RemoveActivityFromSection(int sectionId, int activityId)
        {
            try
            {
                await _activitiesService.RemoveActivityFromSectionAsync(sectionId, activityId);
                return Ok(new { Message = "Activity successfully removed from section." });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { Message = ex.Message });
            }
            catch (InvalidOperationException ex) // Added for lock check
            {
                return BadRequest(new { Message = ex.Message });
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { Message = "An unexpected error occurred." });
            }
        }

        [HttpPut("section/{sectionId}/activity/{activityId}/order")]
        [Authorize(Roles = "Admin, Instructor")]
        public async Task<IActionResult> UpdateSectionActivityOrder(int sectionId, int activityId, [FromQuery] int newOrder)
        {
            try
            {
                await _activitiesService.UpdateSectionActivityOrderAsync(sectionId, activityId, newOrder);
                return Ok(new { Message = "Section activity order updated successfully." });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { Message = ex.Message });
            }
            catch (ArgumentOutOfRangeException ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
            catch (InvalidOperationException ex) // Added for lock check
            {
                return BadRequest(new { Message = ex.Message });
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { Message = "An unexpected error occurred." });
            }
        }

        #endregion

    }
}