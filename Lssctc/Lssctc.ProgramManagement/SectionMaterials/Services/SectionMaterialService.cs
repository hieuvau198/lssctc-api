using Lssctc.ProgramManagement.SectionMaterials.DTOs;
using Lssctc.Share.Common;
using Lssctc.Share.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using Entities = Lssctc.Share.Entities;

namespace Lssctc.ProgramManagement.SectionMaterials.Services
{
    public class SectionMaterialService : ISectionMaterialService
    {
        private readonly IUnitOfWork _uow;

        public SectionMaterialService(IUnitOfWork uow)
        {
            _uow = uow;
        }

        //map respone
        private static SectionMaterialDto MapToDto(Entities.SectionMaterial entity)
        {
            return new SectionMaterialDto
            {
                Id = entity.Id,
                SectionPartitionId = entity.SectionPartitionId,
                LearningMaterialId = entity.LearningMaterialId,
                Name = entity.Name,
                Description = entity.Description
            };
        }

        private static Entities.SectionMaterial MapToEntity(CreateSectionMaterialDto dto)
        {
            return new Entities.SectionMaterial
            {
                SectionPartitionId = dto.SectionPartitionId,
                LearningMaterialId = dto.LearningMaterialId,
                Name = dto.Name ?? "",
                Description = dto.Description ?? ""
            };
        }

        // map to create SectionMaterial (UpsertSectionMaterial)
        private static Entities.SectionMaterial MapToEntity(UpsertSectionMaterialDto dto)
        {
            return new Entities.SectionMaterial
            {
                SectionPartitionId = dto.SectionPartitionId,
                LearningMaterialId = dto.LearningMaterialId,
                Name = dto.Name ?? "",
                Description = dto.Description ?? ""
            };
        }

        private static void MapToEntity(UpdateSectionMaterialDto dto, Entities.SectionMaterial entity)
        {
            if (dto.SectionPartitionId.HasValue)
                entity.SectionPartitionId = dto.SectionPartitionId.Value;
            
            if (dto.LearningMaterialId.HasValue)
                entity.LearningMaterialId = dto.LearningMaterialId.Value;
            
            if (dto.Name != null)
                entity.Name = dto.Name;
            
            if (dto.Description != null)
                entity.Description = dto.Description;
        }

        //map to update (UpsertSectionMaterial )
        private static void MapToEntity(UpsertSectionMaterialDto dto, Entities.SectionMaterial entity)
        {
            entity.SectionPartitionId = dto.SectionPartitionId;
            entity.LearningMaterialId = dto.LearningMaterialId;
            
            if (dto.Name != null)
                entity.Name = dto.Name;
            
            if (dto.Description != null)
                entity.Description = dto.Description;
        }

        public async Task<PagedResult<SectionMaterialDto>> GetSectionMaterialsPaged(int pageIndex, int pageSize)
        {
            if (pageIndex < 1) pageIndex = 1;
            if (pageSize < 1 || pageSize > 200) pageSize = 20;

            var q = _uow.SectionMaterialRepository.GetAllAsQueryable();

            var total = await q.CountAsync();

            var entities = await q
                .OrderByDescending(x => EF.Property<int>(x, "Id"))
                .Skip((pageIndex - 1) * pageSize)
                .Take(pageSize)
                .AsNoTracking()
                .ToListAsync();

            var items = entities.Select(MapToDto).ToList();

            return new PagedResult<SectionMaterialDto>
            {
                Items = items,
                TotalCount = total,
                Page = pageIndex,
                PageSize = pageSize
            };
        }

        public async Task<IReadOnlyList<SectionMaterialDto>> GetAllSectionMaterialsAsync()
        {
            var entities = await _uow.SectionMaterialRepository.GetAllAsQueryable()
                .OrderByDescending(x => EF.Property<int>(x, "Id"))
                .AsNoTracking()
                .ToListAsync();

            return entities.Select(MapToDto).ToList();
        }

        public async Task<SectionMaterialDto?> GetSectionMateriaById(int id)
        {
            var entity = await _uow.SectionMaterialRepository.GetAllAsQueryable()
                .Where(x => EF.Property<int>(x, "Id") == id)
                .FirstOrDefaultAsync();

            return entity != null ? MapToDto(entity) : null;
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

            var entity = MapToEntity(dto);
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

            MapToEntity(dto, entity);
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

        public async Task<SectionMaterialDto> UpsertSectionMaterial(UpsertSectionMaterialDto dto)
        {
            if (dto == null) throw new ValidationException("Body is required.");
            if (dto.SectionPartitionId <= 0) throw new ValidationException("SectionPartitionId is invalid.");
            if (dto.LearningMaterialId <= 0) throw new ValidationException("LearningMaterialId is invalid.");

            // Validate that SectionPartition exists
            if (!await _uow.SectionPartitionRepository.ExistsAsync(s => s.Id == dto.SectionPartitionId))
                throw new KeyNotFoundException($"SectionPartition {dto.SectionPartitionId} not found.");

            // Validate that LearningMaterial exists
            if (!await _uow.LearningMaterialRepository.ExistsAsync(m => m.Id == dto.LearningMaterialId))
                throw new KeyNotFoundException($"LearningMaterial {dto.LearningMaterialId} not found.");

            // Check if SectionMaterial already exists for this combination
            var existingEntity = await _uow.SectionMaterialRepository.GetAllAsQueryable()
                .Where(sm => sm.SectionPartitionId == dto.SectionPartitionId && sm.LearningMaterialId == dto.LearningMaterialId)
                .FirstOrDefaultAsync();

            if (existingEntity != null)
            {
                // Update existing entity
                MapToEntity(dto, existingEntity);
                await _uow.SectionMaterialRepository.UpdateAsync(existingEntity);
                await _uow.SaveChangesAsync();
                return MapToDto(existingEntity);
            }
            else
            {
                // Create new entity
                var newEntity = MapToEntity(dto);
                await _uow.SectionMaterialRepository.CreateAsync(newEntity);
                await _uow.SaveChangesAsync();
                return MapToDto(newEntity);
            }
        }
    }
}
