using Lssctc.ProgramManagement.SimulationComponents.DTOs;
using Lssctc.Share.Common;
using Lssctc.Share.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace Lssctc.ProgramManagement.SimulationComponents.Services
{
    public class SimulationComponentService : ISimulationComponentService
    {
        private readonly IUnitOfWork _uow;

        public SimulationComponentService(IUnitOfWork uow)
        {
            _uow = uow;
        }

        public async Task<SimulationComponentDto?> GetSimulationComponentById(int id, CancellationToken cancellationToken = default)
        {
            var component = await _uow.SimulationComponentRepository.GetAllAsQueryable()
                .Where(sc => sc.Id == id && sc.IsDeleted != true)
                .Include(sc => sc.BrandModel)
                .Select(sc => new SimulationComponentDto
                {
                    Id = sc.Id,
                    BrandModelId = sc.BrandModelId,
                    BrandModelName = sc.BrandModel.Name,
                    Name = sc.Name,
                    Description = sc.Description,
                    ImageUrl = sc.ImageUrl,
                    IsActive = sc.IsActive,
                    CreatedDate = sc.CreatedDate,
                    IsDeleted = sc.IsDeleted
                })
                .FirstOrDefaultAsync(cancellationToken);

            return component;
        }

        public async Task<PagedResult<SimulationComponentDto>> GetAllSimulationComponents(
            int page, 
            int pageSize, 
            CancellationToken cancellationToken = default)
        {
            if (page < 1) page = 1;
            if (pageSize < 1) pageSize = 10;

            var query = _uow.SimulationComponentRepository.GetAllAsQueryable()
                .Where(sc => sc.IsDeleted != true)
                .Include(sc => sc.BrandModel);

            var totalCount = await query.CountAsync(cancellationToken);

            var items = await query
                .OrderBy(sc => sc.Id)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(sc => new SimulationComponentDto
                {
                    Id = sc.Id,
                    BrandModelId = sc.BrandModelId,
                    BrandModelName = sc.BrandModel.Name,
                    Name = sc.Name,
                    Description = sc.Description,
                    ImageUrl = sc.ImageUrl,
                    IsActive = sc.IsActive,
                    CreatedDate = sc.CreatedDate,
                    IsDeleted = sc.IsDeleted
                })
                .ToListAsync(cancellationToken);

            return new PagedResult<SimulationComponentDto>
            {
                Items = items,
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize
            };
        }

        public async Task<PagedResult<SimulationComponentDto>> GetSimulationComponentsByBrandModelId(
            int brandModelId, 
            int page, 
            int pageSize, 
            CancellationToken cancellationToken = default)
        {
            if (page < 1) page = 1;
            if (pageSize < 1) pageSize = 10;

            var query = _uow.SimulationComponentRepository.GetAllAsQueryable()
                .Where(sc => sc.BrandModelId == brandModelId && sc.IsDeleted != true)
                .Include(sc => sc.BrandModel);

            var totalCount = await query.CountAsync(cancellationToken);

            var items = await query
                .OrderBy(sc => sc.Id)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(sc => new SimulationComponentDto
                {
                    Id = sc.Id,
                    BrandModelId = sc.BrandModelId,
                    BrandModelName = sc.BrandModel.Name,
                    Name = sc.Name,
                    Description = sc.Description,
                    ImageUrl = sc.ImageUrl,
                    IsActive = sc.IsActive,
                    CreatedDate = sc.CreatedDate,
                    IsDeleted = sc.IsDeleted
                })
                .ToListAsync(cancellationToken);

            return new PagedResult<SimulationComponentDto>
            {
                Items = items,
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize
            };
        }

        public async Task<int> CreateSimulationComponent(CreateSimulationComponentDto dto, CancellationToken cancellationToken = default)
        {
            if (dto == null) throw new ValidationException("Body is required.");

            var rawName = (dto.Name ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(rawName))
                throw new ValidationException("Name is required.");

            if (rawName.Length > 100)
                throw new ValidationException("Name must be at most 100 characters.");

            var normalizedName = string.Join(' ', rawName.Split(' ', StringSplitOptions.RemoveEmptyEntries));

            // Check if BrandModel exists
            var brandModelExists = await _uow.BrandModelRepository.ExistsAsync(b => 
                b.Id == dto.BrandModelId && b.IsDeleted != true);
            if (!brandModelExists)
                throw new ValidationException("Brand model not found.");

            // Check duplicate name within the same brand model
            var nameExists = await _uow.SimulationComponentRepository.ExistsAsync(sc => 
                sc.BrandModelId == dto.BrandModelId && 
                sc.Name.ToLower() == normalizedName.ToLower() && 
                sc.IsDeleted != true);
            if (nameExists)
                throw new ValidationException("A simulation component with the same name already exists for this brand model.");

            var entity = new Share.Entities.SimulationComponent
            {
                BrandModelId = dto.BrandModelId,
                Name = normalizedName,
                Description = dto.Description,
                ImageUrl = dto.ImageUrl,
                IsActive = true,
                CreatedDate = DateTime.UtcNow,
                IsDeleted = false
            };

            await _uow.SimulationComponentRepository.CreateAsync(entity);
            await _uow.SaveChangesAsync();

            return entity.Id;
        }

        public async Task<bool> UpdateSimulationComponent(int id, UpdateSimulationComponentDto dto, CancellationToken cancellationToken = default)
        {
            if (dto == null) throw new ValidationException("Body is required.");

            var entity = await _uow.SimulationComponentRepository.GetByIdAsync(id);
            if (entity == null || entity.IsDeleted == true) return false;

            if (!string.IsNullOrEmpty(dto.Name))
            {
                var rawName = dto.Name.Trim();
                if (string.IsNullOrWhiteSpace(rawName))
                    throw new ValidationException("Name cannot be empty.");
                if (rawName.Length > 100)
                    throw new ValidationException("Name must be at most 100 characters.");

                var normalizedName = string.Join(' ', rawName.Split(' ', StringSplitOptions.RemoveEmptyEntries));

                // Check duplicate name within the same brand model (excluding current entity)
                var nameExists = await _uow.SimulationComponentRepository.ExistsAsync(sc => 
                    sc.Id != id && 
                    sc.BrandModelId == entity.BrandModelId && 
                    sc.Name.ToLower() == normalizedName.ToLower() && 
                    sc.IsDeleted != true);
                if (nameExists)
                    throw new ValidationException("A simulation component with the same name already exists for this brand model.");

                entity.Name = normalizedName;
            }

            if (dto.Description != null)
            {
                if (dto.Description.Length > 1000)
                    throw new ValidationException("Description must be at most 1000 characters.");
                entity.Description = dto.Description;
            }

            if (dto.ImageUrl != null)
            {
                if (dto.ImageUrl.Length > 1000)
                    throw new ValidationException("ImageUrl must be at most 1000 characters.");
                entity.ImageUrl = dto.ImageUrl;
            }

            if (dto.IsActive.HasValue)
            {
                entity.IsActive = dto.IsActive.Value;
            }

            await _uow.SimulationComponentRepository.UpdateAsync(entity);
            await _uow.SaveChangesAsync();

            return true;
        }

        public async Task<bool> DeleteSimulationComponent(int id, CancellationToken cancellationToken = default)
        {
            var entity = await _uow.SimulationComponentRepository.GetByIdAsync(id);
            if (entity == null || entity.IsDeleted == true) return false;

            // Soft delete
            entity.IsDeleted = true;
            entity.IsActive = false;

            await _uow.SimulationComponentRepository.UpdateAsync(entity);
            await _uow.SaveChangesAsync();

            return true;
        }
    }
}
