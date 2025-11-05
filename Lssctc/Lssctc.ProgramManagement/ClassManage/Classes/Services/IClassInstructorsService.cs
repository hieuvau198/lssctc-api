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
        #endregion
    }
}
