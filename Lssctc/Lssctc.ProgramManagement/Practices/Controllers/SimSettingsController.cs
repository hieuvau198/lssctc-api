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

        [HttpPost("upload-source")]
        public async Task<IActionResult> UploadSource(IFormFile file)
        {
            try
            {
                if (file == null || file.Length == 0)
                    return BadRequest(new { success = false, message = "No file uploaded." });

                using var stream = file.OpenReadStream();
                var updatedSetting = await _simSettingsService.UploadSimulationSourceAsync(stream, file.FileName, file.ContentType);

                return SuccessResponse(updatedSetting, "Simulation source uploaded successfully");
            }
            catch (KeyNotFoundException ex)
            {
                return ErrorResponse("NOT_FOUND", ex.Message, 404);
            }
            catch (Exception ex)
            {
                return ErrorResponse("UPLOAD_ERROR", "Failed to upload simulation source", 500, new { exceptionMessage = ex.Message });
            }
        }

        [HttpGet("download-source")]
        public async Task<IActionResult> DownloadSource()
        {
            try
            {
                var setting = await _simSettingsService.GetSimSettingAsync();

                if (string.IsNullOrEmpty(setting.SourceUrl))
                {
                    return ErrorResponse("NO_SOURCE", "No simulation source file is currently uploaded.", 404);
                }

                // Redirect to the Firebase URL to allow direct download
                return Redirect(setting.SourceUrl);
            }
            catch (KeyNotFoundException ex)
            {
                return ErrorResponse("NOT_FOUND", ex.Message, 404);
            }
            catch (Exception ex)
            {
                return ErrorResponse("DOWNLOAD_ERROR", "Failed to retrieve download link", 500, new { exceptionMessage = ex.Message });
            }
        }
    }
}