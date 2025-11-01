using Lssctc.LearningManagement.PracticeAttempts.Dtos;
using Lssctc.Share.Entities;
using Lssctc.Share.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Lssctc.LearningManagement.PracticeAttempts.Services
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

            #region SectionPractice Existence Check

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

            #endregion

            #region Find or create LearningRecordPartition

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

            #endregion

            #region Create new attempt

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

            #endregion

            #region Create all SectionPracticeAttemptSteps for the new attempt

            await CreateAllSectionPracticeAttemptSteps(newAttempt, sectionPractice);

            #endregion

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

        #region Confirm Practice Attempt Complete

        public async Task<PracticeAttemptDto> ConfirmPracticeAttemptComplete(int attemptId)
        {
            #region Check attempt existence

            var attempt = await _unitOfWork.SectionPracticeAttemptRepository
                .GetAllAsQueryable()
                .Where(spa => spa.Id == attemptId && spa.IsDeleted != true)
                .Include(spa => spa.SectionPracticeAttemptSteps)
                .Include(spa => spa.SectionPractice.Practice.PracticeSteps)
                .FirstOrDefaultAsync();
            if (attempt == null)
                throw new KeyNotFoundException($"Practice attempt with ID {attemptId} does not exist.");

            #endregion

            #region Check all attempt steps belong to this attempt

            var steps = attempt.SectionPracticeAttemptSteps;
            if (steps == null || steps.Count == 0)
                throw new InvalidOperationException("This attempt has no recorded steps.");

            var defaultStepIds = attempt.SectionPractice.Practice.PracticeSteps
            .Where(ps => ps.IsDeleted != true)
            .Select(ps => ps.Id)
            .ToList();

            var attemptStepIds = steps
            .Where(s => s.IsDeleted != true)
            .Select(s => s.PracticeStepId)
            .ToList();

            bool coversAllDefaultSteps = defaultStepIds.All(id => attemptStepIds.Contains(id));
            if (!coversAllDefaultSteps)
                throw new InvalidOperationException("This attempt does not include all required practice steps.");

            bool allStepsPass = steps.All(step => step.IsPass == true && step.IsDeleted == false);
            if (allStepsPass)
            {
                attempt.IsPass = true;
                attempt.Score = 100;
                attempt.Description = "Pass";
            }
            else
            {
                attempt.IsPass = false;
                attempt.Score = 0;
                attempt.Description = "Fail";
            }

            #endregion

            #region Update attempt

            await _unitOfWork.SectionPracticeAttemptRepository.UpdateAsync(attempt);
            await _unitOfWork.SaveChangesAsync();

            #endregion

            var dto = MapToDto(attempt);
            return dto;

        }

        #endregion

        #region Private create all SectionPracticeAttemptStep for a given attempt

        private async Task CreateAllSectionPracticeAttemptSteps(SectionPracticeAttempt attempt, SectionPractice sectionPractice)
        {
            if(attempt == null || sectionPractice == null)
                throw new ArgumentNullException("Attempt or SectionPractice cannot be null.");
            var steps = await _unitOfWork.PracticeStepRepository
                .GetAllAsQueryable()
                .Where(s => 
                    s.PracticeId == sectionPractice.PracticeId
                    && s.IsDeleted != true)
                .ToListAsync();
            var attemptSteps = steps.Select(s => new SectionPracticeAttemptStep
            {
                AttemptId = attempt.Id,
                PracticeStepId = s.Id,
                Score = 0,
                Description = "Initial step attempt",
                IsPass = false,
                IsDeleted = false
            }).ToList();
            foreach (var attemptStep in attemptSteps)
            {
                await _unitOfWork.SectionPracticeAttemptStepRepository.CreateAsync(attemptStep);
            }
            await _unitOfWork.SaveChangesAsync();
        }

        #endregion

        #region Private Mapper
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
