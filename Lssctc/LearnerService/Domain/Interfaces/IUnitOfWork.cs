using LearnerService.Domain.Entities;

namespace LearnerService.Domain.Interfaces
{
    public interface IUnitOfWork : IDisposable
    {
        IGenericRepository<Learner> LearnerRepository { get; }
        IGenericRepository<Course> CourseRepository { get; }
        Task<int> SaveChangesAsync();
    }
}
