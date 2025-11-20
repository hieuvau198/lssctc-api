using Lssctc.Share.Entities;

namespace Lssctc.Share.Interfaces
{
    public interface IUnitOfWork : IDisposable
    {
        IGenericRepository<Activity> ActivityRepository { get; }
        IGenericRepository<ActivityMaterial> ActivityMaterialRepository { get; }
        IGenericRepository<ActivityPractice> ActivityPracticeRepository { get; }
        IGenericRepository<ActivityQuiz> ActivityQuizRepository { get; }
        IGenericRepository<ActivityRecord> ActivityRecordRepository { get; }
        IGenericRepository<Admin> AdminRepository { get; }
        IGenericRepository<BrandModel> BrandModelRepository { get; }
        IGenericRepository<Certificate> CertificateRepository { get; }
        IGenericRepository<Class> ClassRepository { get; }
        IGenericRepository<ClassCode> ClassCodeRepository { get; }
        IGenericRepository<ClassInstructor> ClassInstructorRepository { get; }
        IGenericRepository<Course> CourseRepository { get; }
        IGenericRepository<CourseCategory> CourseCategoryRepository { get; }
        IGenericRepository<CourseCertificate> CourseCertificateRepository { get; }
        IGenericRepository<CourseCode> CourseCodeRepository { get; }
        IGenericRepository<CourseLevel> CourseLevelRepository { get; }
        IGenericRepository<CourseSection> CourseSectionRepository { get; }
        IGenericRepository<Enrollment> EnrollmentRepository { get; }
        IGenericRepository<Instructor> InstructorRepository { get; }
        IGenericRepository<InstructorProfile> InstructorProfileRepository { get; }
        IGenericRepository<InstructorFeedback> InstructorFeedbackRepository { get; }
        IGenericRepository<LearningMaterial> LearningMaterialRepository { get; }
        IGenericRepository<LearningProgress> LearningProgressRepository { get; }
        IGenericRepository<MaterialAuthor> MaterialAuthorRepository { get; }
        IGenericRepository<Practice> PracticeRepository { get; }
        IGenericRepository<PracticeAttempt> PracticeAttemptRepository { get; }
        IGenericRepository<PracticeAttemptTask> PracticeAttemptTaskRepository { get; }
        IGenericRepository<PracticeTask> PracticeTaskRepository { get; }
        IGenericRepository<ProgramCourse> ProgramCourseRepository { get; }
        IGenericRepository<Quiz> QuizRepository { get; }
        IGenericRepository<QuizAttempt> QuizAttemptRepository { get; }
        IGenericRepository<QuizAttemptAnswer> QuizAttemptAnswerRepository { get; }
        IGenericRepository<QuizAttemptQuestion> QuizAttemptQuestionRepository { get; }
        IGenericRepository<QuizAuthor> QuizAuthorRepository { get; }
        IGenericRepository<QuizQuestion> QuizQuestionRepository { get; }
        IGenericRepository<QuizQuestionOption> QuizQuestionOptionRepository { get; }
        IGenericRepository<Section> SectionRepository { get; }
        IGenericRepository<SectionActivity> SectionActivityRepository { get; }
        IGenericRepository<SectionRecord> SectionRecordRepository { get; }
        IGenericRepository<SimTask> SimTaskRepository { get; }
        IGenericRepository<SimulationComponent> SimulationComponentRepository { get; }
        IGenericRepository<SimulationManager> SimulationManagerRepository { get; }
        IGenericRepository<Trainee> TraineeRepository { get; }
        IGenericRepository<TraineeCertificate> TraineeCertificateRepository { get; }
        IGenericRepository<TraineeProfile> TraineeProfileRepository { get; }
        IGenericRepository<TrainingProgram> ProgramRepository { get; }
        IGenericRepository<User> UserRepository { get; }

        Task<int> SaveChangesAsync();
    }
}