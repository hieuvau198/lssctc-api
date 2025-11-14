using Lssctc.ProgramManagement.ClassManage.Helpers;
using Lssctc.ProgramManagement.ClassManage.Progresses.Dtos;
using Lssctc.Share.Entities;
using Lssctc.Share.Enums;
using Lssctc.Share.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Lssctc.ProgramManagement.ClassManage.Progresses.Services
{
    public class ProgressesService : IProgressesService
    {
        private readonly IUnitOfWork _uow;
        private readonly ClassManageHandler _classHandler;

        public ProgressesService(IUnitOfWork uow)
        {
            _uow = uow;
            _classHandler = new ClassManageHandler(uow);
        }

        #region Get Methods

        public async Task<ProgressDto?> GetProgressByIdAsync(int progressId)
        {
            var progress = await GetProgressQuery()
                .FirstOrDefaultAsync(lp => lp.Id == progressId);

            return progress == null ? null : MapToDto(progress);
        }

        public async Task<ProgressDto?> GetProgressByClassAndTraineeAsync(int classId, int traineeId)
        {
            var progress = await GetProgressQuery()
                .FirstOrDefaultAsync(lp => lp.Enrollment.ClassId == classId && lp.Enrollment.TraineeId == traineeId);

            return progress == null ? null : MapToDto(progress);
        }

        public async Task<IEnumerable<ProgressDto>> GetAllProgressesByClassIdAsync(int classId)
        {
            var progresses = await GetProgressQuery()
                .Where(lp => lp.Enrollment.ClassId == classId)
                .ToListAsync();

            return progresses.Select(MapToDto);
        }

        public async Task<IEnumerable<ProgressDto>> GetAllProgressesByTraineeIdAsync(int traineeId)
        {
            var progresses = await GetProgressQuery()
                .Where(lp => lp.Enrollment.TraineeId == traineeId)
                .ToListAsync();

            return progresses.Select(MapToDto);
        }

        #endregion

        #region Create Methods

        public async Task<ProgressDto> CreateProgressAsync(CreateProgressDto dto)
        {
            var enrollment = await _uow.EnrollmentRepository
                .GetAllAsQueryable()
                .FirstOrDefaultAsync(e => e.Id == dto.EnrollmentId);

            if (enrollment == null)
                throw new KeyNotFoundException($"Enrollment with ID {dto.EnrollmentId} not found.");

            await _classHandler.EnsureProgressScaffoldingForTraineeAsync(enrollment.ClassId, enrollment.TraineeId);

            var progress = await GetProgressByClassAndTraineeAsync(enrollment.ClassId, enrollment.TraineeId);

            if (progress == null)
                throw new InvalidOperationException("Failed to create or find learning progress after scaffolding.");

            return progress;
        }
        public async Task<ProgressDto> CreateProgressForTraineeAsync(int classId, int traineeId)
        {
            await _classHandler.EnsureProgressScaffoldingForTraineeAsync(classId, traineeId);

            var progress = await GetProgressByClassAndTraineeAsync(classId, traineeId);

            if (progress == null)
                throw new InvalidOperationException("Failed to create or find learning progress after scaffolding.");

            return progress;
        }

        public async Task<IEnumerable<ProgressDto>> CreateProgressesForClassAsync(int classId)
        {
            await _classHandler.EnsureProgressScaffoldingForClassAsync(classId);

            return await GetAllProgressesByClassIdAsync(classId);
        }
        #endregion

        #region Update Methods

        public async Task<ProgressDto> UpdateProgressAsync(int progressId, UpdateProgressDto dto)
        {
            var progress = await _uow.LearningProgressRepository.GetByIdAsync(progressId);
            if (progress == null)
                throw new KeyNotFoundException($"LearningProgress with ID {progressId} not found.");

            progress.TheoryScore = dto.TheoryScore ?? progress.TheoryScore;
            progress.PracticalScore = dto.PracticalScore ?? progress.PracticalScore;
            progress.FinalScore = dto.FinalScore ?? progress.FinalScore;
            progress.Name = dto.Name ?? progress.Name;
            progress.Description = dto.Description ?? progress.Description;
            progress.LastUpdated = DateTime.UtcNow;

            await _uow.LearningProgressRepository.UpdateAsync(progress);
            await _uow.SaveChangesAsync();

            return MapToDto(progress); 
        }

        public async Task<ProgressDto> UpdateProgressPercentageAsync(int progressId)
        {
            var progress = await _uow.LearningProgressRepository
                .GetAllAsQueryable()
                .Include(lp => lp.SectionRecords)
                .FirstOrDefaultAsync(lp => lp.Id == progressId);

            if (progress == null)
                throw new KeyNotFoundException($"LearningProgress with ID {progressId} not found.");

            int totalSections = progress.SectionRecords.Count;
            if (totalSections == 0)
            {
                progress.ProgressPercentage = 0;
            }
            else
            {
                int completedSections = progress.SectionRecords.Count(sr => sr.IsCompleted);
                progress.ProgressPercentage = ((decimal)completedSections / totalSections) * 100;
            }

            // Auto-complete if 100% and not already failed
            if (progress.ProgressPercentage == 100 &&
                progress.Status != (int)LearningProgressStatusEnum.Failed &&
                progress.Status != (int)LearningProgressStatusEnum.Completed)
            {
                progress.Status = (int)LearningProgressStatusEnum.Completed;
            }

            progress.LastUpdated = DateTime.UtcNow;
            await _uow.LearningProgressRepository.UpdateAsync(progress);
            await _uow.SaveChangesAsync();

            return MapToDto(progress);
        }

        #endregion

        #region Status Change Methods

        public async Task<ProgressDto> StartProgressAsync(int progressId)
        {
            var progress = await _uow.LearningProgressRepository
                .GetAllAsQueryable()
                .Include(lp => lp.Enrollment.Class) // Need class to check status
                .FirstOrDefaultAsync(lp => lp.Id == progressId);

            if (progress == null)
                throw new KeyNotFoundException($"LearningProgress with ID {progressId} not found.");

            if (progress.Enrollment.Class.Status != (int)ClassStatusEnum.Inprogress)
                throw new InvalidOperationException("Cannot start progress: Class is not 'Inprogress'.");

            if (progress.Status != (int)LearningProgressStatusEnum.NotStarted)
                throw new InvalidOperationException("Progress has already started or is completed/failed.");

            progress.Status = (int)LearningProgressStatusEnum.InProgress;
            progress.LastUpdated = DateTime.UtcNow;
            await _uow.LearningProgressRepository.UpdateAsync(progress);
            await _uow.SaveChangesAsync();

            return MapToDto(progress);
        }

        public async Task<ProgressDto> CompleteProgressAsync(int progressId)
        {
            var progress = await _uow.LearningProgressRepository
                .GetAllAsQueryable()
                .Include(lp => lp.Enrollment.Class)
                .FirstOrDefaultAsync(lp => lp.Id == progressId);

            if (progress == null)
                throw new KeyNotFoundException($"LearningProgress with ID {progressId} not found.");

            // BR: only if class is completed
            if (progress.Enrollment.Class.Status != (int)ClassStatusEnum.Completed)
                throw new InvalidOperationException("Cannot complete progress: Class is not 'Completed'.");

            // BR: only if progress has status InProgress
            if (progress.Status != (int)LearningProgressStatusEnum.InProgress)
                throw new InvalidOperationException("Only 'InProgress' progress can be manually completed.");

            progress.Status = (int)LearningProgressStatusEnum.Completed;
            progress.ProgressPercentage = 100; // Assume manual complete means 100%
            progress.LastUpdated = DateTime.UtcNow;
            await _uow.LearningProgressRepository.UpdateAsync(progress);
            await _uow.SaveChangesAsync();

            return MapToDto(progress);
        }

        public async Task<ProgressDto> FailProgressAsync(int progressId)
        {
            var progress = await _uow.LearningProgressRepository
                .GetAllAsQueryable()
                .Include(lp => lp.Enrollment.Class)
                .FirstOrDefaultAsync(lp => lp.Id == progressId);

            if (progress == null)
                throw new KeyNotFoundException($"LearningProgress with ID {progressId} not found.");

            if (progress.Enrollment.Class.Status != (int)ClassStatusEnum.Completed)
                throw new InvalidOperationException("Cannot fail progress: Class is not 'Completed'.");

            if (progress.Status != (int)LearningProgressStatusEnum.InProgress)
                throw new InvalidOperationException("Only 'InProgress' progress can be manually failed.");

            progress.Status = (int)LearningProgressStatusEnum.Failed;
            progress.LastUpdated = DateTime.UtcNow;
            await _uow.LearningProgressRepository.UpdateAsync(progress);
            await _uow.SaveChangesAsync();

            return MapToDto(progress);
        }

        public async Task<int> StartAllProgressesAsync(int classId)
        {
            var classToStart = await _uow.ClassRepository.GetByIdAsync(classId);
            if (classToStart == null)
                throw new KeyNotFoundException($"Class with ID {classId} not found.");

            if (classToStart.Status != (int)ClassStatusEnum.Inprogress)
                throw new InvalidOperationException("Class is not 'Inprogress'. Cannot start progresses.");

            var progressesToStart = await _uow.LearningProgressRepository
                .GetAllAsQueryable()
                .Where(lp => lp.Enrollment.ClassId == classId && lp.Status == (int)LearningProgressStatusEnum.NotStarted)
                .ToListAsync();

            foreach (var progress in progressesToStart)
            {
                progress.Status = (int)LearningProgressStatusEnum.InProgress;
                progress.LastUpdated = DateTime.UtcNow;
                await _uow.LearningProgressRepository.UpdateAsync(progress);
            }

            await _uow.SaveChangesAsync();
            return progressesToStart.Count;
        }

        public async Task<int> CompleteAllProgressesAsync(int classId)
        {
            var classToComplete = await _uow.ClassRepository.GetByIdAsync(classId);
            if (classToComplete == null)
                throw new KeyNotFoundException($"Class with ID {classId} not found.");

            if (classToComplete.Status != (int)ClassStatusEnum.Completed)
                throw new InvalidOperationException("Class is not 'Completed'. Cannot complete progresses.");

            var progressesToComplete = await _uow.LearningProgressRepository
                .GetAllAsQueryable()
                .Where(lp => lp.Enrollment.ClassId == classId && lp.Status == (int)LearningProgressStatusEnum.InProgress)
                .ToListAsync();

            foreach (var progress in progressesToComplete)
            {
                progress.Status = (int)LearningProgressStatusEnum.Completed;
                progress.ProgressPercentage = 100;
                progress.LastUpdated = DateTime.UtcNow;
                await _uow.LearningProgressRepository.UpdateAsync(progress);
            }

            await _uow.SaveChangesAsync();
            return progressesToComplete.Count;
        }

        #endregion

        #region Mappping & Helpers

        private IQueryable<LearningProgress> GetProgressQuery()
        {
            return _uow.LearningProgressRepository
                .GetAllAsQueryable()
                .AsNoTracking() // Use AsNoTracking for read operations
                .Include(lp => lp.Enrollment)
                    .ThenInclude(e => e.Trainee)
                    .ThenInclude(t => t.IdNavigation) // Trainee -> User (for Fullname)
                .Include(lp => lp.Enrollment)
                    .ThenInclude(e => e.Class)
                .Include(lp => lp.SectionRecords)
                    .ThenInclude(sr => sr.ActivityRecords);
        }

        private static ProgressDto MapToDto(LearningProgress lp)
        {
            string status = lp.Status.HasValue
                ? Enum.GetName(typeof(LearningProgressStatusEnum), lp.Status.Value) ?? "NotStarted"
                : "NotStarted";

            return new ProgressDto
            {
                Id = lp.Id,
                EnrollmentId = lp.EnrollmentId,
                TraineeId = lp.Enrollment.TraineeId,
                TraineeName = lp.Enrollment.Trainee.IdNavigation.Fullname ?? "N/A",
                ClassId = lp.Enrollment.ClassId,
                ClassName = lp.Enrollment.Class.Name ?? "N/A",
                Status = status,
                ProgressPercentage = lp.ProgressPercentage,
                TheoryScore = lp.TheoryScore,
                PracticalScore = lp.PracticalScore,
                FinalScore = lp.FinalScore,
                StartDate = lp.StartDate,
                LastUpdated = lp.LastUpdated,
                Name = lp.Name,
                Description = lp.Description,
                SectionRecords = lp.SectionRecords.Select(MapToSectionDto).ToList()
            };
        }

        private static SectionRecordDto MapToSectionDto(SectionRecord sr)
        {
            return new SectionRecordDto
            {
                Id = sr.Id,
                Name = sr.Name,
                SectionId = sr.SectionId,
                SectionName = sr.SectionName,
                IsCompleted = sr.IsCompleted,
                Progress = sr.Progress,
                DurationMinutes = sr.DurationMinutes,
                ActivityRecords = sr.ActivityRecords.Select(MapToActivityDto).ToList()
            };
        }

        private static ActivityRecordDto MapToActivityDto(ActivityRecord ar)
        {
            string status = ar.Status.HasValue
                ? Enum.GetName(typeof(ActivityRecordStatusEnum), ar.Status.Value) ?? "NotStarted"
                : "NotStarted";

            return new ActivityRecordDto
            {
                Id = ar.Id,
                ActivityId = ar.ActivityId,
                Status = status,
                Score = ar.Score,
                IsCompleted = ar.IsCompleted,
                CompletedDate = ar.CompletedDate
            };
        }

        #endregion
    }
}
