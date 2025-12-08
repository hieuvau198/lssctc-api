using Lssctc.ProgramManagement.ClassManage.Classes.Dtos;

namespace Lssctc.ProgramManagement.ClassManage.Classes.Services
{
    public interface IClassInstructorsService
    {
        #region Instructors
        // BR: only users with 'Instructor' role can be assigned to class
        // BR: an instructor cannot be assigned to more than one class at the same time (i.e., in another 'Open' or 'Inprogress' class)
        // BR: only one instructor can be assigned to a class
        // BR: can only remove instructor if class is 'Draft'

        Task AssignInstructorAsync(int classId, int instructorId);

        Task RemoveInstructorAsync(int classId);

        Task<ClassInstructorDto?> GetInstructorByClassIdAsync(int classId);

        /// <summary>
        /// Get available instructors within a date range.
        /// An instructor is available if they are NOT assigned to any class that:
        /// 1. Has status Open or Inprogress, AND
        /// 2. Has date range that overlaps with the specified date range
        /// </summary>
        /// <param name="startDate">Start date of the period</param>
        /// <param name="endDate">End date of the period</param>
        /// <returns>List of available instructors</returns>
        Task<IEnumerable<ClassInstructorDto>> GetAvailableInstructorsAsync(DateTime startDate, DateTime endDate);
        #endregion
    }
}
