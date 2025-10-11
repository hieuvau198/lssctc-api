using Lssctc.SimulationManagement.TraineePractices.Dtos;

namespace Lssctc.SimulationManagement.TraineePractices.Services
{
    public interface ITraineePracticeService
    {
        Task<List<TraineePracticeDto>> GetTraineePracticesByTraineeIdAndClassId(int traineeId, int classId);
        Task<TraineePracticeDto?> GetTraineePracticeByIdA(int practiceId, int traineeId);

    }
}
