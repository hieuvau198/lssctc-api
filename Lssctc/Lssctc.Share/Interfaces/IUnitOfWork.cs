using Lssctc.Share.Entities;

namespace Lssctc.Share.Interfaces
{
    public interface IUnitOfWork : IDisposable
    {
        IGenericRepository<Admin> AdminRepository { get; }
        IGenericRepository<Certificate> CertificateRepository { get; }
        IGenericRepository<Class> ClassRepository { get; }
        IGenericRepository<ClassCode> ClassCodeRepository { get; }
        IGenericRepository<ClassEnrollment> ClassEnrollmentRepository { get; }
        IGenericRepository<ClassInstructor> ClassInstructorRepository { get; }
        IGenericRepository<ClassMember> ClassMemberRepository { get; }
        IGenericRepository<Course> CourseRepository { get; }
        IGenericRepository<CourseCategory> CourseCategoryRepository { get; }
        IGenericRepository<CourseCertificate> CourseCertificateRepository { get; }
        IGenericRepository<CourseCode> CourseCodeRepository { get; }
        IGenericRepository<CourseLevel> CourseLevelRepository { get; }
        IGenericRepository<CourseSyllabuse> CourseSyllabuseRepository { get; }
        IGenericRepository<Instructor> InstructorRepository { get; }
        IGenericRepository<InstructorProfile> InstructorProfileRepository { get; }
        IGenericRepository<LearningMaterial> LearningMaterialRepository { get; }
        IGenericRepository<LearningMaterialType> LearningMaterialTypeRepository { get; }
        IGenericRepository<LearningRecord> LearningRecordRepository { get; }
        IGenericRepository<LearningRecordPartition> LearningRecordPartitionRepository { get; }
        IGenericRepository<Payment> PaymentRepository { get; }
        IGenericRepository<PaymentTransaction> PaymentTransactionRepository { get; }
        IGenericRepository<Practice> PracticeRepository { get; }
        IGenericRepository<PracticeStep> PracticeStepRepository { get; }
        IGenericRepository<PracticeStepComponent> PracticeStepComponentRepository { get; }
        IGenericRepository<PracticeStepType> PracticeStepTypeRepository { get; }
        IGenericRepository<PracticeStepWarning> PracticeStepWarningRepository { get; }
        IGenericRepository<PracticeStepWarningType> PracticeStepWarningTypeRepository { get; }
        IGenericRepository<ProgramCourse> ProgramCourseRepository { get; }
        IGenericRepository<ProgramEntryRequirement> ProgramEntryRequirementRepository { get; }
        IGenericRepository<ProgramManager> ProgramManagerRepository { get; }
        IGenericRepository<Quiz> QuizRepository { get; }
        IGenericRepository<QuizQuestion> QuizQuestionRepository { get; }
        IGenericRepository<QuizQuestionOption> QuizQuestionOptionRepository { get; }
        IGenericRepository<Section> SectionRepository { get; }
        IGenericRepository<SectionMaterial> SectionMaterialRepository { get; }
        IGenericRepository<SectionPartition> SectionPartitionRepository { get; }
        IGenericRepository<SectionPartitionType> SectionPartitionTypeRepository { get; }
        IGenericRepository<SectionPractice> SectionPracticeRepository { get; }
        IGenericRepository<SectionPracticeAttempt> SectionPracticeAttemptRepository { get; }
        IGenericRepository<SectionPracticeAttemptStep> SectionPracticeAttemptStepRepository { get; }
        IGenericRepository<SectionPracticeTimeslot> SectionPracticeTimeslotRepository { get; }
        IGenericRepository<SectionQuiz> SectionQuizRepository { get; }
        IGenericRepository<SectionQuizAttempt> SectionQuizAttemptRepository { get; }
        IGenericRepository<SectionQuizAttemptAnswer> SectionQuizAttemptAnswerRepository { get; }
        IGenericRepository<SectionQuizAttemptQuestion> SectionQuizAttemptQuestionRepository { get; }
        IGenericRepository<SimulationComponent> SimulationComponentRepository { get; }
        IGenericRepository<SimulationComponentType> SimulationComponentTypeRepository { get; }
        IGenericRepository<SimulationManager> SimulationManagerRepository { get; }
        IGenericRepository<SimulationSetting> SimulationSettingRepository { get; }
        IGenericRepository<SimulationTimeslot> SimulationTimeslotRepository { get; }
        IGenericRepository<SyllabusSection> SyllabusSectionRepository { get; }
        IGenericRepository<Syllabuse> SyllabuseRepository { get; }
        IGenericRepository<Trainee> TraineeRepository { get; }
        IGenericRepository<TraineeCertificate> TraineeCertificateRepository { get; }
        IGenericRepository<TraineeProfile> TraineeProfileRepository { get; }
        IGenericRepository<TrainingProgram> ProgramRepository { get; }
        IGenericRepository<TrainingProgress> TrainingProgressRepository { get; }
        IGenericRepository<TrainingResult> TrainingResultRepository { get; }
        IGenericRepository<TrainingResultType> TrainingResultTypeRepository { get; }
        IGenericRepository<Transaction> TransactionRepository { get; }
        IGenericRepository<TransactionProgram> TransactionProgramRepository { get; }
        IGenericRepository<User> UserRepository { get; }

        Task<int> SaveChangesAsync();
    }
}
