using Lssctc.ProgramManagement.ClassManage.Progresses.Dtos;

namespace Lssctc.ProgramManagement.ClassManage.Progresses.Services
{
    public interface IProgressesService
    {
        // get progress by class id and trainee id
        // get all progresses by class id
        // get all progresses by trainee id
        // get progress by progress id
        // create progress for by class id and trainee id
        // create progress by enrollment id
        // create progresses for all trainees in a class, by class id (check existing progresses first)
        // update progress by progress id
        // update progress percentage automatically, calculate based on completed section records
        // start progress by progress id (only if class is started, only if progress has status NotStarted)
        // complete progress by progress id (only if class is completed, only if progress has status InProgress)
        // fail progress by progress id (only if class is completed, only if progress has status InProgress)
        // start all progresses in a class, by class id (only if class is started, and for progress that has status NotStarted)
        // complete all progresses in a class, by class id (only if class is completed, and for progress that has status InProgress)
        
        // === Get ===
        Task<ProgressDto?> GetProgressByClassAndTraineeAsync(int classId, int traineeId);
        Task<IEnumerable<ProgressDto>> GetAllProgressesByClassIdAsync(int classId);
        Task<IEnumerable<ProgressDto>> GetAllProgressesByTraineeIdAsync(int traineeId);
        Task<ProgressDto?> GetProgressByIdAsync(int progressId);

        // === Create ===
        Task<ProgressDto> CreateProgressAsync(CreateProgressDto dto);
        Task<ProgressDto> CreateProgressForTraineeAsync(int classId, int traineeId);
        Task<IEnumerable<ProgressDto>> CreateProgressesForClassAsync(int classId);

        // === Update ===
        Task<ProgressDto> UpdateProgressAsync(int progressId, UpdateProgressDto dto);
        Task<ProgressDto> UpdateProgressPercentageAsync(int progressId);

        // === Status Changes ===
        Task<ProgressDto> StartProgressAsync(int progressId);
        Task<ProgressDto> CompleteProgressAsync(int progressId);
        Task<ProgressDto> FailProgressAsync(int progressId);
        Task<int> StartAllProgressesAsync(int classId); // Returns count of started progresses
        Task<int> CompleteAllProgressesAsync(int classId); // Returns count of completed progresses
    }
}
