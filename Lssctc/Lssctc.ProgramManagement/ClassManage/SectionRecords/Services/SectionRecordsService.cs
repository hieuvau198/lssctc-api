using Lssctc.ProgramManagement.ClassManage.SectionRecords.Dtos;
using Lssctc.Share.Common;
using Lssctc.Share.Entities;
using Lssctc.Share.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Lssctc.ProgramManagement.ClassManage.SectionRecords.Services
{
    public class SectionRecordsService : ISectionRecordsService
    {
        private readonly IUnitOfWork _uow;

        public SectionRecordsService(IUnitOfWork uow)
        {
            _uow = uow;
        }

        public async Task<IEnumerable<SectionRecordDto>> GetSectionRecordsAsync(int classId, int traineeId)
        {
            var records = await GetSectionRecordQuery()
                .Where(sr => sr.LearningProgress.Enrollment.ClassId == classId &&
                             sr.LearningProgress.Enrollment.TraineeId == traineeId)
                .ToListAsync();

            return records.Select(MapToDto);
        }

        public async Task<PagedResult<SectionRecordDto>> GetSectionRecordsPagedAsync(int classId, int traineeId, int pageNumber, int pageSize)
        {
            var query = GetSectionRecordQuery()
                .Where(sr => sr.LearningProgress.Enrollment.ClassId == classId &&
                             sr.LearningProgress.Enrollment.TraineeId == traineeId)
                .Select(sr => MapToDto(sr));

            return await query.ToPagedResultAsync(pageNumber, pageSize);
        }

        public async Task<IEnumerable<SectionRecordDto>> GetSectionRecordsBySectionAsync(int classId, int sectionId)
        {
            var records = await GetSectionRecordQuery()
                .Where(sr => sr.LearningProgress.Enrollment.ClassId == classId &&
                             sr.SectionId == sectionId)
                .ToListAsync();

            return records.Select(MapToDto);
        }

        public async Task<PagedResult<SectionRecordDto>> GetSectionRecordsBySectionPagedAsync(int classId, int sectionId, int pageNumber, int pageSize)
        {
            var query = GetSectionRecordQuery()
                .Where(sr => sr.LearningProgress.Enrollment.ClassId == classId &&
                             sr.SectionId == sectionId)
                .Select(sr => MapToDto(sr));

            return await query.ToPagedResultAsync(pageNumber, pageSize);
        }


        private IQueryable<SectionRecord> GetSectionRecordQuery()
        {
            return _uow.SectionRecordRepository
                .GetAllAsQueryable()
                .AsNoTracking()
                // .Include(sr => sr.ActivityRecords) // Removed
                .Include(sr => sr.LearningProgress)
                    .ThenInclude(lp => lp.Enrollment)
                        .ThenInclude(e => e.Trainee)
                            .ThenInclude(t => t.IdNavigation);
        }

        private static SectionRecordDto MapToDto(SectionRecord sr)
        {
            return new SectionRecordDto
            {
                Id = sr.Id,
                Name = sr.Name,
                LearningProgressId = sr.LearningProgressId,
                SectionId = sr.SectionId,
                SectionName = sr.SectionName,
                IsCompleted = sr.IsCompleted,
                IsTraineeAttended = sr.IsTraineeAttended,
                Progress = sr.Progress,
                TraineeId = sr.LearningProgress.Enrollment.TraineeId,
                TraineeName = sr.LearningProgress.Enrollment.Trainee.IdNavigation.Fullname ?? "N/A",
                ClassId = sr.LearningProgress.Enrollment.ClassId
                // ActivityRecords = sr.ActivityRecords.Select(MapToActivityDto).ToList() // Removed
            };
        }

        /* // Removed
        private static ActivityRecordDto MapToActivityDto(ActivityRecord ar)
        {
            string status = ar.Status.HasValue
                ? Enum.GetName(typeof(ActivityStatusEnum), ar.Status.Value) ?? "NotStarted"
                : "NotStarted";

            return new ActivityRecordDto
            {
                Id = ar.Id,
                ActivityId = ar.ActivityId,
                Status = status,
                Score = ar.Score,
                IsCompleted = ar.IsCompleted,
                CompletedDate = ar.CompletedDate,
                ActivityType = ar.ActivityType
            };
        }
        */
    }
}
