using Lssctc.ProgramManagement.ClassManage.Enrollments.Dtos;
using Lssctc.Share.Common;
using Lssctc.Share.Entities;

namespace Lssctc.ProgramManagement.ClassManage.Enrollments.Services
{
    public interface IEnrollmentsService
    {
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
        Task UpdateEnrollmentAsync(Enrollment enrollment);
        Task UpdateEnrollmentsAsync(IEnumerable<Enrollment> enrollments);
        #endregion
    }
}
