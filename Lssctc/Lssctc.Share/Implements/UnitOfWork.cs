using Lssctc.Share.Contexts;
using Lssctc.Share.Entities;
using Lssctc.Share.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Lssctc.Share.Implements
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly LssctcDbContext _context;

        private IGenericRepository<Activity>? _activityRepository;
        private IGenericRepository<ActivityMaterial>? _activityMaterialRepository;
        private IGenericRepository<ActivityPractice>? _activityPracticeRepository;
        private IGenericRepository<ActivityQuiz>? _activityQuizRepository;
        private IGenericRepository<ActivityRecord>? _activityRecordRepository;
        private IGenericRepository<Admin>? _adminRepository;
        private IGenericRepository<BrandModel>? _brandModelRepository;
        private IGenericRepository<Certificate>? _certificateRepository;
        private IGenericRepository<Class>? _classRepository;
        private IGenericRepository<ClassCode>? _classCodeRepository;
        private IGenericRepository<ClassInstructor>? _classInstructorRepository;
        private IGenericRepository<Course>? _courseRepository;
        private IGenericRepository<CourseCategory>? _courseCategoryRepository;
        private IGenericRepository<CourseCertificate>? _courseCertificateRepository;
        private IGenericRepository<CourseCode>? _courseCodeRepository;
        private IGenericRepository<CourseLevel>? _courseLevelRepository;
        private IGenericRepository<CourseSection>? _courseSectionRepository;
        private IGenericRepository<Enrollment>? _enrollmentRepository;
        private IGenericRepository<Instructor>? _instructorRepository;
        private IGenericRepository<InstructorProfile>? _instructorProfileRepository;
        private IGenericRepository<InstructorFeedback>? _instructorFeedbackRepository;
        private IGenericRepository<LearningMaterial>? _learningMaterialRepository;
        private IGenericRepository<LearningProgress>? _learningProgressRepository;
        private IGenericRepository<MaterialAuthor>? _materialAuthorRepository;
        private IGenericRepository<Practice>? _practiceRepository;
        private IGenericRepository<PracticeAttempt>? _practiceAttemptRepository;
        private IGenericRepository<PracticeAttemptTask>? _practiceAttemptTaskRepository;
        private IGenericRepository<PracticeTask>? _practiceTaskRepository;
        private IGenericRepository<ProgramCourse>? _programCourseRepository;
        private IGenericRepository<Quiz>? _quizRepository;
        private IGenericRepository<QuizAttempt>? _quizAttemptRepository;
        private IGenericRepository<QuizAttemptAnswer>? _quizAttemptAnswerRepository;
        private IGenericRepository<QuizAttemptQuestion>? _quizAttemptQuestionRepository;
        private IGenericRepository<QuizAuthor>? _quizAuthorRepository;
        private IGenericRepository<QuizQuestion>? _quizQuestionRepository;
        private IGenericRepository<QuizQuestionOption>? _quizQuestionOptionRepository;
        private IGenericRepository<Section>? _sectionRepository;
        private IGenericRepository<SectionActivity>? _sectionActivityRepository;
        private IGenericRepository<SectionRecord>? _sectionRecordRepository;
        private IGenericRepository<SimTask>? _simTaskRepository;
        private IGenericRepository<SimulationComponent>? _simulationComponentRepository;
        private IGenericRepository<SimulationManager>? _simulationManagerRepository;
        private IGenericRepository<Trainee>? _traineeRepository;
        private IGenericRepository<TraineeCertificate>? _traineeCertificateRepository;
        private IGenericRepository<TraineeProfile>? _traineeProfileRepository;
        private IGenericRepository<TrainingProgram>? _programRepository;
        private IGenericRepository<Timeslot>? _timeslotRepository;
        private IGenericRepository<Attendance>? _attendanceRepository;
        private IGenericRepository<ActivitySession>? _activitySessionRepository;
        private IGenericRepository<FinalExam>? _finalExamRepository;
        private IGenericRepository<FinalExamPartial>? _finalExamPartialRepository;
        private IGenericRepository<FeSimulation>? _feSimulationRepository;
        private IGenericRepository<FeTheory>? _feTheoryRepository;
        private IGenericRepository<User>? _userRepository;

        public UnitOfWork(LssctcDbContext context)
        {
            _context = context;
        }

        public IGenericRepository<Activity> ActivityRepository =>
            _activityRepository ??= new GenericRepository<Activity>(_context);

        public IGenericRepository<ActivityMaterial> ActivityMaterialRepository =>
            _activityMaterialRepository ??= new GenericRepository<ActivityMaterial>(_context);

        public IGenericRepository<ActivityPractice> ActivityPracticeRepository =>
            _activityPracticeRepository ??= new GenericRepository<ActivityPractice>(_context);

        public IGenericRepository<ActivityQuiz> ActivityQuizRepository =>
            _activityQuizRepository ??= new GenericRepository<ActivityQuiz>(_context);

        public IGenericRepository<ActivityRecord> ActivityRecordRepository =>
            _activityRecordRepository ??= new GenericRepository<ActivityRecord>(_context);

        public IGenericRepository<Admin> AdminRepository =>
            _adminRepository ??= new GenericRepository<Admin>(_context);

        public IGenericRepository<BrandModel> BrandModelRepository =>
            _brandModelRepository ??= new GenericRepository<BrandModel>(_context);

        public IGenericRepository<Certificate> CertificateRepository =>
            _certificateRepository ??= new GenericRepository<Certificate>(_context);

        public IGenericRepository<Class> ClassRepository =>
            _classRepository ??= new GenericRepository<Class>(_context);

        public IGenericRepository<ClassCode> ClassCodeRepository =>
            _classCodeRepository ??= new GenericRepository<ClassCode>(_context);

        public IGenericRepository<ClassInstructor> ClassInstructorRepository =>
            _classInstructorRepository ??= new GenericRepository<ClassInstructor>(_context);

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

        public IGenericRepository<CourseSection> CourseSectionRepository =>
            _courseSectionRepository ??= new GenericRepository<CourseSection>(_context);

        public IGenericRepository<Enrollment> EnrollmentRepository =>
            _enrollmentRepository ??= new GenericRepository<Enrollment>(_context);

        public IGenericRepository<Instructor> InstructorRepository =>
            _instructorRepository ??= new GenericRepository<Instructor>(_context);

        public IGenericRepository<InstructorProfile> InstructorProfileRepository =>
            _instructorProfileRepository ??= new GenericRepository<InstructorProfile>(_context);

        public IGenericRepository<InstructorFeedback> InstructorFeedbackRepository =>
            _instructorFeedbackRepository ??= new GenericRepository<InstructorFeedback>(_context);

        public IGenericRepository<LearningMaterial> LearningMaterialRepository =>
            _learningMaterialRepository ??= new GenericRepository<LearningMaterial>(_context);

        public IGenericRepository<LearningProgress> LearningProgressRepository =>
            _learningProgressRepository ??= new GenericRepository<LearningProgress>(_context);

        public IGenericRepository<MaterialAuthor> MaterialAuthorRepository =>
            _materialAuthorRepository ??= new GenericRepository<MaterialAuthor>(_context);

        public IGenericRepository<Practice> PracticeRepository =>
            _practiceRepository ??= new GenericRepository<Practice>(_context);

        public IGenericRepository<PracticeAttempt> PracticeAttemptRepository =>
            _practiceAttemptRepository ??= new GenericRepository<PracticeAttempt>(_context);

        public IGenericRepository<PracticeAttemptTask> PracticeAttemptTaskRepository =>
            _practiceAttemptTaskRepository ??= new GenericRepository<PracticeAttemptTask>(_context);

        public IGenericRepository<PracticeTask> PracticeTaskRepository =>
            _practiceTaskRepository ??= new GenericRepository<PracticeTask>(_context);

        public IGenericRepository<ProgramCourse> ProgramCourseRepository =>
            _programCourseRepository ??= new GenericRepository<ProgramCourse>(_context);

        public IGenericRepository<Quiz> QuizRepository =>
            _quizRepository ??= new GenericRepository<Quiz>(_context);

        public IGenericRepository<QuizAttempt> QuizAttemptRepository =>
            _quizAttemptRepository ??= new GenericRepository<QuizAttempt>(_context);

        public IGenericRepository<QuizAttemptAnswer> QuizAttemptAnswerRepository =>
            _quizAttemptAnswerRepository ??= new GenericRepository<QuizAttemptAnswer>(_context);

        public IGenericRepository<QuizAttemptQuestion> QuizAttemptQuestionRepository =>
            _quizAttemptQuestionRepository ??= new GenericRepository<QuizAttemptQuestion>(_context);

        public IGenericRepository<QuizAuthor> QuizAuthorRepository =>
            _quizAuthorRepository ??= new GenericRepository<QuizAuthor>(_context);

        public IGenericRepository<QuizQuestion> QuizQuestionRepository =>
            _quizQuestionRepository ??= new GenericRepository<QuizQuestion>(_context);

        public IGenericRepository<QuizQuestionOption> QuizQuestionOptionRepository =>
            _quizQuestionOptionRepository ??= new GenericRepository<QuizQuestionOption>(_context);

        public IGenericRepository<Section> SectionRepository =>
            _sectionRepository ??= new GenericRepository<Section>(_context);

        public IGenericRepository<SectionActivity> SectionActivityRepository =>
            _sectionActivityRepository ??= new GenericRepository<SectionActivity>(_context);

        public IGenericRepository<SectionRecord> SectionRecordRepository =>
            _sectionRecordRepository ??= new GenericRepository<SectionRecord>(_context);

        public IGenericRepository<SimTask> SimTaskRepository =>
            _simTaskRepository ??= new GenericRepository<SimTask>(_context);

        public IGenericRepository<SimulationComponent> SimulationComponentRepository =>
            _simulationComponentRepository ??= new GenericRepository<SimulationComponent>(_context);

        public IGenericRepository<SimulationManager> SimulationManagerRepository =>
            _simulationManagerRepository ??= new GenericRepository<SimulationManager>(_context);

        public IGenericRepository<Trainee> TraineeRepository =>
            _traineeRepository ??= new GenericRepository<Trainee>(_context);

        public IGenericRepository<TraineeCertificate> TraineeCertificateRepository =>
            _traineeCertificateRepository ??= new GenericRepository<TraineeCertificate>(_context);

        public IGenericRepository<TraineeProfile> TraineeProfileRepository =>
            _traineeProfileRepository ??= new GenericRepository<TraineeProfile>(_context);

        public IGenericRepository<TrainingProgram> ProgramRepository =>
            _programRepository ??= new GenericRepository<TrainingProgram>(_context);
        public IGenericRepository<Timeslot> TimeslotRepository =>
            _timeslotRepository ??= new GenericRepository<Timeslot>(_context);
        public IGenericRepository<Attendance> AttendanceRepository =>
            _attendanceRepository ??= new GenericRepository<Attendance>(_context);
        public IGenericRepository<FinalExam> FinalExamRepository =>
            _finalExamRepository ??= new GenericRepository<FinalExam>(_context);
        public IGenericRepository<FinalExamPartial> FinalExamPartialRepository =>
            _finalExamPartialRepository ??= new GenericRepository<FinalExamPartial>(_context);
        public IGenericRepository<FeSimulation> FeSimulationRepository =>
            _feSimulationRepository ??= new GenericRepository<FeSimulation>(_context);
        public IGenericRepository<FeTheory> FeTheoryRepository =>
            _feTheoryRepository ??= new GenericRepository<FeTheory>(_context);
        public IGenericRepository<ActivitySession> ActivitySessionRepository =>
            _activitySessionRepository ??= new GenericRepository<ActivitySession>(_context);
        public IGenericRepository<User> UserRepository =>
            _userRepository ??= new GenericRepository<User>(_context);

        public async Task<int> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync();
        }

        public DbContext GetDbContext()
        {
            return _context;
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}