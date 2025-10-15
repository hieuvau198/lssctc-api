using Lssctc.ProgramManagement.Learnings.LearningsMaterials.Dtos;
using Lssctc.ProgramManagement.Learnings.LearningsPartitions.Services;
using Lssctc.Share.Entities;
using Lssctc.Share.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Lssctc.ProgramManagement.Learnings.LearningsMaterials.Services
{
    public class LearningsSectionMaterialService : ILearningsSectionMaterialService
    {   
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILearningsSectionPartitionService _learningsSectionPartitionService;
        public LearningsSectionMaterialService(IUnitOfWork unitOfWork, ILearningsSectionPartitionService learningsSectionPartitionService)
        {
            _unitOfWork = unitOfWork;
            _learningsSectionPartitionService = learningsSectionPartitionService;
        }
        public async Task<LearningsSectionMaterialDto> GetSectionMaterialByPartitionIdAndTraineeId(
            int partitionId, int traineeId)
        {
            var existedLrp = await GetLrpByPartitionIdandTraineeId(partitionId, traineeId);

            #region Get the section material
            var sectionMaterial = await _unitOfWork.SectionMaterialRepository
                .GetAllAsQueryable()
                .Include(sm => sm.LearningMaterial.LearningMaterialType)
                .FirstOrDefaultAsync(sm =>
                    sm.SectionPartitionId == partitionId &&
                    sm.SectionPartition.LearningRecordPartitions.Any(lrp =>
                        lrp.Id == existedLrp.Id)
                    )
                ;
            if (sectionMaterial == null)
                throw new KeyNotFoundException($"SectionMaterial with Partition ID {partitionId} for trainee {traineeId} not found.");
            if(sectionMaterial.LearningMaterial == null)
                throw new KeyNotFoundException($"LearningMaterial for SectionMaterial ID {sectionMaterial.Id} not found.");
            
            #endregion

            #region Map to DTO and return
            var lsmDto = new LearningsSectionMaterialDto
            {
                SectionMaterialId = sectionMaterial.Id,
                MaterialId = sectionMaterial.LearningMaterialId,
                PartitionId = sectionMaterial.SectionPartitionId,
                PartitionRecordId = existedLrp.Id,
                MaterialName = sectionMaterial.LearningMaterial.Name,
                MaterialDescription = sectionMaterial.LearningMaterial.Description,
                MaterialType = sectionMaterial.LearningMaterial.LearningMaterialType.Id,
                MaterialUrl = sectionMaterial.LearningMaterial.MaterialUrl,
                PartitionRecordStatus = existedLrp.IsComplete ? "Completed" : "InCompleted",
                IsCompleted = existedLrp.IsComplete,
            };

            return lsmDto;
            #endregion

        }

        public async Task<bool> UpdateLearningMaterialAsCompleted(int partitionId, int traineeId)
        {
            try
            {
                var existedLrp = await GetLrpByPartitionIdandTraineeId(partitionId, traineeId);
                if (existedLrp.IsComplete)
                    return true; // Already completed
                existedLrp.IsComplete = true;
                existedLrp.CompletedAt = DateTime.UtcNow;
                await _unitOfWork.LearningRecordPartitionRepository.UpdateAsync(existedLrp);
                await _unitOfWork.SaveChangesAsync();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<bool> UpdateLearningMaterialAsNotCompleted(int partitionId, int traineeId)
        {
            try
            {
                var existedLrp = await GetLrpByPartitionIdandTraineeId(partitionId, traineeId);
                if (!existedLrp.IsComplete)
                    return true; // Already incompleted
                existedLrp.IsComplete = false;
                existedLrp.CompletedAt = DateTime.MaxValue;
                await _unitOfWork.LearningRecordPartitionRepository.UpdateAsync(existedLrp);
                await _unitOfWork.SaveChangesAsync();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        private async Task<LearningRecordPartition> GetLrpByPartitionIdandTraineeId(int partitionId, int traineeId)
        {
            var existedLrp = await _unitOfWork.LearningRecordPartitionRepository
                .GetAllAsQueryable()
                .Include(lrp => lrp.LearningRecord)
                    .ThenInclude(lr => lr.TrainingProgress)
                        .ThenInclude(tp => tp.CourseMember)
                .OrderBy(lrp => lrp.RecordPartitionOrder)
                .FirstOrDefaultAsync(lrp =>
                    lrp.SectionPartitionId == partitionId &&
                    lrp.LearningRecord.TrainingProgress.CourseMember.TraineeId == traineeId);

            if (existedLrp == null)
            {
                await _learningsSectionPartitionService.GetSectionPartitionByPartitionIdAndTraineeId(partitionId, traineeId);
                existedLrp = await _unitOfWork.LearningRecordPartitionRepository
                .GetAllAsQueryable()
                .Include(lrp => lrp.LearningRecord)
                    .ThenInclude(lr => lr.TrainingProgress)
                        .ThenInclude(tp => tp.CourseMember)
                .OrderBy(lrp => lrp.RecordPartitionOrder)
                .FirstOrDefaultAsync(lrp =>
                    lrp.SectionPartitionId == partitionId &&
                    lrp.LearningRecord.TrainingProgress.CourseMember.TraineeId == traineeId);
            }
            if (existedLrp == null)
                throw new KeyNotFoundException($"LearningRecordPartition with Partition ID {partitionId} for trainee {traineeId} not found.");
            return existedLrp;
        }
    }
}
