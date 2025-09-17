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
        IGenericRepository<ProgramPrerequisite> ProgramPrerequisiteRepository { get; }
        IGenericRepository<ProgramCourse> ProgramCourseRepository { get; }
        IGenericRepository<Class> ClassRepository { get; }
        IGenericRepository<ClassCode> ClassCodeRepository { get; }
        IGenericRepository<Instructor> InstructorRepository { get; }
        IGenericRepository<Trainee> TraineeRepository { get; }
        IGenericRepository<ClassInstructor> ClassInstructorRepository { get; }
        IGenericRepository<ClassMember> ClassMemberRepository { get; }

        Task<int> SaveChangesAsync();
    }
}
