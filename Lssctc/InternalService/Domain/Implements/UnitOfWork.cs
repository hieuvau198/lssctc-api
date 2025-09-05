using InternalService.Domain.Contexts;
using InternalService.Domain.Entities;
using InternalService.Domain.Interfaces;

namespace InternalService.Domain.Implements
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly InternalDbContext _context;
        private IGenericRepository<Learner>? _learnerRepository;
        private IGenericRepository<Course>? _courseRepository;
        private IGenericRepository<QuestionOption>? _questionOptionRepository;

        public UnitOfWork(InternalDbContext context)
        {
            _context = context;
        }

        public IGenericRepository<Learner> LearnerRepository =>
            _learnerRepository ??= new GenericRepository<Learner>(_context);

        public IGenericRepository<Course> CourseRepository =>
            _courseRepository ??= new GenericRepository<Course>(_context);

        public IGenericRepository<QuestionOption> QuestionOptionRepository =>
            _questionOptionRepository ??= new GenericRepository<QuestionOption>(_context);

        public async Task<int> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync();
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}
