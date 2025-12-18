using Lssctc.ProgramManagement.ClassManage.FinalExams.Dtos;

namespace Lssctc.ProgramManagement.ClassManage.FinalExams.Services
{
    public interface IFinalExamPartialService
    {
        Task<FinalExamPartialDto> GetFinalExamPartialByIdAsync(int id);
        Task<FinalExamPartialDto> CreateFinalExamPartialAsync(CreateFinalExamPartialDto dto);
        Task<IEnumerable<FinalExamDto>> CreatePartialsForClassAsync(CreateClassPartialDto dto);
        Task UpdatePartialsConfigForClassAsync(UpdateClassPartialConfigDto dto);
        Task<FinalExamPartialDto> UpdateFinalExamPartialAsync(int id, UpdateFinalExamPartialDto dto);
        Task DeleteFinalExamPartialAsync(int id);
        Task<FinalExamDto> AllowPartialRetakeAsync(int partialId, string? note = null);
        Task<object> GetTeQuizContentAsync(int partialId, string examCode, int userId);
        Task<FinalExamDto> SubmitTeAsync(int partialId, int userId, SubmitTeDto dto);
        Task<FinalExamDto> SubmitPeAsync(int partialId, SubmitPeDto dto);
        Task<FinalExamPartialDto> GetFinalExamPartialByIdForTraineeAsync(int partialId, int userId);
        Task<List<PeChecklistItemDto>> GetPeSubmissionChecklistForTraineeAsync(int partialId, int userId);
        Task<ClassExamConfigDto> GetClassExamConfigAsync(int classId);
    }
}