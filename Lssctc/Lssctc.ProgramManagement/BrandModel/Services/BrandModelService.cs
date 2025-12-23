using Lssctc.ProgramManagement.BrandModel.DTOs;
using Lssctc.Share.Common;
using Lssctc.Share.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace Lssctc.ProgramManagement.BrandModel.Services
{
    public class BrandModelService : IBrandModelService
    {
        private readonly IUnitOfWork _uow;

        public BrandModelService(IUnitOfWork uow)
        {
            _uow = uow;
        }

        public async Task<BrandModelDto?> GetBrandModelById(int id, CancellationToken cancellationToken = default)
        {
            var brandModel = await _uow.BrandModelRepository.GetAllAsQueryable()
                .Where(b => b.Id == id && b.IsDeleted != true)
                .Select(b => new BrandModelDto
                {
                    Id = b.Id,
                    Name = b.Name,
                    Description = b.Description,
                    Manufacturer = b.Manufacturer,
                    CountryOfOrigin = b.CountryOfOrigin,
                    IsActive = b.IsActive,
                    IsDeleted = b.IsDeleted
                })
                .FirstOrDefaultAsync(cancellationToken);

            return brandModel;
        }

        public async Task<PagedResult<BrandModelDto>> GetAllBrandModels(int page, int pageSize, CancellationToken cancellationToken = default)
        {
            if (page < 1) page = 1;
            if (pageSize < 1) pageSize = 10;

            var query = _uow.BrandModelRepository.GetAllAsQueryable()
                .Where(b => b.IsDeleted != true);

            var totalCount = await query.CountAsync(cancellationToken);

            var items = await query
                .OrderBy(b => b.Id)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(b => new BrandModelDto
                {
                    Id = b.Id,
                    Name = b.Name,
                    Description = b.Description,
                    Manufacturer = b.Manufacturer,
                    CountryOfOrigin = b.CountryOfOrigin,
                    IsActive = b.IsActive,
                    IsDeleted = b.IsDeleted
                })
                .ToListAsync(cancellationToken);

            return new PagedResult<BrandModelDto>
            {
                Items = items,
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize
            };
        }

        public async Task<int> CreateBrandModel(CreateBrandModelDto dto, CancellationToken cancellationToken = default)
        {
            if (dto == null) throw new ValidationException("Body is required.");

            var rawName = (dto.Name ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(rawName))
                throw new ValidationException("Name is required.");

            if (rawName.Length > 200)
                throw new ValidationException("Name must be at most 200 characters.");

            var normalizedName = string.Join(' ', rawName.Split(' ', StringSplitOptions.RemoveEmptyEntries));

            // Check duplicate name
            var nameExists = await _uow.BrandModelRepository.ExistsAsync(b => 
                b.Name.ToLower() == normalizedName.ToLower() && b.IsDeleted != true);
            if (nameExists)
                throw new ValidationException("A brand model with the same name already exists.");

            var entity = new Share.Entities.BrandModel
            {
                Name = normalizedName,
                Description = dto.Description,
                Manufacturer = dto.Manufacturer,
                CountryOfOrigin = dto.CountryOfOrigin,
                IsActive = true,
                IsDeleted = false
            };

            await _uow.BrandModelRepository.CreateAsync(entity);
            await _uow.SaveChangesAsync();

            return entity.Id;
        }

        public async Task<bool> UpdateBrandModel(int id, UpdateBrandModelDto dto, CancellationToken cancellationToken = default)
        {
            if (dto == null) throw new ValidationException("Body is required.");

            var entity = await _uow.BrandModelRepository.GetByIdAsync(id);
            if (entity == null || entity.IsDeleted == true) return false;

            if (!string.IsNullOrEmpty(dto.Name))
            {
                var rawName = dto.Name.Trim();
                if (string.IsNullOrWhiteSpace(rawName))
                    throw new ValidationException("Name cannot be empty.");
                if (rawName.Length > 200)
                    throw new ValidationException("Name must be at most 200 characters.");

                var normalizedName = string.Join(' ', rawName.Split(' ', StringSplitOptions.RemoveEmptyEntries));

                // Check duplicate name (excluding current entity)
                var nameExists = await _uow.BrandModelRepository.ExistsAsync(b => 
                    b.Id != id && b.Name.ToLower() == normalizedName.ToLower() && b.IsDeleted != true);
                if (nameExists)
                    throw new ValidationException("A brand model with the same name already exists.");

                entity.Name = normalizedName;
            }

            if (dto.Description != null)
            {
                if (dto.Description.Length > 1000)
                    throw new ValidationException("Description must be at most 1000 characters.");
                entity.Description = dto.Description;
            }

            if (dto.Manufacturer != null)
            {
                if (dto.Manufacturer.Length > 200)
                    throw new ValidationException("Manufacturer must be at most 200 characters.");
                entity.Manufacturer = dto.Manufacturer;
            }

            if (dto.CountryOfOrigin != null)
            {
                if (dto.CountryOfOrigin.Length > 100)
                    throw new ValidationException("CountryOfOrigin must be at most 100 characters.");
                entity.CountryOfOrigin = dto.CountryOfOrigin;
            }

            if (dto.IsActive.HasValue)
            {
                entity.IsActive = dto.IsActive.Value;
            }

            await _uow.BrandModelRepository.UpdateAsync(entity);
            await _uow.SaveChangesAsync();

            return true;
        }

        public async Task<bool> DeleteBrandModel(int id, CancellationToken cancellationToken = default)
        {
            var entity = await _uow.BrandModelRepository.GetByIdAsync(id);
            if (entity == null || entity.IsDeleted == true) return false;

            // Check if there are any simulation components using this brand model
            var hasComponents = await _uow.SimulationComponentRepository.ExistsAsync(sc => 
                sc.BrandModelId == id && sc.IsDeleted != true);
            if (hasComponents)
                throw new ValidationException("Cannot delete brand model that has simulation components.");

            // Soft delete
            entity.IsDeleted = true;
            entity.IsActive = false;

            await _uow.BrandModelRepository.UpdateAsync(entity);
            await _uow.SaveChangesAsync();

            return true;
        }
    }
}
