using AutoMapper;
using AutoMapper.QueryableExtensions;
using Lssctc.ProgramManagement.Classes.DTOs;
using Lssctc.Share.Common;
using Lssctc.Share.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using Entities = Lssctc.Share.Entities; 
namespace Lssctc.ProgramManagement.TrainingResult.Services
{
    public class TrainingResultService : ITrainingResultService
    {
        private readonly IUnitOfWork _uow;
        private readonly IMapper _mapper;

        public TrainingResultService(IUnitOfWork uow, IMapper mapper)
        {
            _uow = uow;
            _mapper = mapper;
        }

        public async Task<PagedResult<TrainingResultDto>> GetTrainingResults(int pageIndex, int pageSize)
        {
            if (pageIndex < 1) pageIndex = 1;
            if (pageSize < 1 || pageSize > 200) pageSize = 20;

            var q = _uow.TrainingResultRepository.GetAllAsQueryable();

            var total = await q.CountAsync();

            var items = await q
                .OrderByDescending(x => x.ResultDate)
                .Skip((pageIndex - 1) * pageSize)
                .Take(pageSize)
                .ProjectTo<TrainingResultDto>(_mapper.ConfigurationProvider)
                .AsNoTracking()
                .ToListAsync();

            return new PagedResult<TrainingResultDto>
            {
                Items = items,
                TotalCount = total,
                Page = pageIndex,
                PageSize = pageSize
            };
        }

        public async Task<IReadOnlyList<TrainingResultDto>> GetTrainingResultsNoPagination()
        {
            return await _uow.TrainingResultRepository.GetAllAsQueryable()
                .OrderByDescending(x => x.ResultDate)
                .ProjectTo<TrainingResultDto>(_mapper.ConfigurationProvider)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<TrainingResultDto?> GetTrainingResultById(int id)
        {
            return await _uow.TrainingResultRepository.GetAllAsQueryable()
                .Where(x => x.Id == id)
                .ProjectTo<TrainingResultDto>(_mapper.ConfigurationProvider)
                .FirstOrDefaultAsync();
        }

        public async Task<int> CreateTrainingResult(CreateTrainingResultDto dto)
        {
            if (dto is null) throw new ValidationException("Body is required.");

            // Validate IDs
            if (dto.TrainingProgressId <= 0)
                throw new ValidationException("TrainingProgressId is invalid.");
            if (dto.TrainingResultTypeId <= 0)
                throw new ValidationException("TrainingResultTypeId is invalid.");

            // Validate FK existence
            var progressExists = await _uow.TrainingProgressRepository
                .ExistsAsync(p => p.Id == dto.TrainingProgressId);
            if (!progressExists)
                throw new KeyNotFoundException($"TrainingProgress {dto.TrainingProgressId} not found.");

            var typeExists = await _uow.TrainingResultTypeRepository
                .ExistsAsync(t => t.Id == dto.TrainingResultTypeId);
            if (!typeExists)
                throw new KeyNotFoundException($"TrainingResultType {dto.TrainingResultTypeId} not found.");

            // Normalize + length checks
            dto.ResultValue = dto.ResultValue?.Trim();
            dto.Notes = dto.Notes?.Trim();

            if (dto.ResultValue != null && dto.ResultValue.Length > 2000)
                throw new ValidationException("ResultValue must be at most 2000 characters.");
            if (dto.Notes != null && dto.Notes.Length > 2000)
                throw new ValidationException("Notes must be at most 2000 characters.");

            // Date check
            if (dto.ResultDate == default)
                throw new ValidationException("ResultDate is required.");  
            var minDate = new DateTime(2000, 1, 1);
            var maxDate = DateTime.UtcNow.AddYears(1);
            if (dto.ResultDate < minDate || dto.ResultDate > maxDate)
                throw new ValidationException($"ResultDate must be between {minDate:yyyy-MM-dd} and {maxDate:yyyy-MM-dd} (UTC).");

            // Map & save
            var entity = _mapper.Map<Entities.TrainingResult>(dto);
            await _uow.TrainingResultRepository.CreateAsync(entity);
            await _uow.SaveChangesAsync();
            return entity.Id;
        }

        public async Task<bool> UpdateTrainingResult(int id, UpdateTrainingResultDto dto)
        {
            if (id <= 0) throw new ValidationException("Id is invalid.");
            if (dto is null) throw new ValidationException("Body is required.");

            // Nếu body có Id thì bắt khớp với route (tuỳ chọn)
            if (dto.Id != 0 && dto.Id != id)
                throw new ValidationException("Body.Id must match route id.");

            var entity = await _uow.TrainingResultRepository.GetByIdAsync(id);
            if (entity == null) return false;

            // Chỉ cho phép sửa 2 trường này
            var value = dto.ResultValue?.Trim();
            var notes = dto.Notes?.Trim();

            if (value is { Length: > 2000 })
                throw new ValidationException("ResultValue must be at most 2000 characters.");
            if (notes is { Length: > 2000 })
                throw new ValidationException("Notes must be at most 2000 characters.");

            entity.ResultValue = value;
            entity.Notes = notes;

           

            await _uow.TrainingResultRepository.UpdateAsync(entity);
            await _uow.SaveChangesAsync();
            return true;
        }


        public async Task<bool> DeleteTrainingResultById(int id)
        {
            var entity = await _uow.TrainingResultRepository.GetByIdAsync(id);
            if (entity == null) return false;

            await _uow.TrainingResultRepository.DeleteAsync(entity);
            await _uow.SaveChangesAsync();
            return true;
        }
    }
}
