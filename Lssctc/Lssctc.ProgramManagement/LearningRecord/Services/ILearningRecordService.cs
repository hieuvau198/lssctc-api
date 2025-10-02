using Lssctc.ProgramManagement.LearningRecord.DTOs;
using Lssctc.Share.Common;

namespace Lssctc.ProgramManagement.LearningRecord.Services
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
