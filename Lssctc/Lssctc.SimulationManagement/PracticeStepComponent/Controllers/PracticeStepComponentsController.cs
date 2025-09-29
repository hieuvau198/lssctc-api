using Lssctc.SimulationManagement.PracticeStepComponent.Dtos;
using Lssctc.SimulationManagement.PracticeStepComponent.Services;
using Lssctc.SimulationManagement.SectionPractice.Dtos;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Lssctc.SimulationManagement.PracticeStepComponent.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PracticeStepComponentsController : ControllerBase
    {
        private readonly IPracticeStepComponentService _svc;

        public PracticeStepComponentsController(IPracticeStepComponentService svc)
        {
            _svc = svc;
        }

        // GET: /api/PracticeStepComponents?pageIndex=1&pageSize=20&practiceStepId=&simulationComponentId=&search=
        [HttpGet]
        public async Task<IActionResult> GetPaged(
            [FromQuery] int pageIndex = 1,
            [FromQuery] int pageSize = 20,
            [FromQuery] int? practiceStepId = null,
            [FromQuery] int? simulationComponentId = null,
            [FromQuery] string? search = null)
        {
            var result = await _svc.GetPagedAsync(pageIndex, pageSize, practiceStepId, simulationComponentId, search);

            return Ok(new ApiResponse<IEnumerable<PracticeStepComponentDto>>
            {
                Success = true,
                StatusCode = 200,
                Message = "Get practice step components successfully.",
                Data = result.Items,
                Pagination = new Pagination
                {
                    PageIndex = result.Page,
                    PageSize = result.PageSize,
                    TotalItems = result.TotalCount,
                    TotalPages = result.TotalPages
                }
            });
        }
    }
}
