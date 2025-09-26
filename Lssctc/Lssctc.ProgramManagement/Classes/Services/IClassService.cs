using Lssctc.ProgramManagement.Classes.DTOs;
using Lssctc.Share.Common;


namespace Lssctc.ProgramManagement.Classes.Services
{
    public interface IClassService
    {
        // using string for ClassCode instead of int ClassCodeId
        Task<PagedResult<ClassDto>> GetAllClassesAsync(int page = 1, int pageSize = 10);
        Task<List<ClassDto>> GetClassesByProgramCourseIdAsync(int programCourseId);
        Task<ClassDto> CreateClassByProgramCourseId(ClassCreateDto dto);
        Task<ClassDto> AssignInstructorToClass(AssignInstructorDto dto);
        //Task<ClassDto?> AssignTraineeAsync(AssignTraineeDto dto);
        Task<ClassEnrollmentDto> GetClassEnrollmentById(int classid);
        Task<ClassEnrollmentDto> EnrollTrainee(ClassEnrollmentCreateDto dto);
        Task<ClassMemberDto> ApproveEnrollment(ApproveEnrollmentDto dto);
        Task<IEnumerable<ClassMemberDto>> GetMembersByClassId(int classId);
        Task<InstructorDto> GetInstructorByClassId(int classId);
        // Training Progress
        Task<List<TrainingProgressDto>> GetProgressByMember(int classMemberId);
        Task<TrainingProgressDto> CreateProgress(CreateTrainingProgressDto dto);
        Task<TrainingProgressDto> UpdateProgress(UpdateTrainingProgressDto dto);
        Task<bool> DeleteProgress(int progressId);
        // Training Result
        Task<List<TrainingResultDto>> GetResultsByProgress(int trainingProgressId);
        Task<TrainingResultDto> CreateResult(CreateTrainingResultDto dto);
        Task<TrainingResultDto> UpdateResult(UpdateTrainingResultDto dto);
        Task<bool> DeleteResult(int resultId);
        //section
        Task<SectionDto> CreateSectionAsync(SectionCreateDto dto);
        Task<SyllabusSectionDto> CreateSyllabusSectionAsync(SyllabusSectionCreateDto dto);


    }
}
