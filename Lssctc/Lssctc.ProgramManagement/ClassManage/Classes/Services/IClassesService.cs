using Lssctc.ProgramManagement.ClassManage.Classes.Dtos;
using Lssctc.Share.Common;

namespace Lssctc.ProgramManagement.ClassManage.Classes.Services
{
    public interface IClassesService
    {
        #region Classes
        Task<IEnumerable<ClassDto>> GetAllClassesAsync();
        Task<PagedResult<ClassDto>> GetClassesAsync(int pageNumber, int pageSize);
        Task<ClassDto?> GetClassByIdAsync(int id);
        // BR create class: with initial status 'Draft'
        // startdate must be from current time to
        // enddate must be after startdate, at least 3 day later
        // classcode cannot be duplicated
        Task<ClassDto> CreateClassAsync(CreateClassDto dto);
        // BR update class: allow for update only if class status is 'Draft'
        // startdate must be from current time to
        // enddate must be after startdate, at least 3 day later
        // classcode cannot be duplicated
        Task<ClassDto> UpdateClassAsync(int id, UpdateClassDto dto);
        // BR start class: allow for start only if class status is 'Draft'
        Task OpenClassAsync(int id);
        // BR start class: allow for start only if class status is 'Draft' or 'Open'
        // startdate and enddate must be valid
        // must have at least one instructor assigned
        // must have at least one enrolled student
        // after starting class, class status is 'In Progress'
        // after starting class, automatically create progress, section record, activity record for each enrolled student
        Task StartClassAsync(int id);
        Task CompleteClassAsync(int id);
        // BR cancel class: allow for delete only if no enrolled students,
        // class status is 'Draft' or 'Open'
        Task CancelClassAsync(int id);
        #endregion

        #region Program Course Classes
        // Todo: get classes by program id and course id
        // Todo: get classes by course id
        #endregion

        
    }
}
