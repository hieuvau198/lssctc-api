using Lssctc.Share.Entities;
using Lssctc.Share.Interfaces;
using Lssctc.SimulationManagement.PracticeAttempts.Dtos;
using Microsoft.EntityFrameworkCore;

namespace Lssctc.SimulationManagement.PracticeAttempts.Services
{
    public class PracticeAttemptService : IPracticeAttemptService
    {
        private readonly IUnitOfWork _unitOfWork;
        public PracticeAttemptService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        #region Create Practice Attempt
        public async Task<PracticeAttemptDto> CreatePracticeAttempt(int sectionPracticeId, int traineeId)
        {
            if (sectionPracticeId <= 0 || traineeId <= 0)
                throw new ArgumentException("Invalid section practice ID or trainee ID.");

            // Validate existence of SectionPractice for this trainee
            var sectionPractice = await _unitOfWork.SectionPracticeRepository
                .GetAllAsQueryable()
                .Where(sp =>
                    sp.Id == sectionPracticeId 
                    && sp.IsDeleted != true 
                    && sp.SectionPartition.Section.Classes.ClassMembers.Any(cm => cm.TraineeId == traineeId))
                .Include(sp => sp.SectionPartition)
                    .ThenInclude(sp => sp.LearningRecordPartitions)
                        .ThenInclude(lrp => lrp.LearningRecord)
                .FirstOrDefaultAsync();

            if (sectionPractice == null)
                throw new KeyNotFoundException($"No SectionPractice found for ID {sectionPracticeId} and Trainee {traineeId}.");

            // Find or create LearningRecordPartition
            var learningRecordPartition = sectionPractice.SectionPartition.LearningRecordPartitions?
                .FirstOrDefault();

            if (learningRecordPartition == null)
            {
                var learningRecord = await _unitOfWork.LearningRecordRepository
                    .GetAllAsQueryable()
                    .Where(lr =>
                        lr.SectionId == sectionPractice.SectionPartition.SectionId &&
                        lr.TrainingProgress.CourseMember.TraineeId == traineeId)
                    .FirstOrDefaultAsync();

                if (learningRecord == null)
                    throw new InvalidOperationException($"No LearningRecord found for Section ID {sectionPractice.SectionPartition.SectionId} and Trainee ID {traineeId}.");

                learningRecordPartition = new LearningRecordPartition
                {
                    SectionPartitionId = sectionPractice.SectionPartitionId,
                    Name = sectionPractice.Practice.PracticeName ?? "Learning Record Partition",
                    LearningRecordId = learningRecord.Id,
                    Description = $"Record for {sectionPractice.SectionPartition.Name}",
                    StartedAt = DateTime.UtcNow,
                    RecordPartitionOrder = 1,
                    IsComplete = false
                };

                await _unitOfWork.LearningRecordPartitionRepository.CreateAsync(learningRecordPartition);
                await _unitOfWork.SaveChangesAsync();
            }

            // Create new attempt
            var newAttempt = new SectionPracticeAttempt
            {
                SectionPracticeId = sectionPracticeId,
                LearningRecordPartitionId = learningRecordPartition.Id,
                Score = 0,
                AttemptDate = DateTime.UtcNow,
                AttemptStatus = 1,
                Description = "Initial attempt",
                IsPass = false,
                IsDeleted = false
            };

            await _unitOfWork.SectionPracticeAttemptRepository.CreateAsync(newAttempt);
            await _unitOfWork.SaveChangesAsync();

            return MapToDto(newAttempt);
        }
        #endregion

        #region Get Practice Attempt by ID
        public async Task<PracticeAttemptDto?> GetPracticeAttemptById(int attemptId)
        {
            var attempt = await _unitOfWork.SectionPracticeAttemptRepository
                .GetAllAsQueryable()
                .Where(a => a.Id == attemptId && a.IsDeleted != true)
                .FirstOrDefaultAsync();

            return attempt == null ? null : MapToDto(attempt);
        }
        #endregion

        #region Get Attempts by Practice ID & Trainee ID
        public async Task<List<PracticeAttemptDto>> GetPracticeAttemptsByPracticeIdAndTraineeId(int sectionPracticeId, int traineeId)
        {
            var attempts = await _unitOfWork.SectionPracticeAttemptRepository
                .GetAllAsQueryable()
                .Where(a =>
                    a.SectionPracticeId == sectionPracticeId &&
                    a.LearningRecordPartition.LearningRecord.TrainingProgress.CourseMember.TraineeId == traineeId &&
                    a.IsDeleted != true)
                .Include(a => a.LearningRecordPartition)
                    .ThenInclude(lrp => lrp.LearningRecord)
                .ToListAsync();

            return attempts.Select(MapToDto).ToList();
        }
        #endregion

        #region Delete Practice Attempt (Soft Delete)
        public async Task<bool> DeletePracticeAttempt(int attemptId)
        {
            var attempt = await _unitOfWork.SectionPracticeAttemptRepository
                .GetAllAsQueryable()
                .Where(a => a.Id == attemptId && a.IsDeleted != true)
                .FirstOrDefaultAsync();

            if (attempt == null)
                throw new KeyNotFoundException($"Attempt not found for ID {attemptId}.");

            attempt.IsDeleted = true;
            await _unitOfWork.SectionPracticeAttemptRepository.UpdateAsync(attempt);
            await _unitOfWork.SaveChangesAsync();

            return true;
        }
        #endregion

        #region Private Helper
        private static PracticeAttemptDto MapToDto(SectionPracticeAttempt attempt)
        {
            return new PracticeAttemptDto
            {
                PracticeAttemptId = attempt.Id,
                SectionPracticeId = attempt.SectionPracticeId,
                LearningRecordPartitionId = attempt.LearningRecordPartitionId,
                Score = attempt.Score,
                AttemptDate = attempt.AttemptDate,
                AttemptStatus = attempt.AttemptStatus,
                Description = attempt.Description,
                IsPass = attempt.IsPass
            };
        }
        #endregion
    }
}
