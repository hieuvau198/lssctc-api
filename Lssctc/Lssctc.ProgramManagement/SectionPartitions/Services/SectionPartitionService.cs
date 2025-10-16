using Lssctc.ProgramManagement.SectionPartitions.DTOs;
using Lssctc.Share.Common;
using Lssctc.Share.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using Entities = Lssctc.Share.Entities;

namespace Lssctc.ProgramManagement.SectionPartitions.Services
{
    public class SectionPartitionService : ISectionPartitionService
    {
        private readonly IUnitOfWork _uow;

        public SectionPartitionService(IUnitOfWork uow)
        {
            _uow = uow;
        }

        public async Task<PagedResult<SectionPartitionDto>> GetSectionPartitionsPaged(int pageIndex, int pageSize)
        {
            if (pageIndex < 1) pageIndex = 1;
            if (pageSize < 1 || pageSize > 200) pageSize = 20;

            var q = _uow.SectionPartitionRepository.GetAllAsQueryable();

            var total = await q.CountAsync();

            var items = await q
                .Skip((pageIndex - 1) * pageSize)
                .Take(pageSize)
                .Select(x => new SectionPartitionDto
                {
                    Id = x.Id,
                    SectionId = x.SectionId,
                    Name = x.Name,
                    PartitionTypeId = x.PartitionTypeId,
                    DisplayOrder = x.DisplayOrder,
                    Description = x.Description
                })
                .AsNoTracking()
                .ToListAsync();

            return new PagedResult<SectionPartitionDto>
            {
                Items = items,
                TotalCount = total,
                Page = pageIndex,
                PageSize = pageSize
            };
        }

        public async Task<IReadOnlyList<SectionPartitionDto>> GetSectionPartitionsNoPagination()
        {
            return await _uow.SectionPartitionRepository.GetAllAsQueryable()
                .OrderByDescending(x => x.Id) // giữ đồng nhất style
                .Select(x => new SectionPartitionDto
                {
                    Id = x.Id,
                    SectionId = x.SectionId,
                    Name = x.Name,
                    PartitionTypeId = x.PartitionTypeId,
                    DisplayOrder = x.DisplayOrder,
                    Description = x.Description
                })
                .AsNoTracking()
                .ToListAsync();
        }

        //get by sectionId with pagination
        public async Task<PagedResult<SectionPartitionDto>> GetSectionPartitionBySectionId(int sectionId, int page, int pageSize)
        {
            if (sectionId <= 0)
                throw new ValidationException("sectionId is invalid.");
            if (page <= 0) page = 1;
            if (pageSize <= 0 || pageSize > 200) pageSize = 20;

            var sectionExists = await _uow.SectionRepository.ExistsAsync(x => x.Id == sectionId);
            if (!sectionExists)
                throw new KeyNotFoundException($"Section {sectionId} not found.");

            var baseQuery = _uow.SectionPartitionRepository
                .GetAllAsQueryable()
                .Where(x => x.SectionId == sectionId);

            var total = await baseQuery.CountAsync();

            var items = await baseQuery
                .OrderBy(x => x.DisplayOrder)
                .ThenBy(x => x.Id)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(x => new SectionPartitionDto
                {
                    Id = x.Id,
                    SectionId = x.SectionId,
                    Name = x.Name,
                    PartitionTypeId = x.PartitionTypeId,
                    DisplayOrder = x.DisplayOrder,
                    Description = x.Description
                })
                .AsNoTracking()
                .ToListAsync();

            return new PagedResult<SectionPartitionDto>
            {
                Items = items,
                TotalCount = total,
                Page = page,
                PageSize = pageSize
            };
        }

        public async Task<SectionPartitionDto?> GetSectionPartitionById(int id)
        {
            var entity = await _uow.SectionPartitionRepository.GetAllAsQueryable()
                .Where(x => x.Id == id)
                .FirstOrDefaultAsync();

            if (entity == null) 
                return null;

            return new SectionPartitionDto
            {
                Id = entity.Id,
                SectionId = entity.SectionId,
                Name = entity.Name,
                PartitionTypeId = entity.PartitionTypeId,
                DisplayOrder = entity.DisplayOrder,
                Description = entity.Description
            };
        }

