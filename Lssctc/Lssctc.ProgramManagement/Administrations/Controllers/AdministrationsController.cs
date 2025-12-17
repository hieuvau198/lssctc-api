using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Lssctc.ProgramManagement.ClassManage.Helpers;
using System.Threading.Tasks;

namespace Lssctc.ProgramManagement.Administrations.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AdministrationsController : ControllerBase
    {
        private readonly IClassCustomizeService _classCustomizeService;
        private readonly IClassCompleteService _classCompleteService; // Updated to Interface

        public AdministrationsController(
            IClassCustomizeService classCustomizeService,
            IClassCompleteService classCompleteService) // Updated to Interface
        {
            _classCustomizeService = classCustomizeService;
            _classCompleteService = classCompleteService;
        }

        /// <summary>
        /// Hard deletes a class and all its related data.
        /// </summary>
        [HttpDelete("classes/{id}/hard-delete")]
        public async Task<IActionResult> DeleteClassCompletely(int id)
        {
            await _classCustomizeService.DeleteClassCompletelyAsync(id);
            return NoContent();
        }

        /// <summary>
        /// Force completes a class. 
        /// </summary>
        [HttpPost("classes/{id}/auto-complete")]
        public async Task<IActionResult> AutoCompleteClass(int id)
        {
            await _classCompleteService.AutoCompleteClass(id);
            return NoContent();
        }
    }
}