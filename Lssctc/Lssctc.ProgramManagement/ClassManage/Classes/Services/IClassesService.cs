using Lssctc.ProgramManagement.ClassManage.Classes.Dtos;
using Lssctc.Share.Common;
using Microsoft.AspNetCore.Http;

namespace Lssctc.ProgramManagement.ClassManage.Classes.Services
{
    public interface IClassesService
    {
        #region Classes
        Task<IEnumerable<ClassDto>> GetAllClassesAsync();
        Task<PagedResult<ClassDto>> GetClassesAsync(int pageNumber, int pageSize);
        Task<ClassDto?> GetClassByIdAsync(int id);
        // BR create class: with initial status 'Draft'
        // startdate must be from current time to
        // enddate must be after startdate, at least 3 day later
        // classcode cannot be duplicated
        Task<ClassDto> CreateClassAsync(CreateClassDto dto);
        // BR update class: allow for update only if class status is 'Draft'
        // startdate must be from current time to
        // enddate must be after startdate, at least 3 day later
        // classcode cannot be duplicated
        Task<ClassDto> UpdateClassAsync(int id, UpdateClassDto dto);
        // BR start class: allow for start only if class status is 'Draft'
        Task OpenClassAsync(int id);
        // BR start class: allow for start only if class status is 'Draft' or 'Open'
        // startdate and enddate must be valid
        // must have at least one instructor assigned
        // must have at least one enrolled student
        // after starting class, class status is 'In Progress'
        // after starting class, automatically create progress, section record, activity record for each enrolled student
        Task StartClassAsync(int id);
        Task CompleteClassAsync(int id);
        // BR cancel class: allow for delete only if no enrolled students,
        // class status is 'Draft' or 'Open'
        Task CancelClassAsync(int id);
        // BR hard delete class: permanently delete class and all associated data for demo purposes
        // This includes: ClassInstructors, Enrollments, LearningProgress, SectionRecords, ActivityRecords,
        // QuizAttempts, QuizAttemptQuestions, QuizAttemptAnswers, PracticeAttempts, PracticeAttemptTasks
        // Uses database transaction to ensure atomicity
        Task DeleteClassDataRecursiveAsync(int classId);
        // BR import trainees: bulk import trainees from Excel file
        // For each row: Find or Create User with Role='Trainee', then Enroll in Class (if not already enrolled)
        // Returns summary message of import results
        Task<string> ImportTraineesToClassAsync(int classId, IFormFile file);
        #endregion

        #region Classes By other Filters
        Task<IEnumerable<ClassDto>> GetClassesByProgramAndCourseAsync(int programId, int courseId);
        Task<IEnumerable<ClassDto>> GetClassesByCourseAsync(int courseId);
        Task<IEnumerable<ClassDto>> GetClassesByCourseIdForTrainee(int courseId);
        Task<IEnumerable<ClassDto>> GetClassesByInstructorAsync(int instructorId);
        Task<IEnumerable<ClassDto>> GetAllClassesByTraineeAsync(int traineeId);
        Task<PagedResult<ClassDto>> GetPagedClassesByTraineeAsync(int traineeId, int pageNumber, int pageSize);
        Task<ClassDto?> GetClassByIdAndTraineeAsync(int classId, int traineeId);
        #endregion


    }
}
