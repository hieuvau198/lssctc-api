using Lssctc.ProgramManagement.Practices.Dtos;
using Lssctc.Share.Common;

namespace Lssctc.ProgramManagement.Practices.Services
{
    public interface ITasksService
    {
        #region Tasks
        Task<IEnumerable<TaskDto>> GetAllTasksAsync();
        Task<PagedResult<TaskDto>> GetTasksAsync(int pageNumber, int pageSize);
        Task<TaskDto?> GetTaskByIdAsync(int id);
        Task<TaskDto> CreateTaskAsync(CreateTaskDto dto);
        Task<TaskDto> UpdateTaskAsync(int id, UpdateTaskDto dto);
        // BR: allow soft delete only
        // BR: a task cannot be deleted if it is associated with any practice
        Task DeleteTaskAsync(int id);
        #endregion

        #region Practice Tasks
        // BR: a practice can have multiple tasks, a task can belong to multiple practices
        Task<IEnumerable<TaskDto>> GetTasksByPracticeAsync(int practiceId);
        Task AddTaskToPracticeAsync(int practiceId, int taskId);
        Task RemoveTaskFromPracticeAsync(int practiceId, int taskId);
        #endregion
    }
}
