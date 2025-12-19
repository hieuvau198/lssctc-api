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
        private readonly IClassCompleteService _classCompleteService;

        public AdministrationsController(
            IClassCustomizeService classCustomizeService,
            IClassCompleteService classCompleteService)
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
        /// Force completes a class (Orchestrator). 
        /// </summary>
        [HttpPost("classes/{id}/auto-complete")]
        public async Task<IActionResult> AutoCompleteClass(int id)
        {
            await _classCompleteService.AutoCompleteClass(id);
            return NoContent();
        }

        /// <summary>
        /// Auto completes learning progress for all students in a class.
        /// </summary>
        [HttpPost("classes/{id}/complete-progress")]
        public async Task<IActionResult> AutoCompleteClassProgress(int id)
        {
            await _classCompleteService.AutoCompleteLearningProgress(id);
            return NoContent();
        }

        /// <summary>
        /// Auto completes attendance for all students in a class.
        /// </summary>
        [HttpPost("classes/{id}/complete-attendance")]
        public async Task<IActionResult> AutoCompleteClassAttendance(int id)
        {
            await _classCompleteService.AutoCompleteAttendance(id);
            return NoContent();
        }

        /// <summary>
        /// Auto completes final exams for all students in a class.
        /// </summary>
        [HttpPost("classes/{id}/complete-final-exam")]
        public async Task<IActionResult> AutoCompleteClassFinalExam(int id)
        {
            await _classCompleteService.AutoCompleteFinalExam(id);
            return NoContent();
        }

        /// <summary>
        /// Auto completes learning progress for a specific enrollment.
        /// </summary>
        [HttpPost("enrollments/{id}/complete-progress")]
        public async Task<IActionResult> AutoCompleteEnrollmentProgress(int id)
        {
            await _classCompleteService.AutoCompleteLearningProgressForEnrollment(id);
            return NoContent();
        }

        /// <summary>
        /// Auto completes attendance for a specific enrollment.
        /// </summary>
        [HttpPost("enrollments/{id}/complete-attendance")]
        public async Task<IActionResult> AutoCompleteEnrollmentAttendance(int id)
        {
            await _classCompleteService.AutoCompleteAttendanceForEnrollment(id);
            return NoContent();
        }

        /// <summary>
        /// Auto completes final exam for a specific enrollment.
        /// </summary>
        [HttpPost("enrollments/{id}/complete-final-exam")]
        public async Task<IActionResult> AutoCompleteEnrollmentFinalExam(int id)
        {
            await _classCompleteService.AutoCompleteFinalExamForEnrollment(id);
            return NoContent();
        }
    }
}