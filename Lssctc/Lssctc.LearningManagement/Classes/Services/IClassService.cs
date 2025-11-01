using Lssctc.LearningManagement.Classes.DTOs;
using Lssctc.Share.Common;


namespace Lssctc.LearningManagement.Classes.Services
{
    public interface IClassService
    {
        // Class - get info
        Task<List<ClassDto>> GetAllClasses();
        Task<PagedResult<ClassDto>> GetClasses(int page = 1, int pageSize = 10);
        Task<List<ClassDto>> GetClassesByProgramCourse(int programCourseId);
        Task<ClassDto> GetClassDetailById(int classId);
        Task<ClassEnrollmentDto> GetClassEnrollmentById(int classid);
        Task<PagedResult<ClassMemberDto>> GetMembersByClassId(
    int classId, int page, int pageSize);
        Task<InstructorDto> GetInstructorByClassId(int classId);
        Task<List<MyClassDto>> GetMyClasses(int traineeId);
        // Class - manage
        Task<ClassDto> CreateClassByProgramCourse(ClassCreateDto dto);
        Task<ClassDto> UpdateClassBasicInfoAsync(int classId, ClassUpdateDto dto);
        Task<bool> CancelClassAsync(int classId);
        Task<ClassDto> AssignInstructorToClass(AssignInstructorDto dto);
        Task<ClassEnrollmentDto> EnrollTrainee(ClassEnrollmentCreateDto dto);
        Task<ClassMemberDto> ApproveEnrollment(ApproveEnrollmentDto dto);
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
        // Section
        Task<SectionDto> CreateSectionAsync(SectionCreateDto dto);
        Task<SyllabusSectionDto> CreateSyllabusSectionAsync(SyllabusSectionCreateDto dto);

        //get class by intructor id
        Task<PagedResult<ClassBasicDto>> GetClassesByInstructorId(int instructorId, int page, int pageSize, int? status);

    }
}
