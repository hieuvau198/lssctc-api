using Lssctc.ProgramManagement.ClassManage.Enrollments.Dtos;
using Lssctc.Share.Common;

namespace Lssctc.ProgramManagement.ClassManage.Enrollments.Services
{
    public interface IEnrollmentsService
    {
        /*
        BR: trainee can enroll in a class only if the class status is 'Open', then enrollment auto have status Pending
        BR: trainee can withdraw from a class only if class status is 'Draft' or 'Open', then enrollment status become 'Cancelled'
        BR: instructor can add trainee to a class only if class status is 'Draft' or 'Open', then enrollment auto have status 'Enrolled'
        BR: instructor can remove trainee from a class only if class status is 'Draft' or 'Open', then enrollment status become 'Cancelled'
         */

        #region Trainee Enrollments
        Task<EnrollmentDto> EnrollInClassAsync(int traineeId, CreateEnrollmentDto dto);
        Task WithdrawFromClassAsync(int traineeId, int classId);
        Task<IEnumerable<EnrollmentDto>> GetMyEnrollmentsAsync(int traineeId);
        Task<PagedResult<EnrollmentDto>> GetMyEnrollmentsAsync(int traineeId, int pageNumber, int pageSize);
        Task<EnrollmentDto?> GetMyEnrollmentByIdAsync(int traineeId, int enrollmentId);
        Task<EnrollmentDto?> GetMyEnrollmentByClassIdAsync(int traineeId, int classId);
        #endregion

        #region Internal Enrollments
        Task<PagedResult<EnrollmentDto>> GetEnrollmentsForClassAsync(int classId, int pageNumber, int pageSize);
        Task<EnrollmentDto> ApproveEnrollmentAsync(int enrollmentId);
        Task<EnrollmentDto> RejectEnrollmentAsync(int enrollmentId);
        Task<EnrollmentDto> AddTraineeToClassAsync(InstructorAddTraineeDto dto);
        Task RemoveTraineeFromClassAsync(int enrollmentId);
        #endregion

        // After enrollment service, class service should have logic that when class is started, all enrollment will change status to Inprogress
    }
}
