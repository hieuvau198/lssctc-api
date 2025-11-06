using Lssctc.ProgramManagement.BrandModel.DTOs;
using Lssctc.Share.Common;
using Lssctc.Share.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Lssctc.ProgramManagement.BrandModel.Services
{
    public class BrandModel : IBrandModel
    {
        private readonly IUnitOfWork _uow;

        public BrandModel(IUnitOfWork uow)
        {
            _uow = uow;
        }

        public async Task<PagedResult<SimulationComponentDto>> GetSimulationComponentsByBrandModelIdAsync(
            int brandModelId,
            int page,
            int pageSize,
            CancellationToken cancellationToken = default)
        {
            // Validate page and pageSize
            if (page < 1) page = 1;
            if (pageSize < 1) pageSize = 10;

            // Query simulation components by BrandModelId
            var query = _uow.SimulationComponentRepository.GetAllAsQueryable()
                .Where(sc => sc.BrandModelId == brandModelId && sc.IsDeleted != true);

            // Get total count
            var totalCount = await query.CountAsync(cancellationToken);

            // Get paginated items
            var items = await query
                .OrderBy(sc => sc.Id)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(sc => new SimulationComponentDto
                {
                    Id = sc.Id,
                    BrandModelId = sc.BrandModelId,
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
    }
}
