using Lssctc.Share.Entities;
using Lssctc.Share.Interfaces;
using Lssctc.SimulationManagement.StepActions.Dtos;
using Microsoft.EntityFrameworkCore;

namespace Lssctc.SimulationManagement.StepActions.Services
{
    public class StepActionService : IStepActionService
    {
        private readonly IUnitOfWork _unitOfWork;
        public StepActionService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        public async Task<List<StepActionDto>> GetAllStepActionsAsync()
        {
            var stepActions = await _unitOfWork.PracticeStepActionRepository
                .GetAllAsQueryable()
                .Where(sa => sa.IsDeleted != true)
                .ToListAsync();
            var dtos = new List<StepActionDto>();
            if (stepActions == null || stepActions.Count == 0)
            {
                return dtos;
            }
            return stepActions.Select(MapToDto).ToList()
                ?? throw new InvalidOperationException("Mapping to DTO resulted in null.");
        }

        public async Task<StepActionDto?> GetStepActionByStepId(int stepId)
        {
            if (stepId <= 0)
            {
                throw new ArgumentException("Step ID must be a positive integer.", nameof(stepId));
            }
            var stepAction = await _unitOfWork.PracticeStepActionRepository
                .GetAllAsQueryable()
                .FirstOrDefaultAsync(sa => sa.StepId == stepId && sa.IsDeleted != true);
            if (stepAction == null)
                throw new KeyNotFoundException($"No StepAction found for Step ID {stepId}.");
            return MapToDto(stepAction);
        }

        public async Task<StepActionDto?> GetStepActionByIdAsync(int id)
        {
            var stepAction = await _unitOfWork.PracticeStepActionRepository
                .GetAllAsQueryable()
                .FirstOrDefaultAsync(sa => sa.Id == id && sa.IsDeleted != true);
            if (stepAction == null)
                throw new KeyNotFoundException($"StepAction with ID {id} not found.");
            return MapToDto(stepAction);
        }

        public async Task<StepActionDto> CreateStepActionAsync(CreateStepActionDto dto)
        {
            if (dto == null || dto.StepId <= 0 || dto.ActionId <= 0)
            {
                throw new ArgumentNullException(nameof(dto));
            }
            var existingStepAction = await _unitOfWork.PracticeStepActionRepository
                .GetAllAsQueryable()
                .FirstOrDefaultAsync(sa => sa.StepId == dto.StepId && sa.ActionId == dto.ActionId && sa.IsDeleted != true);
            if (existingStepAction != null)
            {
                throw new InvalidOperationException("A StepAction with the same StepId and ActionId already exists.");
            }
            var newStepAction = new PracticeStepAction
            {
                StepId = dto.StepId,
                ActionId = dto.ActionId,
                Name = dto.StepActionName ?? "Step Action Name",
                Description = dto.StepActionDescription ?? "Step Action Description",
                IsDeleted = false
            };
            await _unitOfWork.PracticeStepActionRepository.CreateAsync(newStepAction);
            await _unitOfWork.SaveChangesAsync();
            return MapToDto(newStepAction);
        }

        public async Task<bool> DeleteStepActionAsync(int id)
        {
            if (id <= 0)
            {
                throw new ArgumentException("ID must be a positive integer.", nameof(id));
            }
            var stepAction = await _unitOfWork.PracticeStepActionRepository
                .GetAllAsQueryable()
                .FirstOrDefaultAsync(sa => sa.Id == id && sa.IsDeleted != true);
            if (stepAction == null)
                throw new KeyNotFoundException($"StepAction with ID {id} not found.");
            stepAction.IsDeleted = true;
            await _unitOfWork.PracticeStepActionRepository.UpdateAsync(stepAction);
            await _unitOfWork.SaveChangesAsync();
            return true;
        }

        public async Task<StepActionDto?> UpdateStepActionAsync(int id, UpdateStepActionDto dto)
        {
            if (id <= 0 || dto == null)
            {
                throw new ArgumentException("Invalid ID or DTO.", nameof(id));
            }
            var stepAction = await _unitOfWork.PracticeStepActionRepository
                .GetAllAsQueryable()
                .FirstOrDefaultAsync(sa => sa.Id == id && sa.IsDeleted != true);
            if (stepAction == null)
                throw new KeyNotFoundException($"StepAction with ID {id} not found.");
            if (dto.StepId > 0)
            {
                var stepExists = await _unitOfWork.PracticeStepRepository
                    .GetAllAsQueryable()
                    .AnyAsync(s => s.Id == dto.StepId 
                    && !s.PracticeStepActions.Any()
                    && s.IsDeleted != true);
                if (!stepExists)
                    throw new InvalidOperationException($"No available PracticeStep found with ID {dto.StepId}, or this step already has action assigned.");
                stepAction.StepId = dto.StepId;
            }
            if(dto.ActionId > 0)
            {
                var actionExists = await _unitOfWork.SimActionRepository
                    .GetAllAsQueryable()
                    .AnyAsync(a => a.Id == dto.ActionId && a.IsDeleted != true);
                if (!actionExists)
                    throw new InvalidOperationException($"No SimAction found with ID {dto.ActionId}.");
                stepAction.ActionId = dto.ActionId;
            }
            
            stepAction.Name = dto.StepActionName ?? stepAction.Name;
            stepAction.Description = dto.StepActionDescription ?? stepAction.Description;
            await _unitOfWork.PracticeStepActionRepository.UpdateAsync(stepAction);
            await _unitOfWork.SaveChangesAsync();
            return MapToDto(stepAction);
        }

        private StepActionDto MapToDto(PracticeStepAction entity)
        {
            if (entity == null)
            {
                throw new ArgumentNullException(nameof(entity));
            }
            return new StepActionDto
            {
                StepActionId = entity.Id,
                StepId = entity.StepId,
                ActionId = entity.ActionId,
                StepActionName = entity.Name,
                StepActionDescription = entity.Description
            };
        }
    }
}
