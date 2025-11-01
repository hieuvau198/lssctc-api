namespace Lssctc.ProgramManagement.Programs.Services
{
    public interface IProgramCoursesService
    {
        // add course to program
        Task AddCourseToProgramAsync(int programId, int courseId);
        // change program course
        Task UpdateProgramCourseAsync(int programId, int courseId, int newOrder);
        // remove course from program
        Task RemoveCourseFromProgramAsync(int programId, int courseId);
    }
}
