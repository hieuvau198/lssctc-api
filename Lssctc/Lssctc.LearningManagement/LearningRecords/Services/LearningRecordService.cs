using AutoMapper;
using AutoMapper.QueryableExtensions;
using Lssctc.LearningManagement.LearningRecords.DTOs;
using Lssctc.Share.Common;
using Lssctc.Share.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using Entities = Lssctc.Share.Entities;
namespace Lssctc.LearningManagement.LearningRecords.Services
{
    public class LearningRecordService : ILearningRecordService
    {
        private readonly IUnitOfWork _uow;
        private readonly IMapper _mapper;

        public LearningRecordService(IUnitOfWork uow, IMapper mapper)
        {
            _uow = uow;
            _mapper = mapper;
        }
        public async Task<PagedResult<LearningRecordDto>> GetLearningRecords(int pageIndex, int pageSize)
        {
            if (pageIndex < 1) pageIndex = 1;
            if (pageSize < 1 || pageSize > 200) pageSize = 20;

            var query = _uow.LearningRecordRepository.GetAllAsQueryable();

            var total = await query.CountAsync();

            var items = await query
                .OrderByDescending(x => x.Id)
                .Skip((pageIndex - 1) * pageSize)
                .Take(pageSize)
                .ProjectTo<LearningRecordDto>(_mapper.ConfigurationProvider)
                .AsNoTracking()
                .ToListAsync();

            return new PagedResult<LearningRecordDto>
            {
                Items = items,
                TotalCount = total,
                Page = pageIndex,
                PageSize = pageSize
            };
        }

        public async Task<IReadOnlyList<LearningRecordDto>> GetLearningRecordsNoPagination()
        {
            return await _uow.LearningRecordRepository.GetAllAsQueryable()
                .OrderByDescending(x => x.Id)
                .ProjectTo<LearningRecordDto>(_mapper.ConfigurationProvider)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<bool> DeleteLearningRecord(int id)
        {
            var entity = await _uow.LearningRecordRepository.GetByIdAsync(id);
            if (entity == null) return false;

            var inUse = await _uow.LearningRecordPartitionRepository.ExistsAsync(x => x.LearningRecordId == id);
            if (inUse) throw new InvalidOperationException("Cannot delete: LearningRecord is in use (partitions exist).");

            await _uow.LearningRecordRepository.DeleteAsync(entity);
            await _uow.SaveChangesAsync();
            return true;
        }

        public async Task<LearningRecordDto?> GetLearningRecordById(int id)
        {
            return await _uow.LearningRecordRepository.GetAllAsQueryable()
                .Where(x => x.Id == id)
                .ProjectTo<LearningRecordDto>(_mapper.ConfigurationProvider)
                .AsNoTracking()
                .FirstOrDefaultAsync();
        }


        public async Task<int> CreateLearningRecord(CreateLearningRecordDto dto)
        {
            if (dto == null)
                throw new ValidationException("Body is required.");

            if (dto.SectionId <= 0)
                throw new ValidationException("SectionId is invalid.");

            if (dto.TrainingProgressId <= 0)
                throw new ValidationException("TrainingProgressId is invalid.");

            // Validate FK tồn tại
            if (!await _uow.SectionRepository.ExistsAsync(s => s.Id == dto.SectionId))
                throw new KeyNotFoundException($"Section {dto.SectionId} not found.");

            if (!await _uow.TrainingProgressRepository.ExistsAsync(p => p.Id == dto.TrainingProgressId))
                throw new KeyNotFoundException($"TrainingProgress {dto.TrainingProgressId} not found.");

            // Validate Name (nếu có)
            if (!string.IsNullOrWhiteSpace(dto.Name) && dto.Name.Length > 200)
                throw new ValidationException("Name must be at most 200 characters.");

            // Validate SectionName (nếu có)
            if (!string.IsNullOrWhiteSpace(dto.SectionName) && dto.SectionName.Length > 200)
                throw new ValidationException("SectionName must be at most 200 characters.");

            // Validate Progress
            if (dto.Progress.HasValue)
            {
                if (dto.Progress.Value < 0 || dto.Progress.Value > 100)
                    throw new ValidationException("Progress must be between 0 and 100.");
                dto.Progress = Math.Round(dto.Progress.Value, 2, MidpointRounding.AwayFromZero);
            }

            var entity = _mapper.Map<Entities.LearningRecord>(dto);

            await _uow.LearningRecordRepository.CreateAsync(entity);
            await _uow.SaveChangesAsync();
            return entity.Id;
        }


        public async Task<bool> UpdateLearningRecord(int id, UpdateLearningRecordDto dto)
        {
            if (dto == null)
                throw new ValidationException("Body is required.");

            var entity = await _uow.LearningRecordRepository.GetByIdAsync(id);
            if (entity == null) return false;

            // Validate SectionId
            if (dto.SectionId <= 0)
                throw new ValidationException("SectionId is invalid.");
            if (!await _uow.SectionRepository.ExistsAsync(s => s.Id == dto.SectionId))
                throw new KeyNotFoundException($"Section {dto.SectionId} not found.");

            // Validate TrainingProgressId
            if (dto.TrainingProgressId <= 0)
                throw new ValidationException("TrainingProgressId is invalid.");
            if (!await _uow.TrainingProgressRepository.ExistsAsync(p => p.Id == dto.TrainingProgressId))
                throw new KeyNotFoundException($"TrainingProgress {dto.TrainingProgressId} not found.");

            // Validate Name
            if (!string.IsNullOrWhiteSpace(dto.Name) && dto.Name.Length > 200)
                throw new ValidationException("Name must be at most 200 characters.");

            // Validate SectionName
            if (!string.IsNullOrWhiteSpace(dto.SectionName) && dto.SectionName.Length > 200)
                throw new ValidationException("SectionName must be at most 200 characters.");

            // Validate Progress
            if (dto.Progress.HasValue)
            {
                if (dto.Progress.Value < 0 || dto.Progress.Value > 100)
                    throw new ValidationException("Progress must be between 0 and 100.");
                dto.Progress = Math.Round(dto.Progress.Value, 2, MidpointRounding.AwayFromZero);
            }

            // Map toàn bộ
            _mapper.Map(dto, entity);

            await _uow.LearningRecordRepository.UpdateAsync(entity);
            await _uow.SaveChangesAsync();
            return true;
        }


    }
}
