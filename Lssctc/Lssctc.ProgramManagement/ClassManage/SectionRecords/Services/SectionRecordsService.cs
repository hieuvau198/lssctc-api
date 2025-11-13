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

        public async Task<IEnumerable<TraineeSectionDto>> GetTraineeSectionRecordsAsync(int classId, int traineeId)
        {
            // 1. Find the trainee's learning progress for this class
            var learningProgress = await _uow.LearningProgressRepository
                .GetAllAsQueryable()
                .Include(lp => lp.Enrollment)
                .Include(lp => lp.SectionRecords) // Load the trainee's actual records
                .FirstOrDefaultAsync(lp => lp.Enrollment.ClassId == classId && lp.Enrollment.TraineeId == traineeId);

            if (learningProgress == null)
            {
                throw new KeyNotFoundException("Learning progress not found for this trainee in this class.");
            }

            // 2. Get the course's section template (the master list of sections and their order)
            var courseTemplate = await _uow.CourseSectionRepository
                .GetAllAsQueryable()
                .Include(cs => cs.Section) // Include the details from the Section entity
                .Where(cs => cs.CourseId == learningProgress.CourseId)
                .OrderBy(cs => cs.SectionOrder)
                .ToListAsync();

            // 3. Create a dictionary of the trainee's records for efficient lookup
            var sectionRecordMap = learningProgress.SectionRecords
                .ToDictionary(sr => sr.SectionId ?? -1, sr => sr); // Keyed by SectionId

            // 4. Join the template with the trainee's records
            var resultList = new List<TraineeSectionDto>();
            foreach (var templateSection in courseTemplate)
            {
                if (templateSection.Section == null || templateSection.Section.IsDeleted == true) continue;

                sectionRecordMap.TryGetValue(templateSection.SectionId, out var traineeRecord);

                var dto = new TraineeSectionDto
                {
                    SectionId = templateSection.SectionId,
                    SectionTitle = templateSection.Section.SectionTitle,
                    SectionDescription = templateSection.Section.SectionDescription,
                    SectionOrder = templateSection.SectionOrder,
                    EstimatedDurationMinutes = templateSection.Section.EstimatedDurationMinutes,

                    SectionRecordId = traineeRecord?.Id ?? 0,
                    IsCompleted = traineeRecord?.IsCompleted ?? false,
                    Progress = traineeRecord?.Progress ?? 0m
                };
                resultList.Add(dto);
            }

            return resultList;
        }


        private IQueryable<SectionRecord> GetSectionRecordQuery()
        {
            return _uow.SectionRecordRepository
                .GetAllAsQueryable()
                .AsNoTracking()
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
            };
        }

    }
}
