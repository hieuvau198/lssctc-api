using InternalService.Domain.Entities;

namespace InternalService.Domain.Interfaces
{
    public interface IUnitOfWork : IDisposable
    {
        IGenericRepository<Learner> LearnerRepository { get; }
        IGenericRepository<Course> CourseRepository { get; }
        IGenericRepository<QuestionOption> QuestionOptionRepository { get; }
        Task<int> SaveChangesAsync();
    }
}
