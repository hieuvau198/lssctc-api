using Lssctc.ProgramManagement.Practices.Dtos;
using Lssctc.Share.Common;
using Lssctc.Share.Entities;
using Lssctc.Share.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Lssctc.ProgramManagement.Practices.Services
{
    public class TasksService : ITasksService
    {
        private readonly IUnitOfWork _uow;

        public TasksService(IUnitOfWork uow)
        {
            _uow = uow;
        }

        #region Tasks CRUD

        public async Task<IEnumerable<TaskDto>> GetAllTasksAsync()
        {
            var tasks = await _uow.SimTaskRepository
                .GetAllAsQueryable()
                .Where(t => t.IsDeleted == null || t.IsDeleted == false)
                .ToListAsync();

            return tasks.Select(MapToDto);
        }

        public async Task<PagedResult<TaskDto>> GetTasksAsync(int pageNumber, int pageSize)
        {
            if (pageNumber < 1) pageNumber = 1;
            if (pageSize < 1) pageSize = 10;

            var query = _uow.SimTaskRepository
                .GetAllAsQueryable()
                .Where(t => t.IsDeleted == null || t.IsDeleted == false)
                .Select(t => MapToDto(t));

            return await query.ToPagedResultAsync(pageNumber, pageSize);
        }

        public async Task<TaskDto?> GetTaskByIdAsync(int id)
        {
            var task = await _uow.SimTaskRepository.GetByIdAsync(id);
            if (task == null || task.IsDeleted == true)
                return null;

            return MapToDto(task);
        }

        public async Task<TaskDto> CreateTaskAsync(CreateTaskDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.TaskName))
                throw new ArgumentException("Task name is required.");

            var task = new SimTask
            {
                TaskName = dto.TaskName.Trim(),
                TaskCode = dto.TaskCode?.Trim(), // Added
                TaskDescription = dto.TaskDescription?.Trim(),
                ExpectedResult = dto.ExpectedResult?.Trim(),
                IsDeleted = false
            };

            await _uow.SimTaskRepository.CreateAsync(task);
            await _uow.SaveChangesAsync();

            return MapToDto(task);
        }

        public async Task<TaskDto> UpdateTaskAsync(int id, UpdateTaskDto dto)
        {
            var task = await _uow.SimTaskRepository.GetByIdAsync(id);
            if (task == null || task.IsDeleted == true)
                throw new KeyNotFoundException($"Task with ID {id} not found.");

            task.TaskName = dto.TaskName?.Trim() ?? task.TaskName;
            task.TaskCode = dto.TaskCode?.Trim() ?? task.TaskCode; // Added
            task.TaskDescription = dto.TaskDescription?.Trim() ?? task.TaskDescription;
            task.ExpectedResult = dto.ExpectedResult?.Trim() ?? task.ExpectedResult;

            await _uow.SimTaskRepository.UpdateAsync(task);
            await _uow.SaveChangesAsync();

            return MapToDto(task);
        }

        public async Task DeleteTaskAsync(int id)
        {
            var task = await _uow.SimTaskRepository
                .GetAllAsQueryable()
                .Include(t => t.PracticeTasks)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (task == null)
                throw new KeyNotFoundException($"Task with ID {id} not found.");

            if (task.PracticeTasks != null && task.PracticeTasks.Any())
                throw new InvalidOperationException("Cannot delete a task associated with one or more practices.");

            // Soft delete only
            task.IsDeleted = true;

            await _uow.SimTaskRepository.UpdateAsync(task);
            await _uow.SaveChangesAsync();
        }


        #endregion

        #region Practice Tasks

        public async Task<IEnumerable<TaskDto>> GetTasksByPracticeAsync(int practiceId)
        {
            var tasks = await _uow.PracticeTaskRepository
                .GetAllAsQueryable()
                .Include(pt => pt.Task)
                .Where(pt => pt.PracticeId == practiceId)
                .Select(pt => pt.Task)
                .Where(t => t.IsDeleted == null || t.IsDeleted == false)
                .ToListAsync();

            return tasks.Select(t => MapToDto(t));
        }

        public async Task AddTaskToPracticeAsync(int practiceId, int taskId)
        {
            var practice = await _uow.PracticeRepository
                .GetAllAsQueryable()
                .Include(p => p.PracticeTasks)
                .FirstOrDefaultAsync(p => p.Id == practiceId);

            var task = await _uow.SimTaskRepository.GetByIdAsync(taskId);

            if (practice == null)
                throw new KeyNotFoundException($"Practice with ID {practiceId} not found.");

            if (task == null)
                throw new KeyNotFoundException($"Task with ID {taskId} not found.");

            bool exists = await _uow.PracticeTaskRepository
                .GetAllAsQueryable()
                .AnyAsync(pt => pt.PracticeId == practiceId && pt.TaskId == taskId);

            if (exists)
                throw new InvalidOperationException("This task is already linked to the practice.");

            var link = new PracticeTask
            {
                PracticeId = practiceId,
                TaskId = taskId,
                Status = 1
            };

            await _uow.PracticeTaskRepository.CreateAsync(link);
            await _uow.SaveChangesAsync();
        }

        public async Task RemoveTaskFromPracticeAsync(int practiceId, int taskId)
        {
            var link = await _uow.PracticeTaskRepository
                .GetAllAsQueryable()
                .FirstOrDefaultAsync(pt => pt.PracticeId == practiceId && pt.TaskId == taskId);

            if (link == null)
                throw new KeyNotFoundException("Task is not linked to this practice.");

            await _uow.PracticeTaskRepository.DeleteAsync(link);
            await _uow.SaveChangesAsync();
        }

        #endregion

        #region Mapping Helpers

        private static TaskDto MapToDto(SimTask t)
        {
            return new TaskDto
            {
                Id = t.Id,
                TaskName = t.TaskName,
                TaskCode = t.TaskCode, // Added
                TaskDescription = t.TaskDescription,
                ExpectedResult = t.ExpectedResult
            };
        }

        #endregion
    }
}