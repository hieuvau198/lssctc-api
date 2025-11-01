using Lssctc.LearningManagement.TraineePractices.Dtos;
using Lssctc.Share.Entities;
using Lssctc.Share.Interfaces;
using Microsoft.EntityFrameworkCore;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Model;

namespace Lssctc.LearningManagement.TraineePractices.Services
{
    public class TraineeStepService : ITraineeStepService
    {
        private readonly IUnitOfWork _unitOfWork;
        public TraineeStepService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<TraineeStepDto?> GetTraineeStepByIdAndTraineeId(
            int stepId, 
            int traineeId)
        {
            var lrp = await GetLearningRecordPartitionByTraineeIdAsync(traineeId);

            var practiceStep = await _unitOfWork.PracticeStepRepository
                .GetAllAsQueryable()
                .Include(ps => ps.Practice)
                .Where(ps =>
                    ps.Id == stepId &&
                    ps.IsDeleted != true &&
                    ps.Practice.SectionPractices.Any(sp => sp.SectionPartition.Id == lrp.SectionPartitionId))
                .FirstOrDefaultAsync();

            if (practiceStep == null)
                throw new KeyNotFoundException($"No PracticeStep found with ID {stepId} associated with Trainee ID {traineeId}.");

            return await MapToTraineeStepDtoAsync(practiceStep, lrp);
        }

        public async Task<List<TraineeStepDto>> GetTraineeStepsByAttemptId(int attemptId)
        {
            // Step 1: Get attempt with related data
            var attempt = await _unitOfWork.SectionPracticeAttemptRepository
                .GetAllAsQueryable()
                .Where(spa => spa.Id == attemptId && spa.IsDeleted != true)
                .Include(spa => spa.LearningRecordPartition)
                .Include(spa => spa.SectionPractice)
                    .ThenInclude(sp => sp.Practice)
                .Include(spa => spa.SectionPracticeAttemptSteps)
                .FirstOrDefaultAsync()
                ?? throw new KeyNotFoundException($"No SectionPracticeAttempt found with ID {attemptId}.");

            // Step 2: Get all steps for the Practice
            var practiceSteps = await _unitOfWork.PracticeStepRepository
                .GetAllAsQueryable()
                .Include(ps => ps.Practice)
                .Where(ps =>
                    ps.PracticeId == attempt.SectionPractice.PracticeId &&
                    ps.IsDeleted != true)
                .ToListAsync();

            if (practiceSteps == null || !practiceSteps.Any())
                throw new KeyNotFoundException($"No PracticeSteps found for Practice ID {attempt.SectionPractice.PracticeId}.");

            // Step 3: Map to DTOs
            var dtos = new List<TraineeStepDto>();
            foreach (var step in practiceSteps)
            {
                // Find matching attempt step (if exists)
                var attemptStep = attempt.SectionPracticeAttemptSteps
                    .FirstOrDefault(spas => spas.PracticeStepId == step.Id && spas.IsDeleted != true);

                // Get action and component (similar to MapToTraineeStepDtoAsync)
                var action = await _unitOfWork.PracticeStepActionRepository
                    .GetAllAsQueryable()
                    .Where(psa => psa.StepId == step.Id && psa.IsDeleted != true)
                    .Select(psa => psa.Action)
                    .FirstOrDefaultAsync();

                var component = await _unitOfWork.PracticeStepComponentRepository
                    .GetAllAsQueryable()
                    .Where(psc => psc.StepId == step.Id && psc.IsDeleted != true)
                    .Select(psc => psc.Component)
                    .FirstOrDefaultAsync();

                if (action == null)
                    throw new InvalidOperationException($"No SimAction assigned to PracticeStep ID {step.Id}.");
                if (component == null)
                    throw new InvalidOperationException($"No SimulationComponent assigned to PracticeStep ID {step.Id}.");

                var dto = new TraineeStepDto
                {
                    StepId = step.Id,
                    StepName = step.StepName ?? "Step Name",
                    StepDescription = step.StepDescription ?? "Step Description",
                    ExpectedResult = step.ExpectedResult ?? "Expected Result",
                    StepOrder = step.StepOrder,
                    IsCompleted = attemptStep?.IsPass ?? false, // ✅ Key difference
                    PracticeId = step.PracticeId,
                    ActionId = action.Id,
                    ActionName = action.Name,
                    ActionDescription = action.Description,
                    ActionKey = action.ActionKey,
                    ComponentId = component.Id,
                    ComponentName = component.Name,
                    ComponentDescription = component.Description,
                    ComponentImageUrl = component.ImageUrl
                };

                dtos.Add(dto);
            }

            // Step 4: Return ordered list
            return dtos.OrderBy(dto => dto.StepOrder).ToList();
        }


