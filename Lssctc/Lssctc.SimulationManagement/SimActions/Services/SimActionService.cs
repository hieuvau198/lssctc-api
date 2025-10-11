using Lssctc.Share.Entities;
using Lssctc.Share.Interfaces;
using Lssctc.SimulationManagement.SimActions.Dtos;
using Microsoft.EntityFrameworkCore;

namespace Lssctc.SimulationManagement.SimActions.Services
{
    public class SimActionService : ISimActionService
    {
        private readonly IUnitOfWork _unitOfWork;
        public SimActionService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<List<SimActionDto>> GetAllSimActionsAsync()
        {
            var actions = await _unitOfWork.SimActionRepository
                .GetAllAsQueryable()
                .Where(a => a.IsDeleted != true)
                .ToListAsync();
            var dtos = new List<SimActionDto>();
            if (actions == null || actions.Count == 0)
            {
                return dtos;
            }
            return actions.Select(MapToDto).ToList() 
                ?? throw new InvalidOperationException("Mapping to DTO resulted in null.")
            ;
        }
        public async Task<SimActionDto?> GetSimActionByIdAsync(int id)
        {
            var action = await _unitOfWork.SimActionRepository
                .GetAllAsQueryable()
                .FirstOrDefaultAsync(a => a.Id == id && a.IsDeleted != true);
            if (action == null)
            {
                return null;
            }
            return MapToDto(action);
        }
        public async Task<SimActionDto> CreateSimActionAsync(CreateSimActionDto dto)
        {
            if (dto == null || dto.ActionName == null || dto.ActionKey == null)
            {
                throw new ArgumentNullException(nameof(dto));
            }
            var existingAction = await _unitOfWork.SimActionRepository
                .GetAllAsQueryable()
                .FirstOrDefaultAsync(a => a.Name == dto.ActionName && a.ActionKey == dto.ActionKey);
            if (existingAction != null)
            {
                throw new InvalidOperationException("A SimAction with the same name and key already exists.");
            }
            var newAction = new SimAction
            {
                Name = dto.ActionName,
                Description = dto.ActionDescription,
                ActionKey = dto.ActionKey,
                IsActive = dto.IsActive ?? true,
                IsDeleted = false
            };
            await _unitOfWork.SimActionRepository.CreateAsync(newAction);
            await _unitOfWork.SaveChangesAsync();
            return MapToDto(newAction);

        }
        public async Task<SimActionDto?> UpdateSimActionAsync(int id, UpdateSimActionDto dto)
        {
            var action = await _unitOfWork.SimActionRepository
                .GetAllAsQueryable()
                .FirstOrDefaultAsync(a => a.Id == id && a.IsDeleted != true);
            if (action == null)
            {
                throw new KeyNotFoundException($"SimAction with ID {id} not found.");
            }
            action.Name = dto.ActionName ?? action.Name;
            action.Description = dto.ActionDescription ?? action.Description;
            action.ActionKey = dto.ActionKey ?? action.ActionKey;
            action.IsActive = dto.IsActive ?? action.IsActive;
            await _unitOfWork.SimActionRepository.UpdateAsync(action);
            await _unitOfWork.SaveChangesAsync();
            return MapToDto(action);
        }
        public async Task<bool> DeleteSimActionAsync(int id)
        {
            var action = await _unitOfWork.SimActionRepository
                .GetAllAsQueryable()
                .FirstOrDefaultAsync(a => a.Id == id && a.IsDeleted != true);
            if (action == null)
            {
                return false;
            }
            action.IsDeleted = true;
            await _unitOfWork.SimActionRepository.UpdateAsync(action);
            await _unitOfWork.SaveChangesAsync();
            return true;
        }


        private SimActionDto MapToDto(SimAction action)
        {
            return new SimActionDto
            {
                ActionId = action.Id,
                ActionName = action.Name,
                ActionDescription = action.Description,
                ActionKey = action.ActionKey,
                IsActive = action.IsActive
            };
        }
    }
}
