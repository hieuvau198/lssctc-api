using AutoMapper;
using Lssctc.Share.Entities;
using Lssctc.Share.Interfaces;
using Lssctc.SimulationManagement.PracticeSteps.Dtos;
using Microsoft.EntityFrameworkCore;

namespace Lssctc.SimulationManagement.PracticeSteps.Services
{
    public class PracticeStepService : IPracticeStepService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public PracticeStepService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }
        public async Task<List<PracticeStepDto>> GetPracticeStepsByPracticeIdAsync(int practiceId)
        {
            var steps = await _unitOfWork.PracticeStepRepository
                .GetAllAsQueryable()
                .Where(x => x.PracticeId == practiceId && x.IsDeleted != true)
                .OrderBy(x => x.StepOrder)
                .ToListAsync();
            return _mapper.Map<List<PracticeStepDto>>(steps);
        }

        public async Task<PracticeStepDto?> GetPracticeStepByIdAsync(int id)
        {
            var entity = await _unitOfWork.PracticeStepRepository.GetByIdAsync(id);
            if (entity == null || entity.IsDeleted == true)
                return null;
            return _mapper.Map<PracticeStepDto>(entity);
        }

        public async Task<PracticeStepDto> CreatePracticeStepAsync(CreatePracticeStepDto dto)
        {
            var entity = _mapper.Map<PracticeStep>(dto);
            entity.IsDeleted = false;
            var created = await _unitOfWork.PracticeStepRepository.CreateAsync(entity);
            await _unitOfWork.SaveChangesAsync();
            return _mapper.Map<PracticeStepDto>(created);
        }

        public async Task<PracticeStepDto?> UpdatePracticeStepAsync(int id, UpdatePracticeStepDto dto)
        {
            var entity = await _unitOfWork.PracticeStepRepository.GetByIdAsync(id);
            if (entity == null || entity.IsDeleted == true)
                return null;

            _mapper.Map(dto, entity);
            await _unitOfWork.PracticeStepRepository.UpdateAsync(entity);
            await _unitOfWork.SaveChangesAsync();
            return _mapper.Map<PracticeStepDto>(entity);
        }

        public async Task<bool> DeletePracticeStepAsync(int id)
        {
            var entity = await _unitOfWork.PracticeStepRepository.GetByIdAsync(id);
            if (entity == null || entity.IsDeleted == true)
                return false;

            entity.IsDeleted = true;
            await _unitOfWork.PracticeStepRepository.UpdateAsync(entity);
            await _unitOfWork.SaveChangesAsync();
            return true;
        }
    }
}