        public async Task<List<TraineeStepDto>> GetTraineeStepsByPracticeIdAndTraineeId(
            int practiceId, 
            int traineeId)
        {
            var lrp = await GetLearningRecordPartitionByTraineeIdAsync(traineeId);

            var practiceSteps = await _unitOfWork.PracticeStepRepository
                .GetAllAsQueryable()
                .Include(ps => ps.Practice)
                .Where(ps =>
                    ps.PracticeId == practiceId &&
                    ps.IsDeleted != true &&
                    ps.Practice.SectionPractices.Any(sp => sp.SectionPartition.Id == lrp.SectionPartitionId))
                .ToListAsync();

            if (practiceSteps == null || !practiceSteps.Any())
                throw new KeyNotFoundException($"No PracticeSteps found for Practice ID {practiceId} and Trainee ID {traineeId}.");

            var dtos = new List<TraineeStepDto>();
            foreach (var step in practiceSteps)
            {
                var dto = await MapToTraineeStepDtoAsync(step, lrp);
                dtos.Add(dto);
            }

            return dtos.OrderBy(dto => dto.StepOrder).ToList();
        }

        public async Task<bool> SubmitTraineeStepAttempt(
            int attemptId, 
            int traineeId , UpdateTraineeStepAttemptDto input)
        {
            #region Find attempt and step, return 404 if not found

            var attempt = await _unitOfWork.SectionPracticeAttemptRepository
                    .GetAllAsQueryable()
                    .Where(spa => spa.Id == attemptId &&
                                  spa.LearningRecordPartition.LearningRecord.TrainingProgress.CourseMember.TraineeId == traineeId &&
                                  spa.IsDeleted != true)
                    .Include(spa => spa.SectionPracticeAttemptSteps)
                    .Include(spa => spa.SectionPractice)
                    .FirstOrDefaultAsync()
                    ?? throw new KeyNotFoundException($"Attempt not found for Attempt ID {attemptId} and Trainee ID {traineeId}.");

            var step = await _unitOfWork.PracticeStepRepository
                .GetAllAsQueryable()
                .Where(ps => 
                    ps.Id == input.CurrentStepId
                    && ps.PracticeId == attempt.SectionPractice.PracticeId
                    && ps.IsDeleted != true)
                .Include(ps => ps.PracticeStepComponents)
                .Include(ps => ps.PracticeStepActions)
                    .ThenInclude(psa => psa.Action)
                .FirstOrDefaultAsync() 
                ?? throw new KeyNotFoundException($"Wrong step! Step not found for Step ID {input.CurrentStepId}.");

            #endregion

            #region Check if attempt step exists, if not create it

            var stepAttempt = attempt.SectionPracticeAttemptSteps
                    .FirstOrDefault(spas => spas.PracticeStepId == step.Id && spas.IsDeleted != true);
            if (stepAttempt == null)
            {
                // Create new attempt step
                stepAttempt = new SectionPracticeAttemptStep
                {
                    AttemptId = attempt.Id,
                    PracticeStepId = step.Id,
                    Score = 100,
                    Description = step.StepName + " Attempt",
                    IsPass = false,
                    IsDeleted = false
                };
                await _unitOfWork.SectionPracticeAttemptStepRepository.CreateAsync(stepAttempt);
                try
                {
                    await _unitOfWork.SaveChangesAsync();
                }
                catch (DbUpdateException dbEx)
                {
                    throw new InvalidOperationException("Failed to create SectionPracticeAttemptStep due to database update error.", dbEx);
                }
            }

            #endregion

            #region Validate action and component

            var action = step.PracticeStepActions.FirstOrDefault(psa => psa.Action.ActionKey == input.ActionKey && psa.IsDeleted != true);
            if (action == null) 
                throw new KeyNotFoundException($"Action with ID {input.ActionKey} not found for Step ID {step.Id}.");

            var component = step.PracticeStepComponents.FirstOrDefault(psc => psc.ComponentId == input.ComponentId && psc.IsDeleted != true);
            if (component == null) 
                throw new KeyNotFoundException($"Component with ID {input.ComponentId} not found for Step ID {step.Id}.");

            #endregion

            stepAttempt.IsPass = true;
            stepAttempt.Score = 100;
            stepAttempt.Description = "Pass";

            await _unitOfWork.SectionPracticeAttemptStepRepository.UpdateAsync(stepAttempt);
            await _unitOfWork.SaveChangesAsync();

            return true;

        }

