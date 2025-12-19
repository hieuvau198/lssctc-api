using Lssctc.ProgramManagement.ClassManage.FinalExams.Dtos;

namespace Lssctc.ProgramManagement.ClassManage.FinalExams.Services
{
    public interface ISEService
    {
        Task EnsureSeTasksForSimulationAsync(int feSimulationId, int practiceId);
        Task<IEnumerable<SePracticeListDto>> GetMySimulationExamPartialsByClassAsync(int classId, int userId);
        Task<FinalExamPartialDto> ValidateSeCodeAndStartSimulationExamAsync(int partialId, string examCode, int userId);
        Task<FinalExamPartialDto> StartSimulationExamAsync(int partialId, int userId);
        Task<SeTaskDto> SubmitSeTaskByCodeAsync(int partialId, string taskCode, int userId, SubmitSeTaskDto dto);
        Task<FinalExamPartialDto> SubmitSeFinalAsync(int partialId, int userId, SubmitSeFinalDto dto);
        Task<FinalExamDto> SubmitSeAsync(int partialId, SubmitSeDto dto);
        Task<SimulationExamDetailDto> GetSimulationExamDetailAsync(int partialId);
        Task<IEnumerable<ClassSimulationResultDto>> GetClassSimulationResultsAsync(int classId);
    }
}