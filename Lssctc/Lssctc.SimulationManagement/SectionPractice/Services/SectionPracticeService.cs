using AutoMapper;
using AutoMapper.QueryableExtensions;
using Lssctc.Share.Common;
using Lssctc.Share.Interfaces;
using Lssctc.SimulationManagement.SectionPractice.Dtos;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using Entities = Lssctc.Share.Entities;

namespace Lssctc.SimulationManagement.SectionPractice.Services
{
    public class SectionPracticeService : ISectionPracticeService
    {
        private readonly IUnitOfWork _uow;
        private readonly IMapper _mapper;

        public SectionPracticeService(IUnitOfWork uow, IMapper mapper)
        {
            _uow = uow;
            _mapper = mapper;
        }

        public async Task<int> CreateSectionPractice(CreateSectionPracticeDto dto)
        {
            if (dto == null || dto.SectionPartitionId <= 0 || dto.PracticeId <= 0)
                throw new ValidationException("Invalid input.");

            if (!await _uow.SectionPartitionRepository.ExistsAsync(s => s.Id == dto.SectionPartitionId))
                throw new KeyNotFoundException($"SectionPartition with id={dto.SectionPartitionId} not found.");

            if (!await _uow.PracticeRepository.ExistsAsync(p => p.Id == dto.PracticeId))
                throw new KeyNotFoundException($"Practice with id={dto.PracticeId} not found.");

            var entity = _mapper.Map<Entities.SectionPractice>(dto);
            entity.IsDeleted = false;
            entity.Status = 1;
            await _uow.SectionPracticeRepository.CreateAsync(entity);
            await _uow.SaveChangesAsync();
            return entity.Id;
        }

        public async Task<bool> DeleteSectionPractice(int id)
        {
            var entity = await _uow.SectionPracticeRepository.GetByIdAsync(id);
            if (entity == null) return false;

            var hasTimeslot = await _uow.SectionPracticeTimeslotRepository
                .ExistsAsync(t => t.SectionPracticeId == id);

            var hasAttempt = await _uow.SectionPracticeAttemptRepository
                .ExistsAsync(a => a.SectionPracticeId == id);

            if (hasTimeslot || hasAttempt)
                throw new InvalidOperationException("Cannot delete: section practice is in use (timeslots/attempts exist).");

            await _uow.SectionPracticeRepository.DeleteAsync(entity);
            await _uow.SaveChangesAsync();
            return true;
        }

        // get section practices by classId with pagination
        public async Task<PagedResult<SectionPracticeListDto>> GetSectionPracticesByClassId(int classId, int page, int pageSize)
        {
            if (classId <= 0) throw new ValidationException("ClassId is invalid.");
            if (page < 1) page = 1;
            if (pageSize < 1 || pageSize > 200) pageSize = 20;

            var exists = await _uow.ClassRepository.ExistsAsync(c => c.Id == classId);
            if (!exists) throw new KeyNotFoundException($"Class {classId} not found.");

            var baseQuery =
                from sp in _uow.SectionPracticeRepository.GetAllAsQueryable()
                join part in _uow.SectionPartitionRepository.GetAllAsQueryable() on sp.SectionPartitionId equals part.Id
                join sec in _uow.SectionRepository.GetAllAsQueryable() on part.SectionId equals sec.Id
                join p in _uow.PracticeRepository.GetAllAsQueryable() on sp.PracticeId equals p.Id
                where sec.ClassesId == classId
                select new { sp, part, sec, p };
            // sp = section_practices, part = section_partitions, sec = sections, p = practices

            var total = await baseQuery.CountAsync();

            var items = await baseQuery
                .OrderBy(x => x.sec.Order)
                .ThenBy(x => x.part.DisplayOrder)
                .ThenBy(x => x.sp.Id)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(x => new SectionPracticeListDto
                {
                    Id = x.sp.Id,
                    SectionId = x.sec.Id,
                    SectionName = x.sec.Name,
                    SectionOrder = x.sec.Order,
                    SectionPartitionId = x.part.Id,
                    PartitionName = x.part.Name,
                    PartitionTypeId = x.part.PartitionTypeId,
                    PracticeId = x.p.Id,
                    // CHÚ Ý: tên property entity. Nếu entity là PracticeName thì dùng dòng dưới:
                    PracticeName = x.p.PracticeName,
                    

                    CustomDeadline = x.sp.CustomDeadline,
                    CustomDescription = x.sp.CustomDescription,
                    Status = x.sp.Status ?? 1,
                    IsActive = x.sp.IsActive ?? true,
                    IsDeleted = x.sp.IsDeleted ?? false
                })
                .AsNoTracking()
                .ToListAsync();

            return new PagedResult<SectionPracticeListDto>
            {
                Items = items,
                TotalCount = total,
                Page = page,
                PageSize = pageSize
            };
        }


        public async Task<SectionPracticeDto?> GetSectionPracticeById(int id)
        {
            var dto = await _uow.SectionPracticeRepository.GetAllAsQueryable()
                .Where(x => x.Id == id)
                .ProjectTo<SectionPracticeDto>(_mapper.ConfigurationProvider)
                .AsNoTracking()
                .FirstOrDefaultAsync();
            return dto;
        }

        public async Task<PagedResult<SectionPracticeDto>> GetSectionPracticesPaged(int pageIndex, int pageSize)
        {
            if (pageIndex < 1) pageIndex = 1;
            if (pageSize < 1 || pageSize > 200) pageSize = 20;

            var q = _uow.SectionPracticeRepository.GetAllAsQueryable();

            var total = await q.CountAsync();

            var items = await q
                .Skip((pageIndex - 1) * pageSize)
                .Take(pageSize)
                .ProjectTo<SectionPracticeDto>(_mapper.ConfigurationProvider)
                .AsNoTracking()
                .ToListAsync();

            return new PagedResult<SectionPracticeDto>
            {
                Items = items,
                TotalCount = total,
                Page = pageIndex,
                PageSize = pageSize
            };
        }

        public async Task<bool> UpdateSectionPractice(int id, UpdateSectionPracticeDto dto)
        {
            var entity = await _uow.SectionPracticeRepository.GetByIdAsync(id);
            if (entity == null) return false;

            if (dto.SectionPartitionId.HasValue)
            {
                if (dto.SectionPartitionId.Value <= 0) throw new ValidationException("SectionPartitionId is invalid.");
                var ok = await _uow.SectionPartitionRepository.ExistsAsync(s => s.Id == dto.SectionPartitionId.Value);
                if (!ok) throw new KeyNotFoundException($"SectionPartition {dto.SectionPartitionId.Value} not found.");
            }

            if (dto.PracticeId.HasValue)
            {
                if (dto.PracticeId.Value <= 0) throw new ValidationException("PracticeId is invalid.");
                var ok = await _uow.PracticeRepository.ExistsAsync(p => p.Id == dto.PracticeId.Value);
                if (!ok) throw new KeyNotFoundException($"Practice {dto.PracticeId.Value} not found.");
            }

            _mapper.Map(dto, entity);
            await _uow.SectionPracticeRepository.UpdateAsync(entity);
            await _uow.SaveChangesAsync();
            return true;
        }
    }
}
