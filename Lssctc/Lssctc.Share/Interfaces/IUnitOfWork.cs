using Lssctc.Share.Entities;

namespace Lssctc.Share.Interfaces
{
    public interface IUnitOfWork : IDisposable
    {
        IGenericRepository<TrainingProgram> ProgramRepository { get; }
        IGenericRepository<Course> CourseRepository { get; }
        IGenericRepository<LearningMaterial> LearningMaterialRepository { get; }
        Task<int> SaveChangesAsync();
    }
}
