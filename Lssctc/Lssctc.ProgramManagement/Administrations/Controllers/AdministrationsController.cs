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
        private readonly AccountHelper _accountHelper;
        private readonly EnrollmentResetHelper _enrollmentResetHelper;

        public AdministrationsController(
            IClassCustomizeService classCustomizeService,
            IClassCompleteService classCompleteService,
            AccountHelper accountHelper,
            EnrollmentResetHelper enrollmentResetHelper)
        {
            _classCustomizeService = classCustomizeService;
            _classCompleteService = classCompleteService;
            _accountHelper = accountHelper;
            _enrollmentResetHelper = enrollmentResetHelper;
        }

        /// <summary>
        /// Hard deletes a user account and all related data (Trainee, Instructor, Profiles, Records, etc.).
        /// </summary>
        [HttpDelete("users/{id}/hard-delete")]
        public async Task<IActionResult> DeleteUserCompletely(int id)
        {
            await _accountHelper.HardDeleteUserAccountAsync(id);
            return NoContent();
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

        /// <summary>
        /// Resets attendance for a specific enrollment to 'NotStarted'.
        /// </summary>
        [HttpPost("enrollments/{id}/reset-attendance")]
        public async Task<IActionResult> ResetEnrollmentAttendance(int id)
        {
            await _enrollmentResetHelper.ResetAttendanceAsync(id);
            return NoContent();
        }

        /// <summary>
        /// Resets learning progress (scores, completion status) for a specific enrollment.
        /// </summary>
        [HttpPost("enrollments/{id}/reset-progress")]
        public async Task<IActionResult> ResetEnrollmentProgress(int id)
        {
            await _enrollmentResetHelper.ResetLearningProgressAsync(id);
            return NoContent();
        }

        /// <summary>
        /// Resets final exam (scores, status) for a specific enrollment.
        /// </summary>
        [HttpPost("enrollments/{id}/reset-final-exam")]
        public async Task<IActionResult> ResetEnrollmentFinalExam(int id)
        {
            await _enrollmentResetHelper.ResetFinalExamAsync(id);
            return NoContent();
        }
    }
}