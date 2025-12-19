using Lssctc.ProgramManagement.ClassManage.FinalExams.Dtos;

namespace Lssctc.ProgramManagement.ClassManage.FinalExams.Services
{
    public interface IFEPartialService
    {
        #region Partials
        Task<FinalExamPartialDto> GetFinalExamPartialByIdAsync(int id);
        Task<FinalExamPartialDto> CreateFinalExamPartialAsync(CreateFinalExamPartialDto dto);
        Task<IEnumerable<FinalExamDto>> CreatePartialsForClassAsync(CreateClassPartialDto dto);
        Task<FinalExamPartialDto> UpdateFinalExamPartialAsync(int id, UpdateFinalExamPartialDto dto);
        Task<FinalExamDto> AllowPartialRetakeAsync(int partialId, string? note = null);
        Task<FinalExamPartialDto> GetFinalExamPartialByIdForTraineeAsync(int partialId, int userId);
        Task DeleteFinalExamPartialAsync(int id);
        #endregion

        #region TE & PE
        Task<object> GetTeQuizContentAsync(int partialId, string examCode, int userId);
        Task<FinalExamDto> SubmitTeAsync(int partialId, int userId, SubmitTeDto dto);
        Task<FinalExamDto> SubmitPeAsync(int partialId, SubmitPeDto dto);
        Task<List<PeChecklistItemDto>> GetPeSubmissionChecklistForTraineeAsync(int partialId, int userId);
        #endregion
    }
}