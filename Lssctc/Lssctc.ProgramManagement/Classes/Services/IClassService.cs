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
        Task<InstructorDto> GetInstructorByClassIdAsync(int classId);

        // Training Progress
        Task<List<TrainingProgressDto>> GetTrainingProgressByClassMemberAsync(int classMemberId);
        Task<TrainingProgressDto> AddTrainingProgressAsync(CreateTrainingProgressDto dto);
        Task<TrainingProgressDto> UpdateTrainingProgressAsync(UpdateTrainingProgressDto dto);
        Task<bool> DeleteTrainingProgressAsync(int progressId);

        // Training Result
        Task<List<TrainingResultDto>> GetTrainingResultsByProgressAsync(int trainingProgressId);
        Task<TrainingResultDto> AddTrainingResultAsync(CreateTrainingResultDto dto);
        Task<TrainingResultDto> UpdateTrainingResultAsync(UpdateTrainingResultDto dto);
        Task<bool> DeleteTrainingResultAsync(int resultId);

        
    }
}
