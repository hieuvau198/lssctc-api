using LearnerService.Domain.Entities;

namespace LearnerService.Domain.Interfaces
{
    public interface IUnitOfWork : IDisposable
    {
        IGenericRepository<Learner> LearnerRepository { get; }
        IGenericRepository<Course> CourseRepository { get; }
        IGenericRepository<QuestionOption> QuestionOptionRepository { get; }
        Task<int> SaveChangesAsync();
    }
}
