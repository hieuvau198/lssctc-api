using Lssctc.ProgramManagement.Learnings.LearningsSections.Dtos;
using Lssctc.Share.Common;

namespace Lssctc.ProgramManagement.Learnings.LearningsSections.Services
{
    public interface ILearningsSectionService
    {
        // get list all of LearningsSectionDto by classId and traineeId
        Task<List<LearningsSectionDto>> GetAllSectionsByClassIdAndTraineeId(int classId, int traineeId);
        // get paged list of LearningsSectionDto by classId and traineeId
        Task<PagedResult<LearningsSectionDto>> GetSectionsByClassIdAndTraineeIdPaged(int classId, int traineeId, int pageIndex, int pageSize);
        // get LearningsSectionDto by sectionId and traineeId
        Task<LearningsSectionDto> GetSectionBySectionIdAndTraineeId(int sectionId, int traineeId);
    }
}
