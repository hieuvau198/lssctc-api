using Lssctc.ProgramManagement.Practices.Dtos;

namespace Lssctc.ProgramManagement.Practices.Services
{
    public interface ITraineePracticesService
    {
        Task<IEnumerable<TraineePracticeDto>> GetPracticesForTraineeAsync(int traineeId, int classId);
        Task<TraineePracticeResponseDto?> GetPracticeForTraineeByActivityIdAsync(int traineeId, int activityRecordId);
    }
}