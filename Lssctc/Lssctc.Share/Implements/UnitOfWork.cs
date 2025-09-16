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
        private IGenericRepository<CourseCategory>? _courseCategoryRepository;
        private IGenericRepository<CourseLevel>? _courseLevelRepository;
        private IGenericRepository<CourseCode>? _courseCodeRepository;
        private IGenericRepository<LearningMaterial>? _learningMaterialRepository;
        private IGenericRepository<TrainingProgram>? _trainingProgramlRepository;
        private IGenericRepository<ProgramPrerequisite>? _programPrerequisiteRepository;
        private IGenericRepository<ProgramCourse>? _programCourseRepository;



        public UnitOfWork(LssctcDbContext context)
        {
            _context = context;
        }

        public IGenericRepository<TrainingProgram> ProgramRepository =>
            _programRepository ??= new GenericRepository<TrainingProgram>(_context);

        public IGenericRepository<Course> CourseRepository =>
            _courseRepository ??= new GenericRepository<Course>(_context);
        public IGenericRepository<CourseCategory> CourseCategoryRepository =>
            _courseCategoryRepository ??= new GenericRepository<CourseCategory>(_context);
        public IGenericRepository<CourseCode> CourseCodeRepository =>
            _courseCodeRepository ??= new GenericRepository<CourseCode>(_context);
        public IGenericRepository<CourseLevel> CourseLevelRepository =>
            _courseLevelRepository ??= new GenericRepository<CourseLevel>(_context);

        public IGenericRepository<LearningMaterial> LearningMaterialRepository =>
            _learningMaterialRepository ??= new GenericRepository<LearningMaterial>(_context);
        public IGenericRepository<TrainingProgram> TrainingProgramRepository =>
            _trainingProgramlRepository ??= new GenericRepository<TrainingProgram>(_context);

        public IGenericRepository<ProgramPrerequisite> ProgramPrerequisiteRepository =>
            _programPrerequisiteRepository ??= new GenericRepository<ProgramPrerequisite>(_context);
        public IGenericRepository<ProgramCourse> ProgramCourseRepository =>
            _programCourseRepository ??= new GenericRepository<ProgramCourse>(_context);
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
