using AutoMapper;
using AutoMapper.QueryableExtensions;
using Lssctc.LearningManagement.SectionMaterial.DTOs;
using Lssctc.Share.Common;
using Lssctc.Share.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using Entities = Lssctc.Share.Entities;
namespace Lssctc.LearningManagement.SectionMaterial.Services
{
    public class SectionMaterialService : ISectionMaterialService
    {
        private readonly IUnitOfWork _uow;
        private readonly IMapper _mapper;

        public SectionMaterialService(IUnitOfWork uow, IMapper mapper)
        {
            _uow = uow;
            _mapper = mapper;
        }

        public async Task<PagedResult<SectionMaterialDto>> GetSectionMaterialsPaged(int pageIndex, int pageSize)
        {
            if (pageIndex < 1) pageIndex = 1;
            if (pageSize < 1 || pageSize > 200) pageSize = 20;

            var q = _uow.SectionMaterialRepository.GetAllAsQueryable();

            var total = await q.CountAsync();

            var items = await q
                .OrderByDescending(x => EF.Property<int>(x, "Id"))
                .Skip((pageIndex - 1) * pageSize)
                .Take(pageSize)
                .ProjectTo<SectionMaterialDto>(_mapper.ConfigurationProvider)
                .AsNoTracking()
                .ToListAsync();

            return new PagedResult<SectionMaterialDto>
            {
                Items = items,
                TotalCount = total,
                Page = pageIndex,
                PageSize = pageSize
            };
        }



        public async Task<SectionMaterialDto?> GetSectionMateriaById(int id)
        {
            return await _uow.SectionMaterialRepository.GetAllAsQueryable()
                .Where(x => EF.Property<int>(x, "Id") == id)
                .ProjectTo<SectionMaterialDto>(_mapper.ConfigurationProvider)
                .FirstOrDefaultAsync();
        }

        public async Task<int> CreateSectionMateria(CreateSectionMaterialDto dto)
        {
            if (dto == null) throw new ValidationException("Body is required.");
            if (dto.SectionPartitionId <= 0) throw new ValidationException("SectionPartitionId is invalid.");
            if (dto.LearningMaterialId <= 0) throw new ValidationException("LearningMaterialId is invalid.");

            if (!await _uow.SectionPartitionRepository.ExistsAsync(s => s.Id == dto.SectionPartitionId))
                throw new KeyNotFoundException($"SectionPartition {dto.SectionPartitionId} not found.");

            if (!await _uow.LearningMaterialRepository.ExistsAsync(m => m.Id == dto.LearningMaterialId))
                throw new KeyNotFoundException($"LearningMaterial {dto.LearningMaterialId} not found.");

            var entity = _mapper.Map<Entities.SectionMaterial>(dto);
            await _uow.SectionMaterialRepository.CreateAsync(entity);
            await _uow.SaveChangesAsync();
            return entity.Id;
        }

        public async Task<bool> UpdateSectionMateria(int id, UpdateSectionMaterialDto dto)
        {
            var entity = await _uow.SectionMaterialRepository.GetByIdAsync(id);
            if (entity == null) return false;

            if (dto.SectionPartitionId.HasValue)
            {
                var ok = await _uow.SectionPartitionRepository.ExistsAsync(s => s.Id == dto.SectionPartitionId.Value);
                if (!ok) throw new KeyNotFoundException($"SectionPartition {dto.SectionPartitionId.Value} not found.");
            }

            if (dto.LearningMaterialId.HasValue)
            {
                var ok = await _uow.LearningMaterialRepository.ExistsAsync(m => m.Id == dto.LearningMaterialId.Value);
                if (!ok) throw new KeyNotFoundException($"LearningMaterial {dto.LearningMaterialId.Value} not found.");
            }

            _mapper.Map(dto, entity);
            await _uow.SectionMaterialRepository.UpdateAsync(entity);
            await _uow.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteSectionMateria(int id)
        {
            var entity = await _uow.SectionMaterialRepository.GetByIdAsync(id);
            if (entity == null) return false;

            await _uow.SectionMaterialRepository.DeleteAsync(entity);
            await _uow.SaveChangesAsync();
            return true;
        }
    }
}
