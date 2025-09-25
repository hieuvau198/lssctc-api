using AutoMapper;
using Lssctc.Share.Common;
using Lssctc.Share.Entities;
using Lssctc.Share.Interfaces;
using Lssctc.SimulationManagement.Practices.Dtos;
using System.Linq.Expressions;

namespace Lssctc.SimulationManagement.Practices.Services
{
    public class PracticeService : IPracticeService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public PracticeService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<PagedResult<PracticeDto>> GetAllAsync(PracticeQueryDto query)
        {
            var queryable = _unitOfWork.PracticeRepository.GetAllAsQueryable()
                .Where(x => x.IsDeleted != true);

            var pagedResult = await queryable.ToPagedResultAsync(query.Page, query.PageSize);

            var dtos = _mapper.Map<IEnumerable<PracticeDto>>(pagedResult.Items);

            return new PagedResult<PracticeDto>
            {
                Items = dtos,
                TotalCount = pagedResult.TotalCount,
                Page = pagedResult.Page,
                PageSize = pagedResult.PageSize
            };
        }


        public async Task<PracticeDto?> GetByIdAsync(int id)
        {
            var entity = await _unitOfWork.PracticeRepository.GetByIdAsync(id);

            if (entity == null || entity.IsDeleted == true)
                return null;

            return _mapper.Map<PracticeDto>(entity);
        }

        public async Task<PracticeDto> CreateAsync(CreatePracticeDto dto)
        {
            if (await ExistsAsync(dto.PracticeName))
            {
                throw new InvalidOperationException($"A practice with name '{dto.PracticeName}' already exists.");
            }

            var entity = _mapper.Map<Practice>(dto);
            var createdEntity = await _unitOfWork.PracticeRepository.CreateAsync(entity);
            await _unitOfWork.SaveChangesAsync();

            return _mapper.Map<PracticeDto>(createdEntity);
        }

        public async Task<PracticeDto?> UpdateAsync(int id, UpdatePracticeDto dto)
        {
            var existingEntity = await _unitOfWork.PracticeRepository.GetByIdAsync(id);

            if (existingEntity == null || existingEntity.IsDeleted == true)
                return null;

            if (dto.PracticeName != null && await ExistsAsync(dto.PracticeName, id))
            {
                throw new InvalidOperationException($"A practice with name '{dto.PracticeName}' already exists.");
            }

            _mapper.Map(dto, existingEntity);

            await _unitOfWork.PracticeRepository.UpdateAsync(existingEntity);
            await _unitOfWork.SaveChangesAsync();

            return _mapper.Map<PracticeDto>(existingEntity);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var entity = await _unitOfWork.PracticeRepository.GetByIdAsync(id);

            if (entity == null || entity.IsDeleted == true)
                return false;

            entity.IsDeleted = true;
            await _unitOfWork.PracticeRepository.UpdateAsync(entity);
            await _unitOfWork.SaveChangesAsync();

            return true;
        }

        public async Task<bool> ExistsAsync(int id)
        {
            return await _unitOfWork.PracticeRepository
                .ExistsAsync(x => x.Id == id && x.IsDeleted != true);
        }

        public async Task<bool> ExistsAsync(string practiceName, int? excludeId = null)
        {
            Expression<Func<Practice, bool>> predicate = x =>
                x.PracticeName == practiceName && x.IsDeleted != true;

            if (excludeId.HasValue)
                predicate = x => x.PracticeName == practiceName && x.IsDeleted != true && x.Id != excludeId.Value;

            return await _unitOfWork.PracticeRepository.ExistsAsync(predicate);
        }
    }
}
