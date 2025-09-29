using AutoMapper;
using AutoMapper.QueryableExtensions;
using Lssctc.LearningManagement.Section.DTOs;
using Lssctc.Share.Common;
using Lssctc.Share.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace Lssctc.LearningManagement.Section.Services
{
    public class SectionService : ISectionService
    {
        private const int MIN_DURATION_MINUTES = 1;
        private const int MAX_DURATION_MINUTES = 720;
        private readonly IUnitOfWork _uow;
        private readonly IMapper _mapper;

        public SectionService(IUnitOfWork uow, IMapper mapper)
        {
            _uow = uow;
            _mapper = mapper;
        }

        public async Task<PagedResult<SectionListItemDto>> GetSections(
     int pageIndex, int pageSize,
     int? classesId = null,
     int? syllabusSectionId = null,
     int? status = null,
     string? search = null)
        {
            if (pageIndex < 1) pageIndex = 1;
            if (pageSize <= 0 || pageSize > 200) pageSize = 20;

            var q = _uow.SectionRepository.GetAllAsQueryable();

            if (classesId.HasValue) q = q.Where(s => s.ClassesId == classesId.Value);
            if (syllabusSectionId.HasValue) q = q.Where(s => s.SyllabusSectionId == syllabusSectionId.Value);
            if (status.HasValue) q = q.Where(s => s.Status == status.Value);

            if (!string.IsNullOrWhiteSpace(search))
            {
                var s = string.Join(" ", search.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries));
                q = q.Where(x =>
                    (x.Name != null && x.Name.Contains(s)) ||
                    (x.Description != null && x.Description.Contains(s)));
            }

            var total = await q.CountAsync();

            var items = await q
                .OrderBy(s => s.ClassesId)
                .ThenBy(s => s.Order)
                .ProjectTo<SectionListItemDto>(_mapper.ConfigurationProvider)
                .Skip((pageIndex - 1) * pageSize)
                .Take(pageSize)
                .AsNoTracking()
                .ToListAsync();

            return new PagedResult<SectionListItemDto>
            {
                Items = items,
                TotalCount = total,
                Page = pageIndex,
                PageSize = pageSize
                // TotalPages tự tính
            };
        }

        public async Task<SectionDto?> GetById(int id)
        {
            var dto = await _uow.SectionRepository.GetAllAsQueryable()
                .Where(s => s.Id == id)
                .ProjectTo<SectionDto>(_mapper.ConfigurationProvider)
                .AsNoTracking()
                .FirstOrDefaultAsync();
            return dto;
        }

        public async Task<int> Create(CreateSectionDto dto)
        {
            if (dto == null) throw new ValidationException("Body is required.");

            var rawName = (dto.Name ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(rawName))
                throw new ValidationException("Name is required.");

            // đảm bảo FK tồn tại
            var classExists = await _uow.ClassRepository.ExistsAsync(c => c.Id == dto.ClassesId);
            if (!classExists) throw new ValidationException($"Class {dto.ClassesId} not found.");

            var syllabusExists = await _uow.SyllabusSectionRepository.ExistsAsync(x => x.Id == dto.SyllabusSectionId);
            if (!syllabusExists) throw new ValidationException($"SyllabusSection {dto.SyllabusSectionId} not found.");

            // Name có thể unique trong cùng 1 class + syllabusSection (tùy bạn): ví dụ
            var nameExists = await _uow.SectionRepository.ExistsAsync(s =>
                s.ClassesId == dto.ClassesId &&
                s.SyllabusSectionId == dto.SyllabusSectionId &&
                s.Name.ToLower() == rawName.ToLower());
            if (nameExists)
                throw new ValidationException("Section name already exists in this class & syllabus section.");

            // Order dương
            if (dto.Order <= 0) throw new ValidationException("Order must be >= 1.");
            // DurationMinutes validate
            if (!dto.DurationMinutes.HasValue)
                throw new ValidationException("DurationMinutes is required.");
            ValidateDurationMinutes(dto.DurationMinutes.Value);


            // validate StartDate, EndDate
            var start = dto.StartDate ?? DateTime.UtcNow;
            ValidateStartEnd(start, dto.EndDate);

            // Chuẩn hoá name
            var normalizedName = string.Join(" ", rawName.Split(' ', StringSplitOptions.RemoveEmptyEntries));

            var entity = _mapper.Map<Lssctc.Share.Entities.Section>(dto);
            entity.Name = normalizedName;

            await _uow.SectionRepository.CreateAsync(entity);
            await _uow.SaveChangesAsync();
            return entity.Id;
        }

        public async Task<bool> Update(int id, UpdateSectionDto dto)
        {
            var entity = await _uow.SectionRepository.GetByIdAsync(id);
            if (entity == null) return false;

            // Validate nếu có thay đổi các field
            if (dto.Name != null)
            {
                var n = dto.Name.Trim();
                if (string.IsNullOrWhiteSpace(n))
                    throw new ValidationException("Name cannot be empty.");
                dto.Name = string.Join(" ", n.Split(' ', StringSplitOptions.RemoveEmptyEntries));

                // unique trong (ClassesId/SyllabusSectionId) nếu các FK không đổi hoặc cập nhật
                var targetClassId = dto.ClassesId ?? entity.ClassesId;
                var targetSylId = dto.SyllabusSectionId ?? entity.SyllabusSectionId;
                var exists = await _uow.SectionRepository.ExistsAsync(s =>
                    s.Id != id &&
                    s.ClassesId == targetClassId &&
                    s.SyllabusSectionId == targetSylId &&
                    s.Name.ToLower() == dto.Name.ToLower());
                if (exists)
                    throw new ValidationException("Section name already exists in this class & syllabus section.");
            }

            if (dto.ClassesId.HasValue)
            {
                var ok = await _uow.ClassRepository.ExistsAsync(c => c.Id == dto.ClassesId.Value);
                if (!ok) throw new ValidationException($"Class {dto.ClassesId} not found.");
            }

            if (dto.SyllabusSectionId.HasValue)
            {
                var ok = await _uow.SyllabusSectionRepository.ExistsAsync(x => x.Id == dto.SyllabusSectionId.Value);
                if (!ok) throw new ValidationException($"SyllabusSection {dto.SyllabusSectionId} not found.");
            }

            if (dto.Order.HasValue && dto.Order <= 0)
                throw new ValidationException("Order must be >= 1.");

            // DurationMinutes validate
            if (!dto.DurationMinutes.HasValue)
                throw new ValidationException("DurationMinutes is required.");
            ValidateDurationMinutes(dto.DurationMinutes.Value);

            //validate StartDate, EndDate
            var targetStart = dto.StartDate ?? entity.StartDate;
            var targetEnd = dto.EndDate ?? entity.EndDate;
            ValidateStartEnd(targetStart, targetEnd);

            _mapper.Map(dto, entity);
            await _uow.SectionRepository.UpdateAsync(entity);
            await _uow.SaveChangesAsync();
            return true;
        }

        public async Task<bool> Delete(int id)
        {
            var entity = await _uow.SectionRepository.GetByIdAsync(id);
            if (entity == null) return false;

            // (tuỳ bạn) kiểm tra ràng buộc: SectionPartition, LearningRecord… nếu có liên kết thì chặn
            var hasPartitions = await _uow.SectionPartitionRepository
                .GetAllAsQueryable().AnyAsync(p => p.SectionId == id);
            if (hasPartitions)
                throw new ValidationException("Cannot delete section that has partitions.");

            await _uow.SectionRepository.DeleteAsync(entity);
            await _uow.SaveChangesAsync();
            return true;
        }


        private static void ValidateStartEnd(DateTime start, DateTime? end)
        {
            if (end.HasValue && end.Value <= start)
                throw new ValidationException("EndDate must be after StartDate.");
        }

        private static void ValidateDurationMinutes(int duration)
        {
            if (duration < MIN_DURATION_MINUTES || duration > MAX_DURATION_MINUTES)
                throw new ValidationException($"DurationMinutes must be between {MIN_DURATION_MINUTES} and {MAX_DURATION_MINUTES}.");
        }

    }
}
