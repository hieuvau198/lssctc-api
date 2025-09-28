using AutoMapper;
using AutoMapper.QueryableExtensions;
using Lssctc.Share.Interfaces;
using Lssctc.SimulationManagement.SectionPractice.Dtos;
using Microsoft.EntityFrameworkCore;

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

        public Task<int> CreateAsync(CreateSectionPracticeDto dto)
        {
            throw new NotImplementedException();
        }

        public Task<bool> DeleteAsync(int id)
        {
            throw new NotImplementedException();
        }

        public Task<SectionPracticeDto?> GetByIdAsync(int id)
        {
            throw new NotImplementedException();
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

        public Task<bool> UpdateAsync(int id, UpdateSectionPracticeDto dto)
        {
            throw new NotImplementedException();
        }
    }
}
