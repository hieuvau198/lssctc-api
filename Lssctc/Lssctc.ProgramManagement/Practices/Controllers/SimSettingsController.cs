using Lssctc.ProgramManagement.Practices.Dtos;
using Lssctc.ProgramManagement.Practices.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http; // Added for HttpClient

namespace Lssctc.ProgramManagement.Practices.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SimSettingsController : ControllerBase
    {
        private readonly ISimSettingsService _simSettingsService;
        private readonly IHttpClientFactory _httpClientFactory; // Added factory

        // Inject IHttpClientFactory
        public SimSettingsController(ISimSettingsService simSettingsService, IHttpClientFactory httpClientFactory)
        {
            _simSettingsService = simSettingsService;
            _httpClientFactory = httpClientFactory;
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
        [DisableRequestSizeLimit]
        [RequestFormLimits(MultipartBodyLengthLimit = long.MaxValue)]
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

                // FIX: Proxy the request instead of Redirecting.
                // This avoids CORS errors because the browser only talks to your API,
                // and your API (server-side) fetches the file from Firebase.

                var client = _httpClientFactory.CreateClient();

                // Use ResponseHeadersRead to avoid buffering the entire file into memory
                var response = await client.GetAsync(setting.SourceUrl, HttpCompletionOption.ResponseHeadersRead);

                if (!response.IsSuccessStatusCode)
                {
                    return ErrorResponse("UPSTREAM_ERROR", "Failed to retrieve file from storage.", (int)response.StatusCode);
                }

                var stream = await response.Content.ReadAsStreamAsync();
                var contentType = response.Content.Headers.ContentType?.ToString() ?? "application/octet-stream";

                // Attempt to extract a clean filename from the URL, or default to a generic name
                string fileName = "SimulationSource.rar";
                try
                {
                    // URL Format: .../o/SimSource_123123_Name.rar?alt=media
                    // Extract the path segment
                    var uri = new Uri(setting.SourceUrl);
                    var path = uri.LocalPath; // /v0/b/.../o/SimSource_...
                    var segments = path.Split('/');
                    if (segments.Length > 0)
                    {
                        // The last segment is the object name (URL encoded)
                        fileName = Uri.UnescapeDataString(segments[^1]);
                    }
                }
                catch
                {
                    // If parsing fails, use the default name
                }

                return File(stream, contentType, fileName);
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