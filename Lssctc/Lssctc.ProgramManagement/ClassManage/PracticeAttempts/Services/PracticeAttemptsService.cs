using Lssctc.Share.Interfaces;
using Lssctc.ProgramManagement.ClassManage.PracticeAttempts.Dtos;
using Microsoft.EntityFrameworkCore;
using Lssctc.Share.Entities;

namespace Lssctc.ProgramManagement.ClassManage.PracticeAttempts.Services
{
    public class PracticeAttemptsService : IPracticeAttemptsService
    {
        private readonly IUnitOfWork _uow;
        
        public PracticeAttemptsService(IUnitOfWork uow)
        {
            _uow = uow;
        }

        public async Task<IEnumerable<PracticeAttemptDto>> GetPracticeAttempts(int traineeId, int activityRecordId)
        {
            var activityRecord = await _uow.ActivityRecordRepository
                .GetAllAsQueryable()
                .Include(ar => ar.SectionRecord.LearningProgress.Enrollment)
                .AsNoTracking()
                .FirstOrDefaultAsync(ar => ar.Id == activityRecordId);

            if (activityRecord == null)
                throw new KeyNotFoundException("Activity record not found.");

            // Verify trainee ownership
            if (activityRecord.SectionRecord.LearningProgress.Enrollment.TraineeId != traineeId)
                throw new KeyNotFoundException("Activity record not found for this trainee.");

            var attempts = await _uow.PracticeAttemptRepository
                .GetAllAsQueryable()
                .AsNoTracking()
                .Include(pa => pa.PracticeAttemptTasks)
                .Where(pa => pa.ActivityRecordId == activityRecordId)
                .OrderByDescending(pa => pa.AttemptDate)
                .ToListAsync();

            return attempts.Select(MapToDto);
        }

        public async Task<PracticeAttemptDto?> GetLatestPracticeAttempt(int traineeId, int activityRecordId)
        {
            var activityRecord = await _uow.ActivityRecordRepository
                .GetAllAsQueryable()
                .Include(ar => ar.SectionRecord.LearningProgress.Enrollment)
                .AsNoTracking()
                .FirstOrDefaultAsync(ar => ar.Id == activityRecordId);

            if (activityRecord == null)
                throw new KeyNotFoundException("Activity record not found.");

            // Verify trainee ownership
            if (activityRecord.SectionRecord.LearningProgress.Enrollment.TraineeId != traineeId)
                throw new KeyNotFoundException("Activity record not found for this trainee.");

            var attempt = await _uow.PracticeAttemptRepository
                .GetAllAsQueryable()
                .AsNoTracking()
                .Include(pa => pa.PracticeAttemptTasks)
                .Where(pa => pa.ActivityRecordId == activityRecordId && pa.IsCurrent)
                .FirstOrDefaultAsync();

            return attempt == null ? null : MapToDto(attempt);
        }

        #region Mapping Methods
        
        private static PracticeAttemptDto MapToDto(PracticeAttempt pa)
        {
            return new PracticeAttemptDto
            {
                Id = pa.Id,
                ActivityRecordId = pa.ActivityRecordId,
                PracticeId = pa.PracticeId,
                Score = pa.Score,
                AttemptDate = pa.AttemptDate,
                AttemptStatus = pa.AttemptStatus?.ToString() ?? "Unknown",
                Description = pa.Description,
                IsPass = pa.IsPass,
                IsCurrent = pa.IsCurrent,
                PracticeAttemptTasks = pa.PracticeAttemptTasks.Select(MapToTaskDto).ToList()
            };
        }

        private static PracticeAttemptTaskDto MapToTaskDto(PracticeAttemptTask pat)
        {
            return new PracticeAttemptTaskDto
            {
                Id = pat.Id,
                PracticeAttemptId = pat.PracticeAttemptId,
                TaskId = pat.TaskId,
                Score = pat.Score,
                Description = pat.Description,
                IsPass = pat.IsPass
            };
        }

        #endregion
    }
}
