using Lssctc.ProgramManagement.ClassManage.FinalExams.Dtos;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Lssctc.ProgramManagement.ClassManage.FinalExams.Services
{
    public interface IFinalExamsService
    {
        // 1. Auto Create
        Task AutoCreateFinalExamsForClassAsync(int classId);

        // 2. Final Exam CRUD (Admin/Instructor)
        Task<FinalExamDto> CreateFinalExamAsync(CreateFinalExamDto dto);
        Task<FinalExamDto> UpdateFinalExamAsync(int id, UpdateFinalExamDto dto); // Đã thêm

        // Get Methods
        Task<FinalExamDto> GetFinalExamByIdAsync(int id);
        Task<IEnumerable<FinalExamDto>> GetFinalExamsByClassAsync(int classId);
        Task<FinalExamDto?> GetMyFinalExamByClassAsync(int classId, int userId);
        Task<IEnumerable<FinalExamDto>> GetFinalExamsByTraineeAsync(int traineeId);
        Task<FinalExamPartialDto> GetFinalExamPartialByIdAsync(int id);
        Task<ClassExamConfigDto> GetClassExamConfigAsync(int classId);

        Task DeleteFinalExamAsync(int id);

        // Exam Code & Finish (Admin/Instructor)
        Task<string> GenerateExamCodeAsync(int finalExamId);
        Task FinishFinalExamAsync(int classId);

        // 3. Partials CRUD & Config (Admin/Instructor)
        Task<FinalExamPartialDto> CreateFinalExamPartialAsync(CreateFinalExamPartialDto dto);
        Task<IEnumerable<FinalExamDto>> CreatePartialsForClassAsync(CreateClassPartialDto dto);
        Task UpdatePartialsConfigForClassAsync(UpdateClassPartialConfigDto dto); // Đã thêm (Bulk Update)
        Task<FinalExamPartialDto> UpdateFinalExamPartialAsync(int id, UpdateFinalExamPartialDto dto);
        Task DeleteFinalExamPartialAsync(int id);

        // 4. Trainee View Specific
        Task<FinalExamPartialDto> GetFinalExamPartialByIdForTraineeAsync(int partialId, int userId); // Đã thêm
        Task<List<PeChecklistItemDto>> GetPeSubmissionChecklistForTraineeAsync(int partialId, int userId); // Đã thêm

        // 5. Allow Retake (Admin/Instructor)
        Task<FinalExamDto> AllowPartialRetakeAsync(int partialId, string? note = null);

        // TE Logic (Sử dụng Quiz Service)
        Task<object> GetTeQuizContentAsync(int partialId, string examCode, int userId); // Đã sửa signature
        Task<FinalExamDto> SubmitTeAsync(int partialId, int userId, SubmitTeDto dto);

        // SE Logic
        Task<IEnumerable<SePracticeListDto>> GetMySimulationExamPartialsByClassAsync(int classId, int userId);
        Task<FinalExamPartialDto> ValidateSeCodeAndStartSimulationExamAsync(int partialId, string examCode, int userId);
        Task<FinalExamPartialDto> SubmitSeFinalAsync(int partialId, int userId, SubmitSeFinalDto dto);
        Task<SeTaskDto> SubmitSeTaskByCodeAsync(int partialId, string taskCode, int userId, SubmitSeTaskDto dto);
        Task<FinalExamPartialDto> StartSimulationExamAsync(int partialId, int userId); // Đã thêm

        Task<FinalExamDto> SubmitSeAsync(int partialId, SubmitSeDto dto);

        // PE Logic
        Task<FinalExamDto> SubmitPeAsync(int partialId, SubmitPeDto dto);
        #region New for SE
        Task<SimulationExamDetailDto> GetSimulationExamDetailAsync(int partialId);
        Task<IEnumerable<ClassSimulationResultDto>> GetClassSimulationResultsAsync(int classId);
        #endregion
    }
}