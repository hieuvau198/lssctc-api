using Lssctc.ProgramManagement.Classes.DTOs;


namespace Lssctc.ProgramManagement.Classes.Services
{
    public interface IClassService
    {
        Task<ClassDto> CreateClassAsync(ClassCreateDto dto);
        Task<ClassDto> AssignInstructorAsync(AssignInstructorDto dto);
        //Task<ClassDto?> AssignTraineeAsync(AssignTraineeDto dto);
        Task<ClassEnrollmentDto> GetClassEnrollmentById(int classid);
        Task<ClassEnrollmentDto> EnrollTraineeAsync(ClassEnrollmentCreateDto dto);
        Task<ClassMemberDto> ApproveEnrollmentAsync(ApproveEnrollmentDto dto);
        Task<IEnumerable<ClassMemberDto>> GetClassMembersByClassIdAsync(int classId);
    }
}
