using System.Threading.Tasks;

namespace Lssctc.ProgramManagement.ClassManage.Helpers
{
    public interface IClassCompleteService
    {
        /// <summary>
        /// Main orchestrator method to simulate/force the completion of a class.
        /// </summary>
        Task AutoCompleteClass(int classId);

        /// <summary>
        /// Auto completes the learning progress for all enrollments in the specified class.
        /// </summary>
        Task AutoCompleteLearningProgress(int classId);

        /// <summary>
        /// Auto completes attendance for all enrollments in the specified class.
        /// </summary>
        Task AutoCompleteAttendance(int classId);

        /// <summary>
        /// Auto completes the final exam for all enrollments in the class.
        /// </summary>
        Task AutoCompleteFinalExam(int classId);

        /// <summary>
        /// Auto completes the learning progress for a specific enrollment.
        /// </summary>
        Task AutoCompleteLearningProgressForEnrollment(int enrollmentId);

        /// <summary>
        /// Auto completes attendance for a specific enrollment.
        /// </summary>
        Task AutoCompleteAttendanceForEnrollment(int enrollmentId);

        /// <summary>
        /// Auto completes the final exam for a specific enrollment.
        /// </summary>
        Task AutoCompleteFinalExamForEnrollment(int enrollmentId);
    }
}