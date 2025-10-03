using AutoMapper;
using AutoMapper.QueryableExtensions;
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
        private readonly IMapper _mapper;

        public SectionPartitionService(IUnitOfWork uow, IMapper mapper)
        {
            _uow = uow;
            _mapper = mapper;
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
                .ProjectTo<SectionPartitionDto>(_mapper.ConfigurationProvider)
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
                .ProjectTo<SectionPartitionDto>(_mapper.ConfigurationProvider)
                .AsNoTracking()
                .ToListAsync();
        }


        public async Task<SectionPartitionDto?> GetSectionPartitionById(int id)
        {
            var dto = await _uow.SectionPartitionRepository.GetAllAsQueryable()
                .Where(x => x.Id == id)
                .ProjectTo<SectionPartitionDto>(_mapper.ConfigurationProvider)
                .AsNoTracking()
                .FirstOrDefaultAsync();

            return dto;
        }

        public async Task<int> CreateSectionPartition(CreateSectionPartitionDto dto)
        {
            if (dto == null) throw new ValidationException("Body is required.");
            if (dto.SectionId <= 0) throw new ValidationException("SectionId is invalid.");
            if (dto.PartitionTypeId <= 0) throw new ValidationException("PartitionTypeId is invalid.");

            if (!await _uow.SectionRepository.ExistsAsync(s => s.Id == dto.SectionId))
                throw new KeyNotFoundException($"Section {dto.SectionId} not found.");

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

            var entity = _mapper.Map<Entities.SectionPartition>(dto);
            await _uow.SectionPartitionRepository.CreateAsync(entity);
            await _uow.SaveChangesAsync();
            return entity.Id;
        }

        public async Task<bool> UpdateSectionPartition(int id, UpdateSectionPartitionDto dto)
        {
            var entity = await _uow.SectionPartitionRepository.GetByIdAsync(id);
            if (entity == null) return false;

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

            _mapper.Map(dto, entity);
            await _uow.SectionPartitionRepository.UpdateAsync(entity);
            await _uow.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteSectionPartition(int id)
        {
            var entity = await _uow.SectionPartitionRepository.GetByIdAsync(id);
            if (entity == null) return false;

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
    }
}
