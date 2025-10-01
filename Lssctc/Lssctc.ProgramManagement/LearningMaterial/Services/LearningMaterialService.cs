using AutoMapper;
using AutoMapper.QueryableExtensions;
using Lssctc.ProgramManagement.LearningMaterial.DTOs;
using Lssctc.Share.Common;
using Lssctc.Share.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using Entities = Lssctc.Share.Entities;
namespace Lssctc.ProgramManagement.LearningMaterial.Services
{
    public class LearningMaterialService : ILearningMaterialService
    {
        private readonly IUnitOfWork _uow;
        private readonly IMapper _mapper;

        public LearningMaterialService(IUnitOfWork uow, IMapper mapper)
        {
            _uow = uow;
            _mapper = mapper;
        }

        // Get all (no paging)
        public async Task<IReadOnlyList<LearningMaterialDto>> GetLearningMaterialsNoPagination()
        {
            return await _uow.LearningMaterialRepository.GetAllAsQueryable()
                .OrderByDescending(x => EF.Property<int>(x, "Id"))
                .ProjectTo<LearningMaterialDto>(_mapper.ConfigurationProvider)
                .AsNoTracking()
                .ToListAsync();
        }

        // Get all with paging
        public async Task<PagedResult<LearningMaterialDto>> GetLearningMaterialsPagination(int pageIndex, int pageSize)
        {
            if (pageIndex < 1) pageIndex = 1;
            if (pageSize < 1 || pageSize > 200) pageSize = 20;

            var q = _uow.LearningMaterialRepository.GetAllAsQueryable();
            var total = await q.CountAsync();

            var items = await q
                .OrderByDescending(x => EF.Property<int>(x, "Id"))
                .Skip((pageIndex - 1) * pageSize)
                .Take(pageSize)
                .ProjectTo<LearningMaterialDto>(_mapper.ConfigurationProvider)
                .AsNoTracking()
                .ToListAsync();

            return new PagedResult<LearningMaterialDto>
            {
                Items = items,
                TotalCount = total,
                Page = pageIndex,
                PageSize = pageSize
            };
        }

        public async Task<LearningMaterialDto?> GetLearningMaterialById(int id)
        {
            var dto = await _uow.LearningMaterialRepository.GetAllAsQueryable()
                .Where(x => EF.Property<int>(x, "Id") == id)
                .ProjectTo<LearningMaterialDto>(_mapper.ConfigurationProvider)
                .AsNoTracking()
                .FirstOrDefaultAsync();
            return dto;
        }

        public async Task<int> CreateLearningMaterial(CreateLearningMaterialDto dto)
        {
            if (dto == null) throw new ValidationException("Body is required.");
            if (dto.LearningMaterialTypeId <= 0)
                throw new ValidationException("LearningMaterialTypeId is invalid.");

            // ✅ Validate FK: type phải tồn tại
            var typeExists = await _uow.LearningMaterialTypeRepository
                .ExistsAsync(t => EF.Property<int>(t, "Id") == dto.LearningMaterialTypeId);
            if (!typeExists)
                throw new KeyNotFoundException(
                    $"LearningMaterialType {dto.LearningMaterialTypeId} not found. Seed types (PDF/Video/Image/URL) first.");

            // (tuỳ chọn) validate nhẹ name/url
            var name = (dto.Name ?? "").Trim();
            if (string.IsNullOrWhiteSpace(name)) throw new ValidationException("Name is required.");
            if (name.Length > 100) throw new ValidationException("Name must be at most 100 characters.");

            var url = (dto.MaterialUrl ?? "").Trim();
            if (string.IsNullOrWhiteSpace(url)) throw new ValidationException("MaterialUrl is required.");
            if (url.Length > 2000) throw new ValidationException("MaterialUrl must be at most 2000 characters.");

            var entity = _mapper.Map<Entities.LearningMaterial>(dto);
            await _uow.LearningMaterialRepository.CreateAsync(entity);
            await _uow.SaveChangesAsync();
            return entity.Id;
        }


        public async Task<bool> UpdateLearningMaterial(int id, UpdateLearningMaterialDto dto)
        {
            var entity = await _uow.LearningMaterialRepository.GetByIdAsync(id);
            if (entity == null) return false;
            // Validate LearningMaterialTypeId
            if ( dto.LearningMaterialTypeId.HasValue)
            {
                if(dto.LearningMaterialTypeId.Value <= 0) throw new ValidationException("LearningMaterialTypeId is invalid.");
                var typeExists = await _uow.LearningMaterialTypeRepository.ExistsAsync(t => EF.Property<int>(t, "Id") == dto.LearningMaterialTypeId.Value);
                if(!typeExists) throw new KeyNotFoundException($"LearningMaterialType {dto.LearningMaterialTypeId.Value} not found. Seed types (PDF/Video/Image/URL) first.");
            }
            // Validate Name
            if(dto.Name != null)
            {
                var raw = dto.Name.Trim();
                var normalizedName = string.Join(" ", raw.Split(' ', StringSplitOptions.RemoveEmptyEntries));

                if(string.IsNullOrWhiteSpace(normalizedName)) throw new ValidationException("Name is required.");
                if(normalizedName.Length > 100) throw new ValidationException("Name must be at most 100 characters.");

                dto.Name = normalizedName;

            }

            if (dto.Description != null)
            {
                if (dto.Description.Length > 2000)
                    throw new ValidationException("Description must be at most 2000 characters.");
            }

            _mapper.Map(dto, entity);
            await _uow.LearningMaterialRepository.UpdateAsync(entity);
            await _uow.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteLearningMaterial(int id)
        {
            var entity = await _uow.LearningMaterialRepository.GetByIdAsync(id);
            if (entity == null) return false;

            await _uow.LearningMaterialRepository.DeleteAsync(entity);
            await _uow.SaveChangesAsync();
            return true;
        }
    }
}
