using AutoMapper;
using Lssctc.Share.Common;
using Lssctc.Share.Entities;
using Lssctc.Share.Interfaces;
using Lssctc.SimulationManagement.Components.Dtos;
using System.Linq.Expressions;

namespace Lssctc.SimulationManagement.Components.Services
{
    public class ComponentService : IComponentService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public ComponentService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<PagedResult<SimulationComponentDto>> GetAllAsync(SimulationComponentQueryDto query)
        {
            var queryable = _unitOfWork.SimulationComponentRepository.GetAllAsQueryable()
                .Where(x => x.IsDeleted != true);

            var pagedResult = await queryable.ToPagedResultAsync(query.Page, query.PageSize);

            var dtos = _mapper.Map<IEnumerable<SimulationComponentDto>>(pagedResult.Items);

            return new PagedResult<SimulationComponentDto>
            {
                Items = dtos,
                TotalCount = pagedResult.TotalCount,
                Page = pagedResult.Page,
                PageSize = pagedResult.PageSize
            };
        }

        public async Task<SimulationComponentDto?> GetByIdAsync(int id)
        {
            var entity = await _unitOfWork.SimulationComponentRepository.GetByIdAsync(id);

            if (entity == null || entity.IsDeleted == true)
                return null;

            return _mapper.Map<SimulationComponentDto>(entity);
        }

        public async Task<SimulationComponentDto> CreateAsync(CreateSimulationComponentDto dto)
        {
            if (await ExistsAsync(dto.Name))
            {
                throw new InvalidOperationException($"A component with name '{dto.Name}' already exists.");
            }

            var entity = _mapper.Map<SimulationComponent>(dto);

            var createdEntity = await _unitOfWork.SimulationComponentRepository.CreateAsync(entity);
            await _unitOfWork.SaveChangesAsync();

            return _mapper.Map<SimulationComponentDto>(createdEntity);
        }

        public async Task<SimulationComponentDto?> UpdateAsync(int id, UpdateSimulationComponentDto dto)
        {
            var existingEntity = await _unitOfWork.SimulationComponentRepository.GetByIdAsync(id);

            if (existingEntity == null || existingEntity.IsDeleted == true)
                return null;

            if (dto.Name != null && await ExistsAsync(dto.Name, id))
            {
                throw new InvalidOperationException($"A component with name '{dto.Name}' already exists.");
            }

            _mapper.Map(dto, existingEntity);

            await _unitOfWork.SimulationComponentRepository.UpdateAsync(existingEntity);
            await _unitOfWork.SaveChangesAsync();

            return _mapper.Map<SimulationComponentDto>(existingEntity);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var entity = await _unitOfWork.SimulationComponentRepository.GetByIdAsync(id);

            if (entity == null || entity.IsDeleted == true)
                return false;

            entity.IsDeleted = true;
            await _unitOfWork.SimulationComponentRepository.UpdateAsync(entity);
            await _unitOfWork.SaveChangesAsync();

            return true;
        }

        public async Task<bool> ExistsAsync(int id)
        {
            return await _unitOfWork.SimulationComponentRepository
                .ExistsAsync(x => x.Id == id && x.IsDeleted != true);
        }

        public async Task<bool> ExistsAsync(string name, int? excludeId = null)
        {
            Expression<Func<SimulationComponent, bool>> predicate = x =>
                x.Name == name && x.IsDeleted != true;

            if (excludeId.HasValue)
            {
                predicate = x => x.Name == name && x.IsDeleted != true && x.Id != excludeId.Value;
            }

            return await _unitOfWork.SimulationComponentRepository.ExistsAsync(predicate);
        }

    }
}