        public async Task<int> CreateSectionPartition(CreateSectionPartitionDto dto)
        {
            if (dto == null) throw new ValidationException("Body is required.");
            if (dto.SectionId <= 0) throw new ValidationException("SectionId is invalid.");
            if (dto.PartitionTypeId <= 0) throw new ValidationException("PartitionTypeId is invalid.");

            var section = await _uow.SectionRepository.GetByIdAsync(dto.SectionId);
            if (section == null)
                throw new KeyNotFoundException($"Section {dto.SectionId} not found.");

            // Kiểm tra Section đã bắt đầu chưa
            await ValidateSectionCanBeModified(section);

            if (!await _uow.SectionPartitionTypeRepository.ExistsAsync(t => t.Id == dto.PartitionTypeId))
                throw new KeyNotFoundException($"PartitionType {dto.PartitionTypeId} not found.");

            // unique name trong 1 section
            var name = (dto.Name ?? "").Trim();
            if (!string.IsNullOrEmpty(name))
            {
                var normalized = string.Join(" ", name.Split(' ', StringSplitOptions.RemoveEmptyEntries));
                var dup = await _uow.SectionPartitionRepository.ExistsAsync(x =>
                    x.SectionId == dto.SectionId &&
                    x.Name != null && x.Name.ToLower() == normalized.ToLower());

                if (dup) throw new InvalidOperationException("Name already exists in this Section.");
            }

            var entity = new Entities.SectionPartition
            {
                SectionId = dto.SectionId,
                Name = dto.Name,
                PartitionTypeId = dto.PartitionTypeId,
                Description = dto.Description
            };

            await _uow.SectionPartitionRepository.CreateAsync(entity);
            await _uow.SaveChangesAsync();
            return entity.Id;
        }

        public async Task<bool> UpdateSectionPartition(int id, UpdateSectionPartitionDto dto)
        {
            var entity = await _uow.SectionPartitionRepository.GetByIdAsync(id);
            if (entity == null) return false;

            // Lấy thông tin Section để kiểm tra trạng thái
            var section = await _uow.SectionRepository.GetByIdAsync(entity.SectionId);
            if (section == null)
                throw new KeyNotFoundException($"Section {entity.SectionId} not found.");

            // Kiểm tra Section đã bắt đầu chưa
            await ValidateSectionCanBeModified(section);

            if (dto.PartitionTypeId.HasValue)
            {
                if (dto.PartitionTypeId.Value <= 0)
                    throw new ValidationException("PartitionTypeId is invalid.");

                var ok = await _uow.SectionPartitionTypeRepository
                    .ExistsAsync(t => t.Id == dto.PartitionTypeId.Value);
                if (!ok) throw new KeyNotFoundException($"PartitionType {dto.PartitionTypeId.Value} not found.");
            }

            if (dto.Name != null)
            {
                var name = string.Join(" ", dto.Name.Trim()
                    .Split(' ', StringSplitOptions.RemoveEmptyEntries));

                if (!string.IsNullOrEmpty(name))
                {
                    var dup = await _uow.SectionPartitionRepository.ExistsAsync(x =>
                        x.SectionId == entity.SectionId &&
                        x.Id != entity.Id &&
                        x.Name != null &&
                        x.Name.ToLower() == name.ToLower());

                    if (dup) throw new InvalidOperationException("Name already exists in this Section.");
                }
            }

            // Manual mapping for update
            if (dto.Name != null)
                entity.Name = dto.Name;

            if (dto.PartitionTypeId.HasValue)
                entity.PartitionTypeId = dto.PartitionTypeId.Value;

            if (dto.Description != null)
                entity.Description = dto.Description;

            await _uow.SectionPartitionRepository.UpdateAsync(entity);
            await _uow.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteSectionPartition(int id)
        {
            var entity = await _uow.SectionPartitionRepository.GetByIdAsync(id);
            if (entity == null) return false;

            // Lấy thông tin Section để kiểm tra trạng thái
            var section = await _uow.SectionRepository.GetByIdAsync(entity.SectionId);
            if (section == null)
                throw new KeyNotFoundException($"Section {entity.SectionId} not found.");

            // Kiểm tra Section đã bắt đầu chưa
            await ValidateSectionCanBeModified(section);

            var inUse =
                await _uow.LearningRecordPartitionRepository.ExistsAsync(x => x.SectionPartitionId == id)
             || await _uow.SectionMaterialRepository.ExistsAsync(x => x.SectionPartitionId == id)
             || await _uow.SectionPracticeRepository.ExistsAsync(x => x.SectionPartitionId == id)
             || await _uow.SectionQuizRepository.ExistsAsync(x => x.SectionPartitionId == id);

            if (inUse)
                throw new InvalidOperationException("Cannot delete: partition is in use (materials/practices/quizzes/records exist).");

            await _uow.SectionPartitionRepository.DeleteAsync(entity);
            await _uow.SaveChangesAsync();
            return true;
        }

        /// <summary>
        /// Kiểm tra Section có thể được chỉnh sửa hay không
        /// Section chỉ có thể chỉnh sửa khi chưa bắt đầu (StartDate > DateTime.Now và Status cho phép)
        /// </summary>
      
        private async Task ValidateSectionCanBeModified(Entities.Section section)
        {
            var currentTime = DateTime.UtcNow;
            
            // Kiểm tra ngày bắt đầu
            var hasStarted = section.StartDate <= currentTime;
            
            // Kiểm tra trạng thái ( Status = 1 là active , 0 là chưa active)
          
            var isActiveOrCompleted = section.Status >= 1; 
            
            if (hasStarted && isActiveOrCompleted)
            {
                throw new InvalidOperationException(
                    $"Cannot modify Section Partition because Section '{section.Name}' has already started on {section.StartDate:yyyy-MM-dd HH:mm:ss} UTC.");
            }
            
            await Task.CompletedTask; // For async consistency
        }
    }
}
