using Lssctc.Share.Entities;
using Lssctc.Share.Interfaces;
using Lssctc.SimulationManagement.TraineePractices.Dtos;
using Microsoft.EntityFrameworkCore;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Model;

namespace Lssctc.SimulationManagement.TraineePractices.Services
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
                    .FirstOrDefaultAsync()
                    ?? throw new KeyNotFoundException($"Attempt not found for Attempt ID {attemptId} and Trainee ID {traineeId}.");

            var step = await _unitOfWork.PracticeStepRepository
                .GetAllAsQueryable()
                .Where(ps => 
                    ps.Id == input.CurrentStepId
                    && ps.PracticeId == attempt.SectionPracticeId 
                    && ps.IsDeleted != true)
                .Include(ps => ps.PracticeStepComponents)
                .Include(ps => ps.PracticeStepActions)
                .FirstOrDefaultAsync() 
                ?? throw new KeyNotFoundException($"Step not found for Step ID {input.CurrentStepId}.");

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
                    Score = 1000,
                    Description = step.StepName + " Attempt",
                    IsPass = false,
                    IsDeleted = false
                };
                await _unitOfWork.SectionPracticeAttemptStepRepository.CreateAsync(stepAttempt);
                await _unitOfWork.SaveChangesAsync();
            }

            #endregion

            #region Validate action and component

            var action = step.PracticeStepActions.FirstOrDefault(psa => psa.ActionId == input.ActionId && psa.IsDeleted != true);
            
            var component = step.PracticeStepComponents.FirstOrDefault(psc => psc.ComponentId == input.ComponentId && psc.IsDeleted != true);
            

            #endregion

            #region Process result
            if (action != null && component != null)
            {
                stepAttempt.IsPass = true;
                stepAttempt.Score = 1000;
                stepAttempt.Description = "Step completed successfully.";
            }
            else
            {
                stepAttempt.IsPass = false;
                stepAttempt.Score = 0;

                if (action == null && component == null)
                    stepAttempt.Description = "Incorrect action and component.";
                else if (action == null)
                    stepAttempt.Description = "Incorrect action for this step.";
                else if (action == null)
                    stepAttempt.Description = "Incorrect component for this step.";
            }

            #endregion

            #region Both conditions pass, so update step attempt to completed

            await _unitOfWork.SectionPracticeAttemptStepRepository.UpdateAsync(stepAttempt);
            await _unitOfWork.SaveChangesAsync();
            #endregion

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
