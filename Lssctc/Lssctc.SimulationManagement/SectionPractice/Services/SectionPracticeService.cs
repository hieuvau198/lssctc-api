using AutoMapper;
using AutoMapper.QueryableExtensions;
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

        public async Task<int> CreateAsync(CreateSectionPracticeDto dto)
        {
            if (dto == null || dto.SectionPartitionId <= 0 || dto.PracticeId <= 0)
                throw new ValidationException("Invalid input.");

            if (!await _uow.SectionPartitionRepository.ExistsAsync(s => s.Id == dto.SectionPartitionId))
                throw new KeyNotFoundException($"SectionPartition with id={dto.SectionPartitionId} not found.");

            if (!await _uow.PracticeRepository.ExistsAsync(p => p.Id == dto.PracticeId))
                throw new KeyNotFoundException($"Practice with id={dto.PracticeId} not found.");

            if (dto.SimulationTimeslotId.HasValue)
            {
                var exists = await _uow.SimulationTimeslotRepository.ExistsAsync(t => t.Id == dto.SimulationTimeslotId.Value);
                if (!exists)
                    throw new KeyNotFoundException($"SimulationTimeslot with id={dto.SimulationTimeslotId.Value} not found.");
            }

            var entity = _mapper.Map<Entities.SectionPractice>(dto);
            await _uow.SectionPracticeRepository.CreateAsync(entity);
            await _uow.SaveChangesAsync();
            return entity.Id;
        }



        public async Task<bool> DeleteAsync(int id)
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


        public async Task<SectionPracticeDto?> GetByIdAsync(int id)
        {
            var dto = await _uow.SectionPracticeRepository.GetAllAsQueryable()
                .Where(x => x.Id == id)
                .ProjectTo<SectionPracticeDto>(_mapper.ConfigurationProvider)
                .AsNoTracking()
                .FirstOrDefaultAsync();
            return dto;
        }

        public async Task<(IReadOnlyList<SectionPracticeDto> Items, int Total)> GetPagedAsync(
             int pageIndex, int pageSize, int? sectionPartitionId, int? practiceId, int? status, string? search)
        {
            if (pageIndex < 1) pageIndex = 1;
            if (pageSize < 1 || pageSize > 200) pageSize = 20;

            var q = _uow.SectionPracticeRepository.GetAllAsQueryable();

            if (sectionPartitionId.HasValue)
                q = q.Where(x => x.SectionPartitionId == sectionPartitionId.Value);

            if (practiceId.HasValue)
                q = q.Where(x => x.PracticeId == practiceId.Value);

            if (status.HasValue)
                q = q.Where(x => x.Status == status.Value);

            if (!string.IsNullOrWhiteSpace(search))
            {
                var s = string.Join(" ", search.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries));
                // search vào custom_description
                q = q.Where(x => x.CustomDescription != null && x.CustomDescription.Contains(s));
            }

            var total = await q.CountAsync();

            var items = await q
                .OrderByDescending(x => x.Id)
                .Skip((pageIndex - 1) * pageSize)
                .Take(pageSize)
                .ProjectTo<SectionPracticeDto>(_mapper.ConfigurationProvider)
                .AsNoTracking()
                .ToListAsync();

            return (items, total);
        }

        public async Task<bool> UpdateAsync(int id, UpdateSectionPracticeDto dto)
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

            if (dto.SimulationTimeslotId.HasValue)
            {
                var ok = await _uow.SimulationTimeslotRepository.ExistsAsync(t => t.Id == dto.SimulationTimeslotId.Value);
                if (!ok) throw new KeyNotFoundException($"SimulationTimeslot {dto.SimulationTimeslotId.Value} not found.");
            }

            _mapper.Map(dto, entity);
            await _uow.SectionPracticeRepository.UpdateAsync(entity);
            await _uow.SaveChangesAsync();
            return true;
        }
    }
}
