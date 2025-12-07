using Lssctc.ProgramManagement.ClassManage.FinalExams.Dtos;

namespace Lssctc.ProgramManagement.ClassManage.FinalExams.Services
{
    public interface IFinalExamsService
    {
        // 1. Auto Create when class starts
        Task AutoCreateFinalExamsForClassAsync(int classId);

        // 2. Final Exam CRUD
        Task<FinalExamDto> CreateFinalExamAsync(CreateFinalExamDto dto);
        Task<FinalExamDto> GetFinalExamByIdAsync(int id);
        Task<IEnumerable<FinalExamDto>> GetFinalExamsByClassAsync(int classId);
        Task<FinalExamDto?> GetFinalExamByEnrollmentAsync(int enrollmentId);
        Task<IEnumerable<FinalExamDto>> GetFinalExamsByTraineeAsync(int traineeId);
        Task DeleteFinalExamAsync(int id);

        // 3. Partials CRUD (Create TE/SE/PE configurations)
        Task<FinalExamPartialDto> CreateFinalExamPartialAsync(CreateFinalExamPartialDto dto);
        Task<FinalExamPartialDto> UpdateFinalExamPartialAsync(int id, UpdateFinalExamPartialDto dto);
        Task DeleteFinalExamPartialAsync(int id);

        // 4. Practical Exam Checklist Helper
        List<string> GetPeChecklistCriteria();

        // 5. Submissions (Calculate scores & weights)
        Task<FinalExamDto> SubmitTeAsync(int partialId, SubmitTeDto dto);
        Task<FinalExamDto> SubmitSeAsync(int partialId, SubmitSeDto dto);
        Task<FinalExamDto> SubmitPeAsync(int partialId, SubmitPeDto dto);
    }
}