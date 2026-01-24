using Lssctc.ProgramManagement.Practices.Dtos;
using Lssctc.ProgramManagement.Practices.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Lssctc.ProgramManagement.Practices.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SimSettingsController : ControllerBase
    {
        private readonly ISimSettingsService _simSettingsService;

        public SimSettingsController(ISimSettingsService simSettingsService)
        {
            _simSettingsService = simSettingsService;
        }

        private IActionResult ErrorResponse(string errorCode, string errorMessage, int statusCode, object? errorDetails = null)
        {
            return StatusCode(statusCode, new
            {
                success = false,
                error = new { code = errorCode, message = errorMessage, details = errorDetails, timestamp = DateTime.UtcNow }
            });
        }

        private IActionResult SuccessResponse<T>(T data, string? message = null)
        {
            return Ok(new { success = true, message, data });
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            try
            {
                var setting = await _simSettingsService.GetSimSettingAsync();
                return SuccessResponse(setting);
            }
            catch (KeyNotFoundException ex)
            {
                return ErrorResponse("NOT_FOUND", ex.Message, 404);
            }
            catch (Exception ex)
            {
                return ErrorResponse("FETCH_ERROR", "Failed to fetch sim settings", 500, new { exceptionMessage = ex.Message });
            }
        }

        [HttpPut]
        public async Task<IActionResult> Update([FromBody] SimSettingDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                // We ignore the ID in the DTO because we always update ID 1
                var updated = await _simSettingsService.UpdateSimSettingAsync(dto);
                return SuccessResponse(updated, "Sim setting updated successfully");
            }
            catch (KeyNotFoundException ex)
            {
                return ErrorResponse("NOT_FOUND", ex.Message, 404);
            }
            catch (Exception ex)
            {
                return ErrorResponse("UPDATE_ERROR", "Failed to update sim settings", 500, new { exceptionMessage = ex.Message });
            }
        }
    }
}
