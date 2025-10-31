using AutoMapper;
using AutoMapper.QueryableExtensions;
using Lssctc.LearningManagement.SectionQuizzes.DTOs;
using Lssctc.Share.Common;
using Lssctc.Share.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using Entities = Lssctc.Share.Entities;
namespace Lssctc.LearningManagement.SectionQuizzes.Services
{
    public class SectionQuizService : ISectionQuizService
    {
        private readonly IUnitOfWork _uow;
        private readonly IMapper _mapper;

        public SectionQuizService(IUnitOfWork uow, IMapper mapper)
        {
            _uow = uow;
            _mapper = mapper;
        }

        public async Task<PagedResult<SectionQuizDto>> GetSectionQuizzesPagination(int pageIndex, int pageSize)
        {
            if (pageIndex < 1) pageIndex = 1;
            if (pageSize < 1 || pageSize > 200) pageSize = 20;

            var q = _uow.SectionQuizRepository.GetAllAsQueryable();
            var total = await q.CountAsync();

            var items = await q
                .OrderByDescending(x => x.Id)
                .Skip((pageIndex - 1) * pageSize)
                .Take(pageSize)
                .ProjectTo<SectionQuizDto>(_mapper.ConfigurationProvider)
                .AsNoTracking()
                .ToListAsync();

            return new PagedResult<SectionQuizDto>
            {
                Items = items,
                TotalCount = total,
                Page = pageIndex,
                PageSize = pageSize
            };
        }

        public async Task<IReadOnlyList<SectionQuizDto>> GetSectionQuizzesNoPagination()
        {
            return await _uow.SectionQuizRepository.GetAllAsQueryable()
                .OrderByDescending(x => x.Id)
                .ProjectTo<SectionQuizDto>(_mapper.ConfigurationProvider)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<SectionQuizDto?> GetSectionQuizById(int id)
        {
            return await _uow.SectionQuizRepository.GetAllAsQueryable()
                .Where(x => x.Id == id)
                .ProjectTo<SectionQuizDto>(_mapper.ConfigurationProvider)
                .AsNoTracking()
                .FirstOrDefaultAsync();
        }

        public async Task<int> CreateSectionQuiz(CreateSectionQuizDto dto)
        {
            if (dto == null) throw new ValidationException("Body is required.");
            if (dto.QuizId <= 0) throw new ValidationException("QuizId is invalid.");
            if (dto.SectionPartitionId <= 0) throw new ValidationException("SectionPartitionId is invalid.");

            // FK checks
            var quizExists = await _uow.QuizRepository.ExistsAsync(q => q.Id == dto.QuizId);
            if (!quizExists) throw new KeyNotFoundException($"Quiz {dto.QuizId} not found.");

            var spExists = await _uow.SectionPartitionRepository.ExistsAsync(s => s.Id == dto.SectionPartitionId);
            if (!spExists) throw new KeyNotFoundException($"SectionPartition {dto.SectionPartitionId} not found.");

            // Name
            var rawName = (dto.Name ?? string.Empty).Trim();
            var normalizedName = string.Join(" ", rawName.Split(' ', StringSplitOptions.RemoveEmptyEntries));
            if (string.IsNullOrWhiteSpace(normalizedName))
                throw new ValidationException("Name is required.");
            if (normalizedName.Length > 200)
                throw new ValidationException("Name must be at most 200 characters.");

            // unique (name) within a SectionPartition
            var dup = await _uow.SectionQuizRepository.ExistsAsync(x =>
                x.SectionPartitionId == dto.SectionPartitionId &&
                x.Name.ToLower() == normalizedName.ToLower());
            if (dup) throw new InvalidOperationException("Name already exists in this SectionPartition.");

            // Description limit
            if (dto.Description != null && dto.Description.Length > 2000)
                throw new ValidationException("Description must be at most 2000 characters.");

            var entity = _mapper.Map<Entities.SectionQuiz>(dto);
            entity.Name = normalizedName;

            await _uow.SectionQuizRepository.CreateAsync(entity);
            await _uow.SaveChangesAsync();
            return entity.Id;
        }

        public async Task<bool> UpdateSectionQuiz(int id, UpdateSectionQuizDto dto)
        {
            if (dto == null) throw new ValidationException("Body is required.");

            var entity = await _uow.SectionQuizRepository.GetByIdAsync(id);
            if (entity == null) return false;

            // If changing QuizId
            if (dto.QuizId.HasValue)
            {
                if (dto.QuizId.Value <= 0) throw new ValidationException("QuizId is invalid.");
                var ok = await _uow.QuizRepository.ExistsAsync(q => q.Id == dto.QuizId.Value);
                if (!ok) throw new KeyNotFoundException($"Quiz {dto.QuizId.Value} not found.");
            }

            // If changing SectionPartitionId
            if (dto.SectionPartitionId.HasValue)
            {
                if (dto.SectionPartitionId.Value <= 0) throw new ValidationException("SectionPartitionId is invalid.");
                var ok = await _uow.SectionPartitionRepository.ExistsAsync(s => s.Id == dto.SectionPartitionId.Value);
                if (!ok) throw new KeyNotFoundException($"SectionPartition {dto.SectionPartitionId.Value} not found.");
            }

            var newSectionPartitionId = dto.SectionPartitionId ?? entity.SectionPartitionId;

            // If changing Name
            if (dto.Name != null)
            {
                var raw = dto.Name.Trim();
                var normalizedName = string.Join(" ", raw.Split(' ', StringSplitOptions.RemoveEmptyEntries));
                if (string.IsNullOrWhiteSpace(normalizedName))
                    throw new ValidationException("Name is required.");
                if (normalizedName.Length > 200)
                    throw new ValidationException("Name must be at most 200 characters.");

                var dup = await _uow.SectionQuizRepository.ExistsAsync(x =>
                    x.Id != id &&
                    x.SectionPartitionId == newSectionPartitionId &&
                    x.Name != null &&
                    x.Name.ToLower() == normalizedName.ToLower());
                if (dup) throw new InvalidOperationException("Name already exists in this SectionPartition.");

                dto.Name = normalizedName;
            }

            // Description limit
            if (dto.Description != null && dto.Description.Length > 2000)
                throw new ValidationException("Description must be at most 2000 characters.");

            _mapper.Map(dto, entity);
            await _uow.SectionQuizRepository.UpdateAsync(entity);
            await _uow.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteSectionQuiz(int id)
        {
            var entity = await _uow.SectionQuizRepository.GetByIdAsync(id);
            if (entity == null) return false;

            // In-use check: có attempt thì chặn xoá
            var inUse = await _uow.SectionQuizAttemptRepository.ExistsAsync(a => a.SectionQuizId == id);
            if (inUse)
                throw new InvalidOperationException("Cannot delete: section quiz has attempts.");

            await _uow.SectionQuizRepository.DeleteAsync(entity);
            await _uow.SaveChangesAsync();
            return true;
        }

    }
}
