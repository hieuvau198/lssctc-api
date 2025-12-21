using Lssctc.ProgramManagement.ClassManage.FinalExams.Dtos;

namespace Lssctc.ProgramManagement.ClassManage.FinalExams.Services
{
    public interface IFEService
    {
        Task AutoCreateFinalExamsForClassAsync(int classId);
        Task<FinalExamDto> CreateFinalExamAsync(CreateFinalExamDto dto);
        Task<FinalExamDto> UpdateFinalExamAsync(int id, UpdateFinalExamDto dto);
        Task<FinalExamDto> GetFinalExamByIdAsync(int id);
        Task<IEnumerable<FinalExamDto>> GetFinalExamsByClassAsync(int classId);
        Task<FinalExamDto?> GetMyFinalExamByClassAsync(int classId, int userId);
        Task<IEnumerable<FinalExamDto>> GetFinalExamsByTraineeAsync(int traineeId);
        Task DeleteFinalExamAsync(int id);
        Task<string> GenerateExamCodeAsync(int fePartialId);
        Task FinishFinalExamAsync(int classId);
        Task RecalculateFinalExamScore(int finalExamId);
        Task StartClassExamAsync(int classId);
    }
}