using Lssctc.Share.Entities;
using Lssctc.Share.Interfaces;
using Lssctc.SimulationManagement.TraineePractices.Dtos;
using Microsoft.EntityFrameworkCore;

namespace Lssctc.SimulationManagement.TraineePractices.Services
{
    public class TraineeStepService : ITraineeStepService
    {
        private readonly IUnitOfWork _unitOfWork;
        public TraineeStepService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        public async Task<TraineeStepDto?> GetTraineeStepByIdAndTraineeId(int stepId, int traineeId)
        {
            var lrp = await _unitOfWork.LearningRecordPartitionRepository
                .GetAllAsQueryable()
                .Where(lrp => 
                lrp.LearningRecord.TrainingProgress.CourseMember.TraineeId == traineeId
                && lrp.SectionPartition.PartitionTypeId == 4
                )
                .FirstOrDefaultAsync();
            if (lrp == null)
            {
                throw new KeyNotFoundException($"No LearningRecordPartition found for Trainee ID {traineeId} with PartitionTypeId 4.");
            }

            // PracticeStep
            var practiceStep = await _unitOfWork.PracticeStepRepository
                .GetAllAsQueryable()
                .Where(ps => ps.Id == stepId && ps.IsDeleted != true
                    && ps.Practice.SectionPractices.Any(sp => sp.SectionPartition.Id == lrp.SectionPartitionId)
                )
                .Include(ps => ps.Practice)
                .FirstOrDefaultAsync();
            if (practiceStep == null)
                throw new KeyNotFoundException($"No PracticeStep found with ID {stepId} associated with Trainee ID {traineeId}.");
            
            // Practice
            var practice = practiceStep.Practice;
            if (practice == null)
                throw new InvalidOperationException($"PracticeStep with ID {stepId} has no associated Practice.");
            
            // SectionPracticeAttemptStep
            var attemptStep = await _unitOfWork.SectionPracticeAttemptStepRepository
                .GetAllAsQueryable()
                .Where(spas => spas.PracticeStepId == stepId
                    && spas.Attempt.LearningRecordPartitionId == lrp.Id
                    && spas.IsDeleted != true)
                .FirstOrDefaultAsync();
            if (attemptStep == null)
                attemptStep = new SectionPracticeAttemptStep { IsPass = false }; // Default if not attempted

            // SimAction via PracticeStepAction
            var action = await _unitOfWork.PracticeStepActionRepository
                .GetAllAsQueryable()
                .Where(psa => psa.StepId == stepId && psa.IsDeleted != true)
                .Select(psa => psa.Action)
                .FirstOrDefaultAsync();
            if (action == null)
                throw new InvalidOperationException($"No SimAction assigned to PracticeStep ID {stepId}.");
            
            // SimulationComponent via PracticeStepComponent
            var component = await _unitOfWork.PracticeStepComponentRepository
                .GetAllAsQueryable()
                .Where(psc => psc.StepId == stepId && psc.IsDeleted != true)
                .Select(psc => psc.Component)
                .FirstOrDefaultAsync();
            if (component == null)
                throw new InvalidOperationException($"No SimulationComponent assigned to PracticeStep ID {stepId}.");

            // Map to DTO
            var dto = new TraineeStepDto
            {
                StepId = practiceStep.Id,
                StepName = practiceStep.StepName ?? "Step Name",
                StepDescription = practiceStep.StepDescription ?? "Step Description",
                ExpectedResult = practiceStep.ExpectedResult ?? "Expected Result",
                StepOrder = practiceStep.StepOrder,
                IsCompleted = attemptStep.IsPass ?? false,
                PracticeId = practice.Id,
                ActionId = action.Id,
                ActionName = action.Name,
                ActionDescription = action.Description,
                ActionKey = action.ActionKey,
                ComponentId = component.Id,
                ComponentName = component.Name,
                ComponentDescription = component.Description,
                ComponentImageUrl = component.ImageUrl,
            };
            return dto;
        }

        public Task<List<TraineeStepDto>> GetTraineeStepsByPracticeIdAndTraineeId(int practiceId, int traineeId)
        {
            throw new NotImplementedException();
        }
    }
}
