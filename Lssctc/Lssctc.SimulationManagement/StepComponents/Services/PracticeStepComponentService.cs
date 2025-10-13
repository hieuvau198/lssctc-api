using AutoMapper;
using Lssctc.Share.Entities;
using Lssctc.Share.Interfaces;
using Lssctc.SimulationManagement.StepComponents.Dtos;
using Microsoft.EntityFrameworkCore;

namespace Lssctc.SimulationManagement.StepComponents.Services
{
    public class PracticeStepComponentService : IPracticeStepComponentService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public PracticeStepComponentService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        // 1. Get by PracticeStepId
        public async Task<List<PracticeStepComponentDto>> GetByPracticeStepIdAsync(int practiceStepId)
        {
            var list = await _unitOfWork.PracticeStepComponentRepository
                .GetAllAsQueryable()
                .Include(x => x.Component)
                .Include(x => x.Step)
                .Where(x => x.StepId == practiceStepId && x.IsDeleted != true)
                .OrderBy(x => x.ComponentOrder)
                
                .ToListAsync();
            return _mapper.Map<List<PracticeStepComponentDto>>(list);
        }

        // 2. Assign SimulationComponent to PracticeStep (create)
        public async Task<PracticeStepComponentDto> AssignSimulationComponentAsync(CreatePracticeStepComponentDto dto)
        {
            if (dto == null) 
                throw new ArgumentNullException(nameof(dto));
            var existingPsc = await _unitOfWork.PracticeStepComponentRepository
                .GetAllAsQueryable()
                .FirstOrDefaultAsync(x => x.StepId == dto.PracticeStepId && x.IsDeleted != true);
            if(existingPsc != null)
                throw new InvalidOperationException($"One Component has already been assigned for PracticeStepId {dto.PracticeStepId}.");

            var stepExists = await _unitOfWork.PracticeStepRepository
                .GetAllAsQueryable()
                .AnyAsync(s => s.Id == dto.PracticeStepId && s.IsDeleted != true);
            if (!stepExists)
                throw new InvalidOperationException($"No PracticeStep found with ID {dto.PracticeStepId}.");
            var componentExists = await _unitOfWork.SimulationComponentRepository
                .GetAllAsQueryable()
                .AnyAsync(c => c.Id == dto.SimulationComponentId && c.IsDeleted != true);
            if (!componentExists)
                throw new InvalidOperationException($"No SimulationComponent found with ID {dto.SimulationComponentId}.");

            var entity = _mapper.Map<PracticeStepComponent>(dto);
            entity.IsDeleted = false;
            var created = await _unitOfWork.PracticeStepComponentRepository.CreateAsync(entity);
            await _unitOfWork.SaveChangesAsync();
            // Reload with .Include for mapping
            created = await _unitOfWork.PracticeStepComponentRepository
                .GetAllAsQueryable()
                .Include(x => x.Component)
                .FirstAsync(x => x.Id == created.Id);
            return _mapper.Map<PracticeStepComponentDto>(created);
        }

        // 3. Update ComponentOrder (move up/down)
        public async Task<PracticeStepComponentDto?> UpdateOrderAsync(int id, UpdatePracticeStepComponentDto dto)
        {
            var entity = await _unitOfWork.PracticeStepComponentRepository.GetByIdAsync(id);
            if (entity == null || entity.IsDeleted == true)
                throw new KeyNotFoundException($"PracticeStepComponent with ID {id} not found.");

            _mapper.Map(dto, entity);
            await _unitOfWork.PracticeStepComponentRepository.UpdateAsync(entity);
            await _unitOfWork.SaveChangesAsync();
            // reload
            entity = await _unitOfWork.PracticeStepComponentRepository
                .GetAllAsQueryable()
                .Include(x => x.Component)
                .FirstAsync(x => x.Id == entity.Id);
            return _mapper.Map<PracticeStepComponentDto>(entity);
        }

        // 4. Remove (soft delete)
        public async Task<bool> RemoveAsync(int id)
        {
            var entity = await _unitOfWork.PracticeStepComponentRepository.GetByIdAsync(id);
            if (entity == null || entity.IsDeleted == true)
                return false;

            entity.IsDeleted = true;
            await _unitOfWork.PracticeStepComponentRepository.UpdateAsync(entity);
            await _unitOfWork.SaveChangesAsync();
            return true;
        }
    }

}
