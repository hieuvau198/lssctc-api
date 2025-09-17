using Lssctc.ProgramManagement.Classes.DTOs;


namespace Lssctc.ProgramManagement.Classes.Services
{
    public interface IClassService
    {
        Task<ClassDto> CreateClassAsync(ClassCreateDto dto);
        Task<ClassDto> AssignInstructorAsync(AssignInstructorDto dto);
        Task<ClassDto?> AssignTraineeAsync(AssignTraineeDto dto);
    }
}
