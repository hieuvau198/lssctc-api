using InternalService.Domain.Contexts;
using InternalService.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace InternalService.Domain.Implements
{
    public class GenericRepository<T> : IGenericRepository<T> where T : class
    {
        protected readonly InternalDbContext _context;
        protected readonly DbSet<T> _dbSet;

        public GenericRepository(InternalDbContext context)
        {
            _context = context;
            _dbSet = context.Set<T>();
        }
              
        public IQueryable<T> GetAllAsQueryable()
        {
            return _dbSet.AsNoTracking();
        }

        public async Task<T?> GetByIdAsync(object id)
        {
            return await _dbSet.FindAsync(id);
        }

        public async Task<T> CreateAsync(T entity)
        {
            await _dbSet.AddAsync(entity);
            return entity;
        }

        public Task UpdateAsync(T entity)
        {
            _dbSet.Update(entity);
            return Task.CompletedTask;
        }

        public Task DeleteAsync(T entity)
        {
            _dbSet.Remove(entity);
            return Task.CompletedTask;
        }

        public async Task<bool> ExistsAsync(System.Linq.Expressions.Expression<Func<T, bool>> predicate)
        {
            return await _dbSet.AsNoTracking().AnyAsync(predicate);
        }

        public async Task<int> CountAsync(System.Linq.Expressions.Expression<Func<T, bool>> predicate)
        {
            return await _dbSet.AsNoTracking().CountAsync(predicate);
        }
    }
}
