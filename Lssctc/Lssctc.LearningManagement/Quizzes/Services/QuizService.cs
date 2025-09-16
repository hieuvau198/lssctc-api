using AutoMapper;
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

        public Task<(IReadOnlyList<QuizDto> Items, int Total)> GetPagedAsync(int pageIndex, int pageSize, string? search)
        {
            throw new NotImplementedException();
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
