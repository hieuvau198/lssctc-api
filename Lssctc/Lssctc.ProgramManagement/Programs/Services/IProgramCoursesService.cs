namespace Lssctc.ProgramManagement.Programs.Services
{
    public interface IProgramCoursesService
    {
        Task AddCourseToProgramAsync(int programId, int courseId);
        Task UpdateProgramCourseAsync(int programId, int courseId, int newOrder);
        Task RemoveCourseFromProgramAsync(int programId, int courseId);
    }
}
