using Lssctc.Share.Contexts;
using Lssctc.Share.Entities;
using Lssctc.Share.Interfaces;

namespace Lssctc.Share.Implements
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly LssctcDbContext _context;
        private IGenericRepository<TrainingProgram>? _programRepository;
        private IGenericRepository<Course>? _courseRepository;
        private IGenericRepository<LearningMaterial>? _learningMaterialRepository;

        public UnitOfWork(LssctcDbContext context)
        {
            _context = context;
        }

        public IGenericRepository<TrainingProgram> ProgramRepository =>
            _programRepository ??= new GenericRepository<TrainingProgram>(_context);

        public IGenericRepository<Course> CourseRepository =>
            _courseRepository ??= new GenericRepository<Course>(_context);

        public IGenericRepository<LearningMaterial> LearningMaterialRepository =>
            _learningMaterialRepository ??= new GenericRepository<LearningMaterial>(_context);

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
