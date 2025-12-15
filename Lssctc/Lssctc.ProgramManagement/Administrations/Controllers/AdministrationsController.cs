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

        public AdministrationsController(IClassCustomizeService classCustomizeService)
        {
            _classCustomizeService = classCustomizeService;
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
    }
}