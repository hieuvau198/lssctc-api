using Lssctc.ProgramManagement.Learnings.Dtos;
using Lssctc.Share.Common;
using Lssctc.Share.Entities;
using Lssctc.Share.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace Lssctc.ProgramManagement.Learnings.Services
{
    public class LearningsSectionPartitionService : ILearningsSectionPartitionService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILearningsSectionService _learningsSectionService;
        public LearningsSectionPartitionService(IUnitOfWork unitOfWork, ILearningsSectionService learningsSectionService)
        {
            _unitOfWork = unitOfWork;
            _learningsSectionService = learningsSectionService;
        }
        public async Task<List<LearningsSectionPartitionDto>> GetAllSectionPartitionsBySectionIdAndTraineeId(int sectionId, int traineeId)
        {
            var partitions = await _unitOfWork.SectionPartitionRepository
                .GetAllAsQueryable()
                .Where(sp => sp.SectionId == sectionId)
                .OrderBy(sp => sp.DisplayOrder)
                .Include(sp => sp.LearningRecordPartitions)
                    .ThenInclude(lrp => lrp.LearningRecord)
                .ToListAsync();
            if (partitions == null || !partitions.Any())
                return new List<LearningsSectionPartitionDto>();
            var existedLearningRecord = partitions.First().LearningRecordPartitions
                .FirstOrDefault()?.LearningRecord
                ?? await GetOrCreateLearningRecord(partitions.First(), traineeId);
            var result = new List<LearningsSectionPartitionDto>();
            foreach (var partition in partitions)
            {
                var dto = await MapToLearningSectionPartitionDto(partition, traineeId, existedLearningRecord);
                result.Add(dto);
            }
            return result;
        }

        public async Task<LearningsSectionPartitionDto> GetSectionPartitionByPartitionIdAndTraineeId(int partitionId, int traineeId)
        {
            var partition = await _unitOfWork.SectionPartitionRepository
                .GetAllAsQueryable()
                .Where(sp => sp.Id == partitionId)
                .Include(sp => sp.LearningRecordPartitions)
                    .ThenInclude(lrp => lrp.LearningRecord)
                .FirstOrDefaultAsync();
            if (partition == null)
                throw new KeyNotFoundException($"SectionPartition with ID {partitionId} not found.");
            var existedLearningRecord = partition.LearningRecordPartitions
                .FirstOrDefault()?.LearningRecord
                ?? await GetOrCreateLearningRecord(partition, traineeId);
            return await MapToLearningSectionPartitionDto(partition, traineeId, existedLearningRecord);
        }

        public async Task<PagedResult<LearningsSectionPartitionDto>> GetSectionPartitionsBySectionIdAndTraineeIdPaged(int sectionId, int traineeId, int pageIndex, int pageSize)
        {
            var query = _unitOfWork.SectionPartitionRepository
                .GetAllAsQueryable()
                .Where(sp => sp.SectionId == sectionId)
                .OrderBy(sp => sp.DisplayOrder)
                .Include(sp => sp.LearningRecordPartitions)
                    .ThenInclude(lrp => lrp.LearningRecord);
            var totalCount = await query.CountAsync();
            var partitions = await query
                .Skip((pageIndex - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
            if (partitions == null || !partitions.Any())
                return new PagedResult<LearningsSectionPartitionDto>
                {
                    Items = new List<LearningsSectionPartitionDto>(),
                    TotalCount = 0,
                    Page = pageIndex,
                    PageSize = pageSize
                };
            var existedLearningRecord = partitions.First().LearningRecordPartitions
                .FirstOrDefault()?.LearningRecord
                ?? await GetOrCreateLearningRecord(partitions.First(), traineeId);
            var items = new List<LearningsSectionPartitionDto>();
            foreach (var partition in partitions)
            {
                var dto = await MapToLearningSectionPartitionDto(partition, traineeId, existedLearningRecord);
                items.Add(dto);
            }
            return new PagedResult<LearningsSectionPartitionDto>
            {
                Items = items.ToList(),
                TotalCount = totalCount,
                Page = pageIndex,
                PageSize = pageSize
            };
        }

        private async Task<LearningRecord> GetOrCreateLearningRecord(SectionPartition partition, int traineeId)
        {
            var existingLr = _unitOfWork.LearningRecordRepository
            .GetAllAsQueryable()
            .FirstOrDefault(lr => lr.SectionId == partition.SectionId && lr.TrainingProgress.CourseMember.TraineeId == traineeId);
            if (existingLr != null)
                return existingLr;
            var newLr = await _learningsSectionService.GetSectionBySectionIdAndTraineeId(partition.Section.Id, traineeId);
            return await _unitOfWork.LearningRecordRepository
                .GetByIdAsync(newLr.SectionRecordId) 
                ?? throw new Exception($"LearningRecord not found after creation. SectionId: {partition.Section.Id}, TraineeId: {traineeId}");

        }

        private async Task<LearningsSectionPartitionDto> MapToLearningSectionPartitionDto(SectionPartition partition, int traineeId, LearningRecord existedLearningRecord)
        {
            partition = partition ?? throw new ArgumentNullException(nameof(partition));
            existedLearningRecord = existedLearningRecord ?? throw new ArgumentNullException(nameof(existedLearningRecord));
            var firstLrp = await GetOrCreateLearningRecordPartition(existedLearningRecord, partition);

            return new LearningsSectionPartitionDto
            {
                SectionPartitionId = partition.Id,
                SectionId = partition.SectionId,
                PartitionRecordId = firstLrp.Id,
                PartitionName = partition.Name,
                PartitionDescription = partition.Description,
                PartitionOrder = partition.DisplayOrder ?? 1,
                PartitionType = partition.PartitionTypeId,
                PartitionRecordStatus = firstLrp.IsComplete ? "Completed" : "InCompleted",
                IsCompleted = existedLearningRecord?.IsCompleted ?? false,
            };
        }

        private async Task<LearningRecordPartition> GetOrCreateLearningRecordPartition(LearningRecord learningRecord, SectionPartition partition)
        {
            if (learningRecord == null)
                throw new ArgumentNullException(nameof(learningRecord));
            if (partition == null)
                throw new ArgumentNullException(nameof(partition));
            var existingLrp = partition.LearningRecordPartitions
                .OrderBy(lrp => lrp.RecordPartitionOrder)
                .FirstOrDefault();
            if (existingLrp != null)
                return existingLrp;
            var newLrp = new LearningRecordPartition
            {
                SectionPartitionId = partition.Id,
                Name = partition.Name + "Attempt",
                LearningRecordId = learningRecord.Id,
                Description = partition.Description,
                UpdatedAt = DateTime.UtcNow,
                StartedAt = DateTime.UtcNow,
                IsComplete = false,
                RecordPartitionOrder = 1,
            };
            await _unitOfWork.LearningRecordPartitionRepository.CreateAsync(newLrp);
            await _unitOfWork.SaveChangesAsync();
            return newLrp;
        }
    }
}
