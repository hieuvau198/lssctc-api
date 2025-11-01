using Lssctc.LearningManagement.LearningRecords.DTOs;
using Lssctc.Share.Common;

namespace Lssctc.LearningManagement.LearningRecords.Services
{
    public interface ILearningRecordService
    {
        Task<PagedResult<LearningRecordDto>> GetLearningRecords(int pageIndex, int pageSize);
        Task<IReadOnlyList<LearningRecordDto>> GetLearningRecordsNoPagination();
        Task<LearningRecordDto?> GetLearningRecordById(int id);
        Task<int> CreateLearningRecord(CreateLearningRecordDto dto);
        Task<bool> UpdateLearningRecord(int id, UpdateLearningRecordDto dto);
        Task<bool> DeleteLearningRecord(int id);
    }
}
