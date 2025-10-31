using Lssctc.LearningManagement.TraineePractices.Dtos;

namespace Lssctc.LearningManagement.TraineePractices.Services
{
    public interface ITraineePracticeService
    {
        Task<List<TraineePracticeDto>> GetTraineePracticesByTraineeIdAndClassId(int traineeId, int classId);
        Task<TraineePracticeDto?> GetTraineePracticeByIdA(int practiceId, int traineeId);

    }
}
