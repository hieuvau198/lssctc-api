using Lssctc.Share.Entities;

namespace Lssctc.Share.Interfaces
{
    public interface IUnitOfWork : IDisposable
    {
        IGenericRepository<TrainingProgram> ProgramRepository { get; }
        IGenericRepository<Course> CourseRepository { get; }
        IGenericRepository<LearningMaterial> LearningMaterialRepository { get; }

        IGenericRepository<Quiz> QuizRepository { get; }

        IGenericRepository<QuizQuestion> QuizQuestionRepository { get; }

        IGenericRepository<QuizQuestionOption> QuizQuestionOptionRepository { get; }

        Task<int> SaveChangesAsync();
    }
}
