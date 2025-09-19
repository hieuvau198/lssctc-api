using Lssctc.Share.Contexts;
using Lssctc.Share.Entities;
using Lssctc.Share.Interfaces;

namespace Lssctc.Share.Implements
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly LssctcDbContext _context;

        private IGenericRepository<Admin>? _adminRepository;
        private IGenericRepository<Certificate>? _certificateRepository;
        private IGenericRepository<Class>? _classRepository;
        private IGenericRepository<ClassCode>? _classCodeRepository;
        private IGenericRepository<ClassEnrollment>? _classEnrollmentRepository;
        private IGenericRepository<ClassInstructor>? _classInstructorRepository;
        private IGenericRepository<ClassMember>? _classMemberRepository;
        private IGenericRepository<Course>? _courseRepository;

        private IGenericRepository<CourseCategory>? _courseCategoryRepository;
        private IGenericRepository<CourseCertificate>? _courseCertificateRepository;
        private IGenericRepository<CourseCode>? _courseCodeRepository;
        private IGenericRepository<CourseLevel>? _courseLevelRepository;
        private IGenericRepository<CourseSyllabuse>? _courseSyllabuseRepository;
        private IGenericRepository<Instructor>? _instructorRepository;
        private IGenericRepository<InstructorProfile>? _instructorProfileRepository;
        private IGenericRepository<LearningMaterial>? _learningMaterialRepository;
        private IGenericRepository<LearningMaterialType>? _learningMaterialTypeRepository;
        private IGenericRepository<LearningRecord>? _learningRecordRepository;
        private IGenericRepository<LearningRecordPartition>? _learningRecordPartitionRepository;
        private IGenericRepository<Payment>? _paymentRepository;
        private IGenericRepository<PaymentTransaction>? _paymentTransactionRepository;
        private IGenericRepository<Practice>? _practiceRepository;
        private IGenericRepository<PracticeStep>? _practiceStepRepository;
        private IGenericRepository<PracticeStepComponent>? _practiceStepComponentRepository;
        private IGenericRepository<PracticeStepType>? _practiceStepTypeRepository;
        private IGenericRepository<PracticeStepWarning>? _practiceStepWarningRepository;
        private IGenericRepository<PracticeStepWarningType>? _practiceStepWarningTypeRepository;
        private IGenericRepository<ProgramCourse>? _programCourseRepository;
        private IGenericRepository<ProgramEntryRequirement>? _programEntryRequirementRepository;
        private IGenericRepository<ProgramManager>? _programManagerRepository;
        private IGenericRepository<Quiz>? _quizRepository;
        private IGenericRepository<QuizQuestion>? _quizQuestionRepository;
        private IGenericRepository<QuizQuestionOption>? _quizQuestionOptionRepository;
        private IGenericRepository<Section>? _sectionRepository;
        private IGenericRepository<SectionMaterial>? _sectionMaterialRepository;
        private IGenericRepository<SectionPartition>? _sectionPartitionRepository;
        private IGenericRepository<SectionPartitionType>? _sectionPartitionTypeRepository;
        private IGenericRepository<SectionPractice>? _sectionPracticeRepository;
        private IGenericRepository<SectionPracticeAttempt>? _sectionPracticeAttemptRepository;
        private IGenericRepository<SectionPracticeAttemptStep>? _sectionPracticeAttemptStepRepository;
        private IGenericRepository<SectionPracticeTimeslot>? _sectionPracticeTimeslotRepository;
        private IGenericRepository<SectionQuiz>? _sectionQuizRepository;
        private IGenericRepository<SectionQuizAttempt>? _sectionQuizAttemptRepository;
        private IGenericRepository<SectionQuizAttemptAnswer>? _sectionQuizAttemptAnswerRepository;
        private IGenericRepository<SectionQuizAttemptQuestion>? _sectionQuizAttemptQuestionRepository;
        private IGenericRepository<SimulationComponent>? _simulationComponentRepository;
        private IGenericRepository<SimulationComponentType>? _simulationComponentTypeRepository;
        private IGenericRepository<SimulationManager>? _simulationManagerRepository;
        private IGenericRepository<SimulationSetting>? _simulationSettingRepository;
        private IGenericRepository<SimulationTimeslot>? _simulationTimeslotRepository;
        private IGenericRepository<SyllabusSection>? _syllabusSectionRepository;
        private IGenericRepository<Syllabuse>? _syllabuseRepository;
        private IGenericRepository<Trainee>? _traineeRepository;
        private IGenericRepository<TraineeCertificate>? _traineeCertificateRepository;
        private IGenericRepository<TraineeProfile>? _traineeProfileRepository;
        private IGenericRepository<TrainingProgram>? _programRepository;
        private IGenericRepository<TrainingProgress>? _trainingProgressRepository;
        private IGenericRepository<TrainingResult>? _trainingResultRepository;
        private IGenericRepository<TrainingResultType>? _trainingResultTypeRepository;
        private IGenericRepository<Transaction>? _transactionRepository;
        private IGenericRepository<TransactionProgram>? _transactionProgramRepository;
        private IGenericRepository<User>? _userRepository;


        public UnitOfWork(LssctcDbContext context)
        {
            _context = context;
        }

        public IGenericRepository<Admin> AdminRepository =>
            _adminRepository ??= new GenericRepository<Admin>(_context);

        public IGenericRepository<Certificate> CertificateRepository =>
            _certificateRepository ??= new GenericRepository<Certificate>(_context);

        public IGenericRepository<Class> ClassRepository =>
            _classRepository ??= new GenericRepository<Class>(_context);

        public IGenericRepository<ClassCode> ClassCodeRepository =>
            _classCodeRepository ??= new GenericRepository<ClassCode>(_context);

        public IGenericRepository<ClassEnrollment> ClassEnrollmentRepository =>
            _classEnrollmentRepository ??= new GenericRepository<ClassEnrollment>(_context);

        public IGenericRepository<ClassInstructor> ClassInstructorRepository =>
            _classInstructorRepository ??= new GenericRepository<ClassInstructor>(_context);

        public IGenericRepository<ClassMember> ClassMemberRepository =>
            _classMemberRepository ??= new GenericRepository<ClassMember>(_context);

        public IGenericRepository<Course> CourseRepository =>
            _courseRepository ??= new GenericRepository<Course>(_context);


        public IGenericRepository<CourseCategory> CourseCategoryRepository =>
            _courseCategoryRepository ??= new GenericRepository<CourseCategory>(_context);

        public IGenericRepository<CourseCertificate> CourseCertificateRepository =>
            _courseCertificateRepository ??= new GenericRepository<CourseCertificate>(_context);

        public IGenericRepository<CourseCode> CourseCodeRepository =>
            _courseCodeRepository ??= new GenericRepository<CourseCode>(_context);

        public IGenericRepository<CourseLevel> CourseLevelRepository =>
            _courseLevelRepository ??= new GenericRepository<CourseLevel>(_context);


        public IGenericRepository<CourseSyllabuse> CourseSyllabuseRepository =>
            _courseSyllabuseRepository ??= new GenericRepository<CourseSyllabuse>(_context);

        public IGenericRepository<Instructor> InstructorRepository =>
            _instructorRepository ??= new GenericRepository<Instructor>(_context);

        public IGenericRepository<InstructorProfile> InstructorProfileRepository =>
            _instructorProfileRepository ??= new GenericRepository<InstructorProfile>(_context);

        public IGenericRepository<LearningMaterial> LearningMaterialRepository =>
            _learningMaterialRepository ??= new GenericRepository<LearningMaterial>(_context);

        public IGenericRepository<LearningMaterialType> LearningMaterialTypeRepository =>
            _learningMaterialTypeRepository ??= new GenericRepository<LearningMaterialType>(_context);

        public IGenericRepository<LearningRecord> LearningRecordRepository =>
            _learningRecordRepository ??= new GenericRepository<LearningRecord>(_context);

        public IGenericRepository<LearningRecordPartition> LearningRecordPartitionRepository =>
            _learningRecordPartitionRepository ??= new GenericRepository<LearningRecordPartition>(_context);

        public IGenericRepository<Payment> PaymentRepository =>
            _paymentRepository ??= new GenericRepository<Payment>(_context);

        public IGenericRepository<PaymentTransaction> PaymentTransactionRepository =>
            _paymentTransactionRepository ??= new GenericRepository<PaymentTransaction>(_context);

        public IGenericRepository<Practice> PracticeRepository =>
            _practiceRepository ??= new GenericRepository<Practice>(_context);

        public IGenericRepository<PracticeStep> PracticeStepRepository =>
            _practiceStepRepository ??= new GenericRepository<PracticeStep>(_context);

        public IGenericRepository<PracticeStepComponent> PracticeStepComponentRepository =>
            _practiceStepComponentRepository ??= new GenericRepository<PracticeStepComponent>(_context);

        public IGenericRepository<PracticeStepType> PracticeStepTypeRepository =>
            _practiceStepTypeRepository ??= new GenericRepository<PracticeStepType>(_context);

        public IGenericRepository<PracticeStepWarning> PracticeStepWarningRepository =>
            _practiceStepWarningRepository ??= new GenericRepository<PracticeStepWarning>(_context);

        public IGenericRepository<PracticeStepWarningType> PracticeStepWarningTypeRepository =>
            _practiceStepWarningTypeRepository ??= new GenericRepository<PracticeStepWarningType>(_context);

        public IGenericRepository<ProgramCourse> ProgramCourseRepository =>
            _programCourseRepository ??= new GenericRepository<ProgramCourse>(_context);

        public IGenericRepository<ProgramEntryRequirement> ProgramEntryRequirementRepository =>
            _programEntryRequirementRepository ??= new GenericRepository<ProgramEntryRequirement>(_context);

        public IGenericRepository<ProgramManager> ProgramManagerRepository =>
            _programManagerRepository ??= new GenericRepository<ProgramManager>(_context);

        public IGenericRepository<Quiz> QuizRepository =>
            _quizRepository ??= new GenericRepository<Quiz>(_context);

        public IGenericRepository<QuizQuestion> QuizQuestionRepository =>
            _quizQuestionRepository ??= new GenericRepository<QuizQuestion>(_context);

        public IGenericRepository<QuizQuestionOption> QuizQuestionOptionRepository =>
            _quizQuestionOptionRepository ??= new GenericRepository<QuizQuestionOption>(_context);

        public IGenericRepository<Section> SectionRepository =>
            _sectionRepository ??= new GenericRepository<Section>(_context);

        public IGenericRepository<SectionMaterial> SectionMaterialRepository =>
            _sectionMaterialRepository ??= new GenericRepository<SectionMaterial>(_context);

        public IGenericRepository<SectionPartition> SectionPartitionRepository =>
            _sectionPartitionRepository ??= new GenericRepository<SectionPartition>(_context);

        public IGenericRepository<SectionPartitionType> SectionPartitionTypeRepository =>
            _sectionPartitionTypeRepository ??= new GenericRepository<SectionPartitionType>(_context);

        public IGenericRepository<SectionPractice> SectionPracticeRepository =>
            _sectionPracticeRepository ??= new GenericRepository<SectionPractice>(_context);

        public IGenericRepository<SectionPracticeAttempt> SectionPracticeAttemptRepository =>
            _sectionPracticeAttemptRepository ??= new GenericRepository<SectionPracticeAttempt>(_context);

        public IGenericRepository<SectionPracticeAttemptStep> SectionPracticeAttemptStepRepository =>
            _sectionPracticeAttemptStepRepository ??= new GenericRepository<SectionPracticeAttemptStep>(_context);

        public IGenericRepository<SectionPracticeTimeslot> SectionPracticeTimeslotRepository =>
            _sectionPracticeTimeslotRepository ??= new GenericRepository<SectionPracticeTimeslot>(_context);

        public IGenericRepository<SectionQuiz> SectionQuizRepository =>
            _sectionQuizRepository ??= new GenericRepository<SectionQuiz>(_context);

        public IGenericRepository<SectionQuizAttempt> SectionQuizAttemptRepository =>
            _sectionQuizAttemptRepository ??= new GenericRepository<SectionQuizAttempt>(_context);

        public IGenericRepository<SectionQuizAttemptAnswer> SectionQuizAttemptAnswerRepository =>
            _sectionQuizAttemptAnswerRepository ??= new GenericRepository<SectionQuizAttemptAnswer>(_context);

        public IGenericRepository<SectionQuizAttemptQuestion> SectionQuizAttemptQuestionRepository =>
            _sectionQuizAttemptQuestionRepository ??= new GenericRepository<SectionQuizAttemptQuestion>(_context);

        public IGenericRepository<SimulationComponent> SimulationComponentRepository =>
            _simulationComponentRepository ??= new GenericRepository<SimulationComponent>(_context);

        public IGenericRepository<SimulationComponentType> SimulationComponentTypeRepository =>
            _simulationComponentTypeRepository ??= new GenericRepository<SimulationComponentType>(_context);

        public IGenericRepository<SimulationManager> SimulationManagerRepository =>
            _simulationManagerRepository ??= new GenericRepository<SimulationManager>(_context);

        public IGenericRepository<SimulationSetting> SimulationSettingRepository =>
            _simulationSettingRepository ??= new GenericRepository<SimulationSetting>(_context);

        public IGenericRepository<SimulationTimeslot> SimulationTimeslotRepository =>
            _simulationTimeslotRepository ??= new GenericRepository<SimulationTimeslot>(_context);

        public IGenericRepository<SyllabusSection> SyllabusSectionRepository =>
            _syllabusSectionRepository ??= new GenericRepository<SyllabusSection>(_context);

        public IGenericRepository<Syllabuse> SyllabuseRepository =>
            _syllabuseRepository ??= new GenericRepository<Syllabuse>(_context);

        public IGenericRepository<Trainee> TraineeRepository =>
            _traineeRepository ??= new GenericRepository<Trainee>(_context);

        public IGenericRepository<TraineeCertificate> TraineeCertificateRepository =>
            _traineeCertificateRepository ??= new GenericRepository<TraineeCertificate>(_context);

        public IGenericRepository<TraineeProfile> TraineeProfileRepository =>
            _traineeProfileRepository ??= new GenericRepository<TraineeProfile>(_context);

        public IGenericRepository<TrainingProgram> ProgramRepository =>
            _programRepository ??= new GenericRepository<TrainingProgram>(_context);

        public IGenericRepository<TrainingProgress> TrainingProgressRepository =>
            _trainingProgressRepository ??= new GenericRepository<TrainingProgress>(_context);

        public IGenericRepository<TrainingResult> TrainingResultRepository =>
            _trainingResultRepository ??= new GenericRepository<TrainingResult>(_context);

        public IGenericRepository<TrainingResultType> TrainingResultTypeRepository =>
            _trainingResultTypeRepository ??= new GenericRepository<TrainingResultType>(_context);

        public IGenericRepository<Transaction> TransactionRepository =>
            _transactionRepository ??= new GenericRepository<Transaction>(_context);

        public IGenericRepository<TransactionProgram> TransactionProgramRepository =>
            _transactionProgramRepository ??= new GenericRepository<TransactionProgram>(_context);

        public IGenericRepository<User> UserRepository =>
            _userRepository ??= new GenericRepository<User>(_context);


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
