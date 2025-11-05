using Lssctc.ProgramManagement.ClassManage.SectionRecords.Dtos;
using Lssctc.Share.Common;

namespace Lssctc.ProgramManagement.ClassManage.SectionRecords.Services
{
    public interface ISectionRecordsService
    {
        // BR: Trainee can only view their own section records in a class they are enrolled in

        Task<IEnumerable<SectionRecordDto>> GetSectionRecordsAsync(int classId, int traineeId);
        Task<PagedResult<SectionRecordDto>> GetSectionRecordsPagedAsync(int classId, int traineeId, int pageNumber, int pageSize);
        Task<IEnumerable<SectionRecordDto>> GetSectionRecordsBySectionAsync(int classId, int sectionId);
        Task<PagedResult<SectionRecordDto>> GetSectionRecordsBySectionPagedAsync(int classId, int sectionId, int pageNumber, int pageSize);
    }
}
