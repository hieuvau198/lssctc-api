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
        private readonly ClassCompleteService _classCompleteService;

        public AdministrationsController(IClassCustomizeService classCustomizeService, ClassCompleteService classCompleteService)
        {
            _classCustomizeService = classCustomizeService;
            _classCompleteService = classCompleteService;
        }

        /// <summary>
        /// Hard deletes a class and all its related data (enrollments, records, exams, etc.).
        /// This action is irreversible.
        /// </summary>
        /// <param name="id">The Class ID to delete</param>
        /// <returns>No Content</returns>
        [HttpDelete("classes/{id}/hard-delete")]
        public async Task<IActionResult> DeleteClassCompletely(int id)
        {
            await _classCustomizeService.DeleteClassCompletelyAsync(id);
            return NoContent();
        }

        /// <summary>
        /// Force completes a class. 
        /// Auto-fills attendance, learning progress, passes final exams, sets class status to Completed, and generates certificates/emails.
        /// </summary>
        /// <param name="id">The Class ID to complete</param>
        /// <returns>No Content</returns>
        [HttpPost("classes/{id}/auto-complete")]
        public async Task<IActionResult> AutoCompleteClass(int id)
        {
            await _classCompleteService.AutoCompleteClass(id);
            return NoContent();
        }
    }
}