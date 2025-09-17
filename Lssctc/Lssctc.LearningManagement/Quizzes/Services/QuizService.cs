using AutoMapper;
using AutoMapper.QueryableExtensions;
using Lssctc.LearningManagement.Quizzes.DTOs;
using Lssctc.Share.Entities;
using Lssctc.Share.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Lssctc.LearningManagement.Quizzes.Services
{
    public class QuizService : IQuizService
    {
        private readonly IUnitOfWork _uow;
        private readonly IMapper _mapper;

        public QuizService(IUnitOfWork uow, IMapper mapper)
        {
            _uow = uow;
            _mapper = mapper;
        }

      

        public async Task<QuizDto?> GetByIdAsync(int id)
        {
            var entity = await _uow.QuizRepository.GetByIdAsync(id); 
            return entity == null ? null : _mapper.Map<QuizDto>(entity);
        }

        public async Task<(IReadOnlyList<QuizDto> Items, int Total)> GetPagedAsync(int pageIndex, int pageSize, string? search)
        {
            if (pageIndex < 1) pageIndex = 1;
            if (pageSize <= 0 || pageSize > 200) pageSize = 20;

            var query = _uow.QuizRepository.GetAllAsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                var s = search.Trim();
                query = query.Where(q =>
                    (q.Name != null && q.Name.Contains(s)) ||
                    (q.Description != null && q.Description.Contains(s)));
            }

            var total = await query.CountAsync();

            var items = await query
                .OrderByDescending(q => q.UpdatedAt)   // ưu tiên bản cập nhật mới
                .ThenByDescending(q => q.Id)           // ổn định thứ tự
                .Skip((pageIndex - 1) * pageSize)
                .Take(pageSize)
                .ProjectTo<QuizDto>(_mapper.ConfigurationProvider)
                .ToListAsync();

            return (items, total);
        }

        public async Task<int> CreateAsync(CreateQuizDto dto)
        {
            var entity = _mapper.Map<Quiz>(dto);
            await _uow.QuizRepository.CreateAsync(entity);           
            await _uow.SaveChangesAsync();
            return entity.Id;
        }

        public async Task<bool> UpdateAsync(int id, UpdateQuizDto dto)
        {
            var entity = await _uow.QuizRepository.GetByIdAsync(id);  
            if (entity == null) return false;

            _mapper.Map(dto, entity);
            await _uow.QuizRepository.UpdateAsync(entity);            
            await _uow.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var entity = await _uow.QuizRepository.GetByIdAsync(id);  
            if (entity == null) return false;

            await _uow.QuizRepository.DeleteAsync(entity);            
            await _uow.SaveChangesAsync();
            return true;
        }
    }
}