        #region Shared Helper Methods 

        private async Task<LearningRecordPartition> GetLearningRecordPartitionByTraineeIdAsync(
            int traineeId)
        {
            var lrp = await _unitOfWork.LearningRecordPartitionRepository
                .GetAllAsQueryable()
                .Where(lrp =>
                    lrp.LearningRecord.TrainingProgress.CourseMember.TraineeId == traineeId &&
                    lrp.SectionPartition.PartitionTypeId == 4)
                .FirstOrDefaultAsync();

            if (lrp == null)
                throw new KeyNotFoundException($"No LearningRecordPartition found for Trainee ID {traineeId} with PartitionTypeId 4.");

            return lrp;
        }

        private async Task<TraineeStepDto> MapToTraineeStepDtoAsync(
            PracticeStep practiceStep,
            LearningRecordPartition lrp)
        {
            // Attempt info
            var attemptStep = await _unitOfWork.SectionPracticeAttemptStepRepository
                .GetAllAsQueryable()
                .Where(spas => spas.PracticeStepId == practiceStep.Id &&
                               spas.Attempt.LearningRecordPartitionId == lrp.Id &&
                               spas.IsDeleted != true)
                .FirstOrDefaultAsync();

            // SimAction
            var action = await _unitOfWork.PracticeStepActionRepository
                .GetAllAsQueryable()
                .Where(psa => psa.StepId == practiceStep.Id && psa.IsDeleted != true)
                .Select(psa => psa.Action)
                .FirstOrDefaultAsync();

            if (action == null)
                throw new InvalidOperationException($"No SimAction assigned to PracticeStep ID {practiceStep.Id}.");

            // SimulationComponent
            var component = await _unitOfWork.PracticeStepComponentRepository
                .GetAllAsQueryable()
                .Where(psc => psc.StepId == practiceStep.Id && psc.IsDeleted != true)
                .Select(psc => psc.Component)
                .FirstOrDefaultAsync();

            if (component == null)
                throw new InvalidOperationException($"No SimulationComponent assigned to PracticeStep ID {practiceStep.Id}.");

            return new TraineeStepDto
            {
                StepId = practiceStep.Id,
                StepName = practiceStep.StepName ?? "Step Name",
                StepDescription = practiceStep.StepDescription ?? "Step Description",
                ExpectedResult = practiceStep.ExpectedResult ?? "Expected Result",
                StepOrder = practiceStep.StepOrder,
                IsCompleted = attemptStep?.IsPass ?? false,
                PracticeId = practiceStep.PracticeId,
                ActionId = action.Id,
                ActionName = action.Name,
                ActionDescription = action.Description,
                ActionKey = action.ActionKey,
                ComponentId = component.Id,
                ComponentName = component.Name,
                ComponentDescription = component.Description,
                ComponentImageUrl = component.ImageUrl
            };
        }

        #endregion
    }
}
