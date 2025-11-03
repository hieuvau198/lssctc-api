using Lssctc.ProgramManagement.Classes.Dtos;
using Lssctc.Share.Common;

namespace Lssctc.ProgramManagement.Classes.Services
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
        Task StartClassAsync(int id);
        Task CompleteClassAsync(int id);
        // BR cancel class: allow for delete only if no enrolled students,
        // class status is 'Draft' or 'Open'
        Task CancelClassAsync(int id);
        #endregion

        #region Program Course Classes

        #endregion


    }
}
