using Lssctc.Share.Entities;

namespace Lssctc.Share.Interfaces
{
    public interface IUnitOfWork : IDisposable
    {
        IGenericRepository<TrainingProgram> ProgramRepository { get; }
        IGenericRepository<Course> CourseRepository { get; }
        IGenericRepository<LearningMaterial> LearningMaterialRepository { get; }
        IGenericRepository<CourseCategory> CourseCategoryRepository { get; }
        IGenericRepository<CourseLevel> CourseLevelRepository { get; }
        IGenericRepository<CourseCode> CourseCodeRepository { get; }
        IGenericRepository<TrainingProgram> TrainingProgramRepository { get; }

        Task<int> SaveChangesAsync();
    }
}
