using Lssctc.ProgramManagement.Learnings.Dtos;
using Lssctc.Share.Common;

namespace Lssctc.ProgramManagement.Learnings.Services
{
    public interface ILearningsClassService
    {
        // get paged list of LearningsClassDto by traineeId
        Task<PagedResult<LearningsClassDto>> GetClassesByTraineeIdPaged(int traineeId, int pageIndex, int pageSize);
        // get list all of LearningsClassDto by traineeId
        Task<List<LearningsClassDto>> GetAllClassesByTraineeId(int traineeId);
        // get LearningsClassDto by class id
        Task<LearningsClassDto> GetClassById(int classId);
    }
}
