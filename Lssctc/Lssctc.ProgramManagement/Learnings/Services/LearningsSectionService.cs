using Lssctc.ProgramManagement.Learnings.Dtos;
using Lssctc.Share.Common;
using Lssctc.Share.Entities;
using Lssctc.Share.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace Lssctc.ProgramManagement.Learnings.Services
{
    public class LearningsSectionService : ILearningsSectionService
    {
        private readonly IUnitOfWork _unitOfWork;

        public LearningsSectionService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<List<LearningsSectionDto>> GetAllSectionsByClassIdAndTraineeId(int classId, int traineeId)
        {
            var sections = await _unitOfWork.SectionRepository
                .GetAllAsQueryable()
                .Include(s => s.LearningRecords)
                    .ThenInclude(lr => lr.TrainingProgress)
                        .ThenInclude(tp => tp.CourseMember)
                            .ThenInclude(cm => cm.Trainee)
                .Where(s => s.ClassesId == classId)
                .OrderBy(s => s.Order)
                .ToListAsync();

            var existedProgress = await CheckIfAllSectionsHaveProgress(traineeId, classId);

            var result = new List<LearningsSectionDto>();

            foreach (var section in sections)
            {
                var dto = await MapToLearningSectionToDto(section, traineeId, existedProgress);
                result.Add(dto);
            }

            return result;
        }

        public async Task<LearningsSectionDto> GetSectionBySectionIdAndTraineeId(int sectionId, int traineeId)
        {
            var section = await _unitOfWork.SectionRepository
                .GetAllAsQueryable()
                .Include(s => s.LearningRecords)
                    .ThenInclude(lr => lr.TrainingProgress)
                        .ThenInclude(tp => tp.CourseMember)
                            .ThenInclude(cm => cm.Trainee)
                .FirstOrDefaultAsync(s => s.Id == sectionId);

            if (section == null)
                throw new KeyNotFoundException($"Section with ID {sectionId} not found.");

            var classId = section.ClassesId;

            var existedProgress = await CheckIfAllSectionsHaveProgress(traineeId, classId);

            return await MapToLearningSectionToDto(section, traineeId, existedProgress);
        }

        public async Task<PagedResult<LearningsSectionDto>> GetSectionsByClassIdAndTraineeIdPaged(int classId, int traineeId, int pageIndex, int pageSize)
        {
            var query = _unitOfWork.SectionRepository
                .GetAllAsQueryable()
                .Include(s => s.LearningRecords)
                    .ThenInclude(lr => lr.TrainingProgress)
                        .ThenInclude(tp => tp.CourseMember)
                            .ThenInclude(cm => cm.Trainee)
                .Where(s => s.ClassesId == classId);

            var totalCount = await query.CountAsync();

            var sections = await query
                .OrderBy(s => s.Order)
                .Skip((pageIndex - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var existedProgress = await CheckIfAllSectionsHaveProgress(traineeId, classId);

            var items = new List<LearningsSectionDto>();
            foreach (var section in sections)
            {
                var dto = await MapToLearningSectionToDto(section, traineeId, existedProgress);
                items.Add(dto);
            }

            return new PagedResult<LearningsSectionDto>
            {
                Items = items.ToList(),
                TotalCount = totalCount,
                Page = pageIndex,
                PageSize = pageSize
            };
        }

        private async Task<LearningsSectionDto> MapToLearningSectionToDto(Section section, int traineeId, TrainingProgress existedProgress)
        {
            if (section == null)
                throw new ArgumentNullException(nameof(section));

            try
            {
                var sectionRecord = await GetOrCreateLearningRecord(section, traineeId, existedProgress);

                return new LearningsSectionDto
                {
                    SectionId = section.Id,
                    SectionName = section.Name,
                    SectionDescription = section.Description,
                    SectionOrder = section.Order,
                    DurationMinutes = section?.DurationMinutes ?? 0,
                    SectionRecordStatus = sectionRecord != null && sectionRecord.IsCompleted ? "Completed" : "InCompleted",
                    ClassId = section?.ClassesId ?? 0,
                    SectionRecordId = sectionRecord?.Id ?? 0,
                    IsCompleted = sectionRecord?.IsCompleted ?? false,
                    SectionProgress = sectionRecord?.Progress ?? 0,
                    IsTraineeAttended = sectionRecord?.IsTraineeAttended ?? false
                };
            }
            catch (Exception ex)
            {
                throw new Exception($"Error in MapToLearningSectionToDto - SectionId: {section?.Id}, TraineeId: {traineeId}", ex);
            }
        }

        private async Task<LearningRecord> GetOrCreateLearningRecord(Section section, int traineeId, TrainingProgress existedProgress)
        {
            if (section == null)
                throw new ArgumentNullException(nameof(section));

            try
            {
                var existingRecord = section.LearningRecords
                    .FirstOrDefault(lr => lr.TrainingProgress.CourseMember.Trainee.Id == traineeId);

                if (existingRecord != null)
                    return existingRecord;

                var newRecord = new LearningRecord
                {
                    SectionId = section.Id,
                    Name = section.Name,
                    TrainingProgressId = existedProgress.Id,
                    SectionName = section.Name,
                    IsCompleted = false,
                    IsTraineeAttended = false,
                    Progress = 0,
                };

                await _unitOfWork.LearningRecordRepository.CreateAsync(newRecord);
                await _unitOfWork.SaveChangesAsync();

                return newRecord;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error in GetOrCreateLearningRecord - SectionId: {section?.Id}, TraineeId: {traineeId}, ProgressId: {existedProgress?.Id}", ex);
            }
        }

        private async Task<TrainingProgress> CheckIfAllSectionsHaveProgress(int traineeId, int classId)
        {
            try
            {
                var classMember = _unitOfWork.ClassMemberRepository
                    .GetAllAsQueryable()
                    .Include(cm => cm.TrainingProgresses)
                    .FirstOrDefault(cm => cm.TraineeId == traineeId && cm.ClassId == classId);

                if (classMember == null)
                    throw new InvalidOperationException($"No class member found for trainee ID {traineeId} in class {classId}");

                var trainingProgress = classMember.TrainingProgresses.FirstOrDefault();

                if (trainingProgress == null)
                {
                    trainingProgress = new TrainingProgress
                    {
                        CourseMemberId = classMember.Id,
                        Status = 1,
                        ProgressPercentage = 0,
                        StartDate = DateTime.UtcNow,
                        LastUpdated = DateTime.UtcNow,
                        Name = $"Progress for class {classId}",
                        Description = $"Progress for class {classId}",
                    };

                    await _unitOfWork.TrainingProgressRepository.CreateAsync(trainingProgress);
                    await _unitOfWork.SaveChangesAsync();
                }

                return trainingProgress;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error in CheckIfAllSectionsHaveProgress - TraineeId: {traineeId}, ClassId: {classId}", ex);
            }
        }
    }
}
