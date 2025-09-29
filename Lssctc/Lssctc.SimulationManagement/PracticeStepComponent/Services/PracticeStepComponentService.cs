using AutoMapper;
using AutoMapper.QueryableExtensions;
using Lssctc.Share.Common;
using Lssctc.Share.Interfaces;
using Lssctc.SimulationManagement.PracticeStepComponent.Dtos;
using Microsoft.EntityFrameworkCore;

namespace Lssctc.SimulationManagement.PracticeStepComponent.Services
{
    public class PracticeStepComponentService : IPracticeStepComponentService
    {
        private readonly IUnitOfWork _uow;
        private readonly IMapper _mapper;

        public PracticeStepComponentService(IUnitOfWork uow, IMapper mapper)
        {
            _uow = uow;
            _mapper = mapper;
        }

        public Task<int> CreateAsync(CreatePracticeStepComponentDto dto)
        {
            throw new NotImplementedException();
        }

        public Task<bool> DeleteAsync(int id)
        {
            throw new NotImplementedException();
        }

        public Task<PracticeStepComponentDto?> GetByIdAsync(int id)
        {
            throw new NotImplementedException();
        }

        public async Task<PagedResult<PracticeStepComponentDto>> GetPagedAsync(
            int pageIndex, int pageSize, int? practiceStepId, int? simulationComponentId, string? search)
        {
            if (pageIndex < 1) pageIndex = 1;
            if (pageSize < 1 || pageSize > 200) pageSize = 20;

            var q = _uow.PracticeStepComponentRepository.GetAllAsQueryable();

            if (practiceStepId.HasValue)
                q = q.Where(x => x.StepId == practiceStepId.Value);

            if (simulationComponentId.HasValue)
                q = q.Where(x => x.ComponentId == simulationComponentId.Value);

            // Không có Name/Description -> bỏ search, hoặc có thể map search vào ComponentId/StepId nếu muốn.

            var total = await q.CountAsync();

            var items = await q
                .OrderBy(x => x.ComponentOrder)
                .ThenBy(x => x.Id)
                .Skip((pageIndex - 1) * pageSize)
                .Take(pageSize)
                .ProjectTo<PracticeStepComponentDto>(_mapper.ConfigurationProvider)
                .AsNoTracking()
                .ToListAsync();

            return new PagedResult<PracticeStepComponentDto>
            {
                Page = pageIndex,
                PageSize = pageSize,
                TotalCount = total,
                Items = items
            };
        }


        public Task<bool> UpdateAsync(int id, UpdatePracticeStepComponentDto dto)
        {
            throw new NotImplementedException();
        }
    }
}
